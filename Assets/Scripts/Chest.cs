using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour, EventInterface
{
	public Key requiredKey;
	public Animator chestAnimator;
	public GameObject lockedUI;

	bool m_opened;
	bool m_used;
	float m_showUI;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);

		m_opened = false;
		m_used = false;
		m_showUI = 0.0f;

		if(lockedUI != null)
		{
			if(requiredKey != null)
			{
				MeshRenderer renderer = lockedUI.GetComponentInChildren<MeshRenderer>();

				if(renderer != null)
					renderer.material = requiredKey.keyMat;
				
			}

			lockedUI.SetActive(false);
		}
	}

	// Use this for initialization
	void Start () 
	{
		GetTilesEvent getTiles = new GetTilesEvent();
		EventSys.BroadcastEvent(gameObject, getTiles);

		if(getTiles.rewardFreetiles == null || getTiles.rewardFreetiles.Count <= 0)
			return;

		List<GameObject> validTiles = new List<GameObject>();

		for(int i = 0; i < getTiles.rewardFreetiles.Count; i++)
		{
			TileData tileData = getTiles.rewardFreetiles[i].GetComponent<TileData>();

			if(!tileData)
				continue;

			if(!tileData.CanTraverse(0))
				continue;

			validTiles.Add(getTiles.rewardFreetiles[i]);
		}

		if(validTiles.Count <= 0)
			return;
		
		int randTile = Random.Range(0, validTiles.Count);

		GameObject keyTile = validTiles[randTile];
		KeyReward newKeyComp = keyTile.AddComponent<KeyReward>();
		newKeyComp.SetKey(requiredKey);

		getTiles.rewardFreetiles.Remove(keyTile);
		getTiles.rewardTiles.Add(keyTile);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);

	}

	void Update()
	{
		if(m_showUI <= 0.0f)
		{
			if(lockedUI)
			{
				if(lockedUI.activeSelf)
					lockedUI.SetActive(false);
			}

			return;
		}
			

		if(lockedUI)
		{
			if(!lockedUI.activeSelf)
				lockedUI.SetActive(true);
		}

		m_showUI -= Time.deltaTime;
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.CANINTERACT))
		{
			CanInteractEvent interactEvent = data as CanInteractEvent;

			HasKeyEvent hasKeyEvent = new HasKeyEvent();
			hasKeyEvent.requiredKey = requiredKey;

			EventSys.BroadcastEvent(gameObject, go, hasKeyEvent);

			if(!hasKeyEvent.hasKey && !m_used)
			{
				m_showUI = 1.0f;
				interactEvent.denyInteract = true;
			}
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.INTERACT))
		{
			if(chestAnimator)
				chestAnimator.SetBool("Open", !m_opened);

			m_opened = !m_opened;

			if(m_used)
				return;

			RemovekeyEvent removeKey = new RemovekeyEvent();
			removeKey.key = requiredKey;
			EventSys.BroadcastEvent(gameObject, removeKey);

			DigEvent digEvent = new DigEvent();

			EventSys.BroadcastEvent(go, gameObject, digEvent);

			m_used = true;
		}
	}
}
