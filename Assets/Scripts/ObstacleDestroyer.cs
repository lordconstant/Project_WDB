using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDestroyer : MonoBehaviour, EventInterface
{
	TileTracker m_tileTracker;
	//bool m_destroyNextTurn;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.BUSY);
	}

	void Start () 
	{
		m_tileTracker = GetComponent<TileTracker>();
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.BUSY);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(m_tileTracker == null)
			return;
		
		if(data.IsTypeOfEvent(EVENTTYPE.BUSY))
		{
			BusyEvent busyData = data as BusyEvent;

			busyData.isBusy = m_tileTracker.curTile.obstacle != null;

			if(busyData.isBusy)
			{
				busyData.busyScripts.Add(this);
				Invoke("DestroyObstacle", 0.5f);
			}
		}
//		else if(data.IsTypeOfEvent(EVENTTYPE.PROGRESS))
//		{
//			if(!m_destroyNextTurn)
//				return;
//			
//			m_destroyNextTurn = false;
//
//			Invoke("DestroyObstacle", 0.5f);
//		}
	}

	public void DestroyObstacle()
	{
		Destroy(m_tileTracker.curTile.obstacle);

		EndProgressEvent endProgressEvent = new EndProgressEvent();
		endProgressEvent.endScript = this;
		EventSys.BroadcastEvent(gameObject, endProgressEvent);
	}
}
