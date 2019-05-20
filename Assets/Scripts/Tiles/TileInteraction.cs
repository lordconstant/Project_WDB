using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanInteractEvent : EventBase
{
	public bool allowInteract;
	public bool denyInteract;
	public TRAVERSEFLAGS moveFlags;

	public CanInteractEvent()
	{
		eventType = EVENTTYPE.CANINTERACT;
		allowInteract = false;
		denyInteract = false;
		moveFlags = 0;
	}

	public bool Allowed()
	{
		return allowInteract && !denyInteract;
	}
}

public class InteractEvent : EventBase
{
	public InteractEvent()
	{
		eventType = EVENTTYPE.INTERACT;
	}
}

public class TileInteraction : MonoBehaviour, EventInterface
{
	public bool denyIfBlocked = false;
	public int range = 0;

	TileData m_tileData;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);
	}

	void Start()
	{
		m_tileData = GetComponent<TileData>();

		if(!m_tileData)
		{
			Debug.LogWarning("TileData is needed on" + name + "for TileInteraction to be used");
			Destroy(this);
		}
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.CANINTERACT))
		{
			CanInteractEvent interactEvent = data as CanInteractEvent;

			foreach(Transform child in transform)
			{
				EventSys.BroadcastEvent(go, child.gameObject, interactEvent);
			}

			if(m_tileData.occupant == go)
			{
				interactEvent.allowInteract = true;
				return;
			}

			if(range == 0)
			{
				interactEvent.allowInteract = false;
				return;
			}

			if(denyIfBlocked)
			{
				if(!m_tileData.CanTraverse(interactEvent.moveFlags))
				{
					interactEvent.allowInteract = false;
					return;
				}
			}

			GameObject onTile = IsInteractorInRange(range, go);

			if(onTile == null)
			{
				interactEvent.allowInteract = false;
				return;
			}

			interactEvent.allowInteract = true;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.INTERACT))
		{
			foreach(Transform child in transform)
			{
				EventSys.BroadcastEvent(go, child.gameObject, data);
			}
		}
	}

	public GameObject IsInteractorInRange(int depth, GameObject interactor)
	{
		if(depth <= 0)
			return null;

		depth--;

		GameObject retTile = m_tileData.IsTileWithOccupantLinked(interactor);

		if(retTile != null)
			return retTile;

		return IsInteractorInRange(depth, interactor);
	}
}
