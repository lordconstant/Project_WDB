using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasKeyEvent : EventBase
{
	public Key requiredKey;
	public bool hasKey;

	public HasKeyEvent()
	{
		eventType = EVENTTYPE.HASKEY;
		requiredKey = null;
		hasKey = false;
	}
}

public class GrantKeyEvent : EventBase
{
	public Key key;

	public GrantKeyEvent()
	{
		eventType = EVENTTYPE.GRANTKEY;
		key = null;
	}
}

public class RemovekeyEvent : EventBase
{
	public Key key;

	public RemovekeyEvent()
	{
		eventType = EVENTTYPE.REMOVEKEY;
		key = null;
	}
}

public class KeyCollector : MonoBehaviour, EventInterface
{
	public List<Key> m_keys = new List<Key>();

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GRANTKEY);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.HASKEY);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.REMOVEKEY);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GRANTKEY);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.HASKEY);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.REMOVEKEY);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.GRANTKEY))
		{
			GrantKeyEvent grantKeyEvent = data as GrantKeyEvent;

			m_keys.Add(grantKeyEvent.key);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.HASKEY))
		{
			HasKeyEvent hasKeyEvent = data as HasKeyEvent;

			if(m_keys.Contains(hasKeyEvent.requiredKey))
			{
				hasKeyEvent.hasKey = true;
			}
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.REMOVEKEY))
		{
			RemovekeyEvent removeKey = data as RemovekeyEvent;

			m_keys.Remove(removeKey.key);
		}
	}
}
