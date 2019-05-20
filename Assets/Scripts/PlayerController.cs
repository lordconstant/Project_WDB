using UnityEngine;
using System.Collections;

public enum HIGHLIGHTCOLOURS
{
	MOVE = 0,
	BLOCKED,
	INTERACT,
	COUNT
}

public class PlayerController : MonoBehaviour, EventInterface
{
	public GameObject dirt;
	public GameObject highlightMarker;
	public Color[] highlightColours = new Color[(int)HIGHLIGHTCOLOURS.COUNT];

	ObjectLink m_storedLink;
	TileTracker m_tileTracker;
	Material m_hightlightMaterial;

	GameObject m_hightlightMarker;
	LINKFLAGS m_linkFlags;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PROGRESS);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ARRIVED);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);

		m_tileTracker = GetComponent<TileTracker>();

		EnemySystem.SetTarget(gameObject);

		m_hightlightMarker = Instantiate(highlightMarker, transform.position, Quaternion.identity) as GameObject;

		m_hightlightMaterial = m_hightlightMarker.GetComponentInChildren<Renderer>().material;
	}

	void Start()
	{
		GetTraverseFlags getTraverse = new GetTraverseFlags();
		EventSys.BroadcastEvent(gameObject, gameObject, getTraverse);
		m_linkFlags = getTraverse.flags;
	}

	void OnDestroy()
	{
		if(m_hightlightMarker)
			Destroy(m_hightlightMarker);

		EndProgressEvent endProgressEvent = new EndProgressEvent();
		endProgressEvent.endScript = this;

		EventSys.BroadcastEvent(gameObject, endProgressEvent);

		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PROGRESS);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ARRIVED);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);

	}

	// Update is called once per frame
	void Update () 
	{
		IsProgressingEvent isProgressingEvent = new IsProgressingEvent();

		EventSys.BroadcastEvent(gameObject, isProgressingEvent);

		if(isProgressingEvent.waitToProgress)
			return;
		
		if(!m_tileTracker)
			return;

		if(m_tileTracker.curTile.OccupantRooted())
			return;
		
		if(Camera.main == null)
			return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit;

		bool gotMouseInput = Input.GetMouseButtonDown(0);

		Vector3 mouseGridPos;

		if(Physics.Raycast(ray, out hit))
		{
			mouseGridPos = hit.point;
		}
		else
		{
			mouseGridPos = ray.origin + (ray.direction * -(ray.origin.y * (1.0f / ray.direction.y)));
		}

		GameObject bestTile = m_tileTracker.curTile.GetTileFromPosition(mouseGridPos, m_linkFlags);

		if(bestTile)
		{
			bool onBestTile = bestTile == m_tileTracker.curTile.gameObject;

			if(!m_hightlightMarker.activeSelf)
				m_hightlightMarker.SetActive(true);

			Vector3 highlightPos = bestTile.transform.position;
			m_hightlightMarker.transform.position = highlightPos;

			CanInteractEvent canInteract = new CanInteractEvent();
			EventSys.BroadcastEvent(gameObject, bestTile, canInteract);

			if(canInteract.Allowed())
			{
				m_hightlightMaterial.color = highlightColours[(int)HIGHLIGHTCOLOURS.INTERACT];

				if(gotMouseInput)
				{
					InteractEvent interact = new InteractEvent();
					EventSys.BroadcastEvent(gameObject, bestTile, interact);
				}

				return;
			}
			else if(onBestTile)
			{
				m_hightlightMaterial.color = highlightColours[(int)HIGHLIGHTCOLOURS.BLOCKED];
				return;
			}

			CanMoveToEvent canMoveTo = new CanMoveToEvent();
			EventSys.BroadcastEvent(gameObject, bestTile, canMoveTo);

			if(!canMoveTo.allowMove)
			{
				m_hightlightMaterial.color = highlightColours[(int)HIGHLIGHTCOLOURS.BLOCKED];
				return;
			}
				
			m_hightlightMaterial.color = highlightColours[(int)HIGHLIGHTCOLOURS.MOVE];

			if(gotMouseInput)
			{
				ObjectLink foundLink = m_tileTracker.curTile.IsLinked(bestTile);

				m_storedLink = foundLink;

				UpdateTileEvent updateTileEvent = new UpdateTileEvent();

				updateTileEvent.newTile = foundLink.to.GetComponent<TileData>();

				EventSys.BroadcastEvent(gameObject, gameObject, updateTileEvent);

				EventSys.BroadcastEvent(gameObject, new StartProgressEvent());
				return;
			}
		}
		else if(m_hightlightMarker.activeSelf)
		{
			m_hightlightMarker.SetActive(false);
		}

//		if(Input.GetKeyUp(KeyCode.Space))
//		{
//			if(AttemptDig())
//			{
//				Vector3 highlightPos = transform.position;
//				m_hightlightMarker.transform.position = highlightPos;
//			}
//		}
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.PROGRESS))
		{
			m_hightlightMaterial.color = Color.green;

			if(m_storedLink == null)
				return;

			ProgressEvent progressEvent = data as ProgressEvent;

			MoveEvent moveEvent = new MoveEvent();
			moveEvent.moveLink = m_storedLink;

			EventSys.BroadcastEvent(gameObject, gameObject, moveEvent);

			progressEvent.waitForScripts.Add(this);

			m_storedLink = null;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ARRIVED))
		{
			EndProgressEvent endProgressEvent = new EndProgressEvent();
			endProgressEvent.endScript = this;

			EventSys.BroadcastEvent(gameObject, endProgressEvent);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			Destroy(this);
		}
	}

//	public bool AttemptDig()
//	{
//		CanDigEvent canDigEvent = new CanDigEvent();
//
//		EventSystem.BroadcastEvent(gameObject, m_tileTracker.curTile.gameObject, canDigEvent);
//
//		if(canDigEvent.denyDig)
//			return false;
//
//		DigEvent digEvent = new DigEvent();
//
//		EventSystem.BroadcastEvent(gameObject, gameObject, digEvent);
//
//		return true;
//	}
}