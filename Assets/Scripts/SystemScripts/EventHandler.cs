using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//Add any new event types here
//Make sure to create an Event for the type
public enum EVENTTYPE { MOVE = 0, LEVELLOADED, 
	PREGAMESTART, GAMESTART, STARTLEVEL, FINISHEDSTARTING, ISSTARTING,
	PREGAMEEND, GAMEEND, ENDLEVEL, FINISHEDENDING,
	STARTPROGRESS, PROGRESS, ADDTOPROGRESS, PROGRESSED, ENDPROGRESS, ISPROGRESSING,
	MOVEDTO, MOVEDFROM, UPDATETILE,
	GETTRAVERSEFLAGS, TRAVERSEBEGIN, TRAVERSEEND,
	DIG, ARRIVED, HEALTHCHANGED, DAMAGE, TREASUREINCREASE, TREASURECHANGED, FADE, FADEFINISH, BUSY, 
	BOMBTIMERSTART, GETREQTREASURE, GETLEVELDATA, TILEBOUNCEDATA, MOVETILEUPDATE, THROWOFF, BOMBTIMEROVERRIDE,
	DIGENEMYSPAWNCOUNT, TREASUREOVERRIDEVALUEEVENT, SETDIFFICULTYEVENT, GETTILES, GRANTKEY, CANMOVETO, CANINTERACT,
	INTERACT, HASKEY, REMOVEKEY, MODELSWAP, NOREWARD, SCOREACHIEVED, EXITINTERACT, INDICATORCLICK, REQUESTSCENECHANGE, FINISHSCENECHANGE, ANIMATION,
};

//Inherit from this to be able to receive events
public interface EventInterface
{
	void EventReceive(GameObject go, EventBase data);
};
	
public static class EventSys
{
	//Used for sending messages to a specific recipient
	static Dictionary<GameObject, Dictionary<EVENTTYPE, List<EventInterface>>> m_eventHandlers = new Dictionary<GameObject, Dictionary<EVENTTYPE, List<EventInterface>>>();
	//Holds all event registered listeners, used for public calls
	static Dictionary<EVENTTYPE, List<EventInterface>> m_eventObjs = new Dictionary<EVENTTYPE, List<EventInterface>>();

	//Register a public only listener
	public static void RegisterDelegate(EventInterface eventInter, EVENTTYPE type)
	{
		RegisterDelegate(null, eventInter, type);
	}

	//Register a private & public listener
	public static void RegisterDelegate(GameObject go, EventInterface eventInter, EVENTTYPE type)
	{
		if(!m_eventObjs.ContainsKey(type))
			m_eventObjs.Add(type, new List<EventInterface>());

		m_eventObjs[type].Add(eventInter);

		if(!go)
			return;

		if(!m_eventHandlers.ContainsKey(go))
			m_eventHandlers.Add(go, new Dictionary<EVENTTYPE, List<EventInterface>>());

		if(!m_eventHandlers[go].ContainsKey(type))
			m_eventHandlers[go].Add(type, new List<EventInterface>());

		m_eventHandlers[go][type].Add(eventInter);
	}

	//Remove a public listener
	public static void UnRegisterDelegate(EventInterface eventInter, EVENTTYPE type)
	{
		UnRegisterDelegate(null, eventInter, type);
	}

	//Remove a public & private listener
	public static void UnRegisterDelegate(GameObject go, EventInterface eventInter, EVENTTYPE type)
	{
		m_eventObjs[type].Remove(eventInter);

		if(!go)
			return;

		m_eventHandlers[go][type].Remove(eventInter);

		if(m_eventHandlers[go][type].Count > 0)
			return;

		m_eventHandlers[go].Remove(type);

		if(m_eventHandlers[go].Count > 0)
			return;

		m_eventHandlers.Remove(go);
	}

	//Send a public event
	public static void BroadcastEvent(GameObject sentFrom, EventBase eventData)
	{
		BroadcastEvent(sentFrom, null, eventData);
	}

	//Send a private event is sendTo is specified, send a public event otherwise
	public static void BroadcastEvent(GameObject sentFrom, GameObject sendTo, EventBase eventData)
	{
		List<EventInterface> curDeles = null;

		//Grab specific targets listeners
		if(sendTo)
		{
			if(m_eventHandlers.ContainsKey(sendTo))
			if(m_eventHandlers[sendTo].ContainsKey(eventData.GetEventType()))
				curDeles = m_eventHandlers[sendTo][eventData.GetEventType()];
		}
		//Grab any registered listeners
		else
		{
			if(m_eventObjs.ContainsKey(eventData.GetEventType()))
				curDeles = m_eventObjs[eventData.GetEventType()];
		}

		if(curDeles == null)
			return;

		for(int i = 0; i < curDeles.Count; i++)
		{
			curDeles[i].EventReceive(sentFrom, eventData);
		}
	}
}

//All events need to inherit from this
//Set eventType in the constructor
public abstract class EventBase
{
	protected EVENTTYPE eventType;

	public bool IsTypeOfEvent(EVENTTYPE type)
	{
		return eventType == type;
	}

	public EVENTTYPE GetEventType()
	{
		return eventType;
	}
}

public class MoveEvent : EventBase
{
	public ObjectLink moveLink;

	public MoveEvent()
	{
		eventType = EVENTTYPE.MOVE;
	}
}
	
public class DamageEvent : EventBase
{
	public int damage;
	public DAMAGETYPE dmgType;

	public DamageEvent()
	{
		eventType = EVENTTYPE.DAMAGE;
		dmgType = DAMAGETYPE.DEFAULT;
	}
}

public class BusyEvent : EventBase
{
	public bool isBusy;
	public List<MonoBehaviour> busyScripts;

	public BusyEvent()
	{
		eventType = EVENTTYPE.BUSY;
		busyScripts = new List<MonoBehaviour>();
	}
}
