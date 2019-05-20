using UnityEngine;
using System.Collections;

public class UpdateTileEvent : EventBase
{
	public TileData newTile;

	public UpdateTileEvent()
	{
		eventType = EVENTTYPE.UPDATETILE;
	}
}

public class TileTracker : MonoBehaviour, EventInterface
{
	public TileData curTile;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.UPDATETILE);
	}

	void Start()
	{
		Ray ray = new Ray(transform.position + (Vector3.up * 0.5f), Vector3.down);
		RaycastHit hit;

		if(!Physics.Raycast(ray, out hit))
			return;

		TileData tileData = hit.collider.gameObject.GetComponent<TileData>();

		if(tileData == null)
			return;

		curTile = tileData;
		tileData.occupant = gameObject;
	}
		
	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.UPDATETILE);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.UPDATETILE))
		{
			UpdateTileEvent updateTileEvent = data as UpdateTileEvent;

			if(curTile == updateTileEvent.newTile)
				return;

			if(curTile != null)
			{
				MovedFromEvent movedFromEvent = new MovedFromEvent();
				movedFromEvent.leaveObj = gameObject;

				EventSys.BroadcastEvent(gameObject, curTile.gameObject, movedFromEvent);
			}

			curTile = updateTileEvent.newTile;

			if(curTile == null)
				return;
			
			MovedToEvent movedToEvent = new MovedToEvent();
			movedToEvent.incObj = gameObject;

			EventSys.BroadcastEvent(gameObject, curTile.gameObject, movedToEvent);
		}
	}
}
