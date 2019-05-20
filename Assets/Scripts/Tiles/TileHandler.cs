using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTilesEvent : EventBase
{
	public List<GameObject> allTiles;
	public List<GameObject> rewardTiles;
	public List<GameObject> rewardFreetiles;

	public GetTilesEvent()
	{
		eventType = EVENTTYPE.GETTILES;
		allTiles = null;
		rewardTiles = null;
		rewardFreetiles = null;
	}
}

public class TileHandler : MonoBehaviour, EventInterface
{
	class TransitioningTile
	{
		public float progress;
		public GameObject tile;
		public Vector3 toPos;
		public Vector3 fromPos;
	}

	public float transitionInTime = 3;
	public int transitionChunkSize = 1;
	public bool waitToStart = true;
	public AnimationCurve lerpInCurve = new AnimationCurve();

	List<GameObject> m_levelTiles;
	List<GameObject> m_rewardTiles;
	List<GameObject> m_rewardFreeTiles;

	List<GameObject> m_tilesToTransition;
	List<TransitioningTile> m_transitioningTiles;

	Coroutine m_transitionRoutine;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GETTILES);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMESTART);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);

		GameObject[] foundTiles = GameObject.FindGameObjectsWithTag("Tile");
		m_transitionRoutine = null;

		m_levelTiles = new List<GameObject>(foundTiles);
		m_rewardFreeTiles = new List<GameObject>(foundTiles);
		m_rewardTiles = new List<GameObject>();
		m_tilesToTransition = new List<GameObject>();
		m_transitioningTiles = new List<TransitioningTile>();

		for(int i = 0; i < m_levelTiles.Count; i++)
		{
			m_levelTiles[i].SetActive(false);
			m_tilesToTransition.Add(m_levelTiles[i]);
		}
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GETTILES);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMESTART);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.GETTILES))
		{
			GetTilesEvent getTilesEvent = data as GetTilesEvent;

			getTilesEvent.allTiles = m_levelTiles;
			getTilesEvent.rewardTiles = m_rewardTiles;
			getTilesEvent.rewardFreetiles = m_rewardFreeTiles;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.PREGAMESTART))
		{
			PreStartGameEvent preStartEvent = data as PreStartGameEvent;

			GameObject foundtile = m_tilesToTransition.Find(x => x.GetComponent<PlayerSpawner>());

			Vector3 spawnTilePos = Vector3.zero;

			if(foundtile)
				spawnTilePos = foundtile.transform.position;
			
			m_tilesToTransition.Sort((x, y) => (x.transform.position - spawnTilePos).sqrMagnitude.CompareTo((y.transform.position - spawnTilePos).sqrMagnitude));
			m_transitionRoutine = StartCoroutine(TransitionTiles(new Vector3(0, -1, 0), true));

			if(waitToStart)
				preStartEvent.waitForScripts.Add(this);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			PreEndGameEvent preEndEvent = data as PreEndGameEvent;
			m_tilesToTransition.Clear();
			m_transitioningTiles.Clear();

			for(int i = 0; i < m_levelTiles.Count; i++)
			{
				m_tilesToTransition.Add(m_levelTiles[i]);
			}

			GameObject foundtile = m_tilesToTransition.Find(x => x.GetComponent<TileData>() && x.GetComponent<TileData>().IsOccupantPlayer());

			Vector3 spawnTilePos = Vector3.zero;

			if(foundtile)
				spawnTilePos = foundtile.transform.position;
			
//			Vector3 centerTilePos = new Vector3(-10.0f, 0.0f, 0.0f);
//			for(int i = 0; i < m_tilesToTransition.Count; i++)
//			{
//				centerTilePos += m_tilesToTransition[i].transform.position;
//			}
//
//			centerTilePos /= m_tilesToTransition.Count;
//
			m_tilesToTransition.Sort((x, y) => (y.transform.position - spawnTilePos).sqrMagnitude.CompareTo((x.transform.position - spawnTilePos).sqrMagnitude));
			StopCoroutine(m_transitionRoutine);
			m_transitionRoutine = StartCoroutine(TransitionTiles(new Vector3(0, -1, 0), false));

			preEndEvent.waitForScripts.Add(this);
		}
	}

	IEnumerator TransitionTiles(Vector3 transDir, bool transIn)
	{
		bool transitioning = true;

		while(transitioning)
		{
			int preppedTiles = transitionChunkSize;

			while(preppedTiles > 0 &&  m_tilesToTransition.Count > 0)
			{
				TransitioningTile tileTran = new TransitioningTile();
				tileTran.tile = m_tilesToTransition[0];
				Vector3 toPos = tileTran.tile.transform.position;
				Vector3 fromPos = tileTran.tile.transform.position;
				if(transIn)
				{
					fromPos += transDir * 50.0f;
					tileTran.tile.transform.position = fromPos;
					tileTran.tile.SetActive(true);
				}
				else
				{
					toPos += transDir * 50.0f;
				}
				tileTran.fromPos = fromPos;
				tileTran.toPos = toPos;
				m_transitioningTiles.Add(tileTran);
				m_tilesToTransition.RemoveAt(0);
				preppedTiles--;
			}
//			if(m_tilesToTransition.Count > 0)
//			{
//				TransitioningTile tileTran = new TransitioningTile();
//				tileTran.tile = m_tilesToTransition[0];
//				Vector3 toPos = tileTran.tile.transform.position;
//				Vector3 fromPos = tileTran.tile.transform.position;
//				if(transIn)
//				{
//					fromPos += transDir * 50.0f;
//					tileTran.tile.transform.position = fromPos;
//					tileTran.tile.SetActive(true);
//				}
//				else
//				{
//					toPos += transDir * 50.0f;
//				}
//				tileTran.fromPos = fromPos;
//				tileTran.toPos = toPos;
//				m_transitioningTiles.Add(tileTran);
//				m_tilesToTransition.RemoveAt(0);
//			}

			if(m_transitioningTiles.Count <= 0)
				transitioning = false;
			
			Vector3 curPos;
			for(int i = 0; i < m_transitioningTiles.Count; i++)
			{
				TransitioningTile transTile = m_transitioningTiles[i];
				curPos = transTile.tile.transform.position;
				transTile.progress += Time.deltaTime / transitionInTime;
				if(transTile.progress > 1.0f)
					transTile.progress = 1.0f;
				curPos = Vector3.LerpUnclamped(transTile.fromPos, transTile.toPos, lerpInCurve.Evaluate(transTile.progress));
				MoveTileEvent moveEvent = new MoveTileEvent();
				moveEvent.pos = curPos;
				EventSys.BroadcastEvent(transTile.tile, transTile.tile, moveEvent);
				//transTile.tile.transform.position = curPos;
				if(transTile.progress >= 1.0f)
				{
					m_transitioningTiles.RemoveAt(i);

					if(m_transitioningTiles.Count > 0)
						i--;
				}
			}

			yield return null;
		}

		if(transIn)
		{
			if(waitToStart)
			{
				FinishedStartEvent finishStartEvent = new FinishedStartEvent();
				finishStartEvent.finishedScript = this;

				EventSys.BroadcastEvent(gameObject, finishStartEvent);
			}
		}
		else
		{
			FinishedEndEvent finishEndEvent = new FinishedEndEvent();
			finishEndEvent.finishedScript = this;

			EventSys.BroadcastEvent(gameObject, finishEndEvent);
		}
	}
}
