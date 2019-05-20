using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureReward : MonoBehaviour, EventInterface
{
	public TreasureData treasure;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.DIG);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.DIG);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.DIG))
		{
			DigEvent digEvent = data as DigEvent;

			digEvent.hadReward = true;

			TreasureIncreaseEvent treasureIncEvent = new TreasureIncreaseEvent();
			treasureIncEvent.data = treasure;

			EventSys.BroadcastEvent(gameObject, treasureIncEvent);
		}
	}

	public void SetTreasure(TreasureData newTreasure)
	{
		treasure = newTreasure;
	}
}
