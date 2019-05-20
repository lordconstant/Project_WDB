using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dig : MonoBehaviour, EventInterface
{
	public GameObject dugObject;
	bool m_beenDug;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);

		m_beenDug = false;
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
			CanInteractEvent canInteract = data as CanInteractEvent;

			if(m_beenDug)
				canInteract.denyInteract = true;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.INTERACT))
		{
			m_beenDug = true;

			ModelSwapEvent swapEvent = new ModelSwapEvent(dugObject);
			swapEvent.keepDecoration = false;

			EventSys.BroadcastEvent(gameObject, gameObject, swapEvent);

			DigEvent digEvent = new DigEvent();

			EventSys.BroadcastEvent(go, gameObject, digEvent);

			if(digEvent.hadReward)
				return;
			
			EventSys.BroadcastEvent(gameObject, go, new NoRewardEvent());
		}
	}
}
