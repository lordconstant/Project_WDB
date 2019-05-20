using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitInteractEvent : EventBase
{
	public ExitInteractEvent()
	{
		eventType = EVENTTYPE.EXITINTERACT;
	}
}

public class ExitInteraction : MonoBehaviour, EventInterface
{
	public GameObject effectObject;
	bool m_canExit;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.SCOREACHIEVED);

		if(effectObject)
			effectObject.SetActive(false);
		m_canExit = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.CANINTERACT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.SCOREACHIEVED);

	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.CANINTERACT))
		{
			CanInteractEvent canInteract = data as CanInteractEvent;

			if(!m_canExit)
				canInteract.denyInteract = true;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.INTERACT))
		{
			EventSys.BroadcastEvent(gameObject, new ExitInteractEvent());
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.SCOREACHIEVED))
		{
			m_canExit = true;
			if(effectObject)
				effectObject.SetActive(true);
		}
	}
}
