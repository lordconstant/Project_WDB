using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoadedEvent : EventBase
{
	public LevelLoadedEvent()
	{
		eventType = EVENTTYPE.LEVELLOADED;
	}
}
	
//Anything we want to do before we give the player ingame control
public class PreStartGameEvent : EventBase
{
	public List<MonoBehaviour> waitForScripts = new List<MonoBehaviour>();

	public PreStartGameEvent()
	{
		eventType = EVENTTYPE.PREGAMESTART;
	}
}

public class StartLevelEvent : EventBase
{
	public StartLevelEvent()
	{
		eventType = EVENTTYPE.STARTLEVEL;
	}
}

public class StartGameEvent : EventBase
{
	public StartGameEvent()
	{
		eventType = EVENTTYPE.GAMESTART;
	}
}

public class FinishedStartEvent : EventBase
{
	public MonoBehaviour finishedScript;

	public FinishedStartEvent()
	{
		eventType = EVENTTYPE.FINISHEDSTARTING;
	}
}

public class IsStartingEvent : EventBase
{
	public bool waitingToStart;

	public IsStartingEvent()
	{
		eventType = EVENTTYPE.ISSTARTING;
		waitingToStart = false;
	}
}

public class EndLevelEvent : EventBase
{
	public EndLevelEvent()
	{
		eventType = EVENTTYPE.ENDLEVEL;
	}
}

public class PreEndGameEvent : EventBase
{
	public List<MonoBehaviour> waitForScripts = new List<MonoBehaviour>();

	public PreEndGameEvent()
	{
		eventType = EVENTTYPE.PREGAMEEND;
	}
}

public class GameEndEvent : EventBase
{
	public GameEndEvent()
	{
		eventType = EVENTTYPE.GAMEEND;
	}
}

public class FinishedEndEvent : EventBase
{
	public MonoBehaviour finishedScript;

	public FinishedEndEvent()
	{
		eventType = EVENTTYPE.FINISHEDENDING;
	}
}

public class StartProgressEvent : EventBase
{
	public StartProgressEvent()
	{
		eventType = EVENTTYPE.STARTPROGRESS;
	}
}

public class ProgressEvent : EventBase
{
	public List<MonoBehaviour> waitForScripts = new List<MonoBehaviour>();

	public ProgressEvent()
	{
		eventType = EVENTTYPE.PROGRESS;
	}
}

public class AddToProgressEvent : EventBase
{
	public ProgressEvent progressEvent;

	public AddToProgressEvent()
	{
		eventType = EVENTTYPE.ADDTOPROGRESS;
	}

	public AddToProgressEvent(ProgressEvent toAddTo)
	{
		eventType = EVENTTYPE.ADDTOPROGRESS;
		progressEvent = toAddTo;
	}
}

public class ProgressedEvent : EventBase
{
	public ProgressedEvent()
	{
		eventType = EVENTTYPE.PROGRESSED;
	}
}

public class EndProgressEvent : EventBase
{
	public MonoBehaviour endScript;

	public EndProgressEvent()
	{
		eventType = EVENTTYPE.ENDPROGRESS;
	}
}

public class IsProgressingEvent : EventBase
{
	public bool waitToProgress;

	public IsProgressingEvent()
	{
		eventType = EVENTTYPE.ISPROGRESSING;
		waitToProgress = false;
	}
}

public class GameSystem : MonoBehaviour, EventInterface
{
	List<MonoBehaviour> m_waitToProgress = new List<MonoBehaviour>();
	List<MonoBehaviour> m_waitToStart = new List<MonoBehaviour>();
	List<MonoBehaviour> m_waitToEnd = new List<MonoBehaviour>();

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.STARTLEVEL);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.FINISHEDSTARTING);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ISSTARTING);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.STARTPROGRESS);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ENDPROGRESS);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ISPROGRESSING);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ENDLEVEL);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.FINISHEDENDING);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.STARTLEVEL);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.FINISHEDSTARTING);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ISSTARTING);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.STARTPROGRESS);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ENDPROGRESS);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ISPROGRESSING);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ENDLEVEL);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.FINISHEDENDING);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.STARTLEVEL))
		{
			StartLevel();
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.FINISHEDSTARTING))
		{
			FinishedStartEvent finishedStartData = data as FinishedStartEvent;

			FinishedStarting(finishedStartData.finishedScript);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ISSTARTING))
		{
			IsStartingEvent isStartingData = data as IsStartingEvent;

			isStartingData.waitingToStart = IsStarting();
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.STARTPROGRESS))
		{
			StartProgressing();
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ENDPROGRESS))
		{
			EndProgressEvent endProgressEvent = data as EndProgressEvent;

			FinishedProgressing(endProgressEvent.endScript);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ISPROGRESSING))
		{
			IsProgressingEvent isProgressData = data as IsProgressingEvent;

			isProgressData.waitToProgress = IsProgressing();
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ENDLEVEL))
		{
			EndLevel();
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.FINISHEDENDING))
		{
			FinishedEndEvent finishEndData = data as FinishedEndEvent;

			FinishedEnding(finishEndData.finishedScript);
		}
	}

	public void StartProgressing()
	{
		if(IsProgressing())
			return;

		ProgressEvent progressEvent = new ProgressEvent();
		EventSys.BroadcastEvent(null, progressEvent);

		m_waitToProgress = progressEvent.waitForScripts;
	}

	public void FinishedProgressing(MonoBehaviour script)
	{
		if(script == null)
			return;
		
		m_waitToProgress.Remove(script);

		if(IsProgressing())
		{
			EventSys.BroadcastEvent(null, new ProgressedEvent());
		}
	}

	public bool IsProgressing()
	{
		return m_waitToProgress.Count > 0;
	}

	public void StartLevel()
	{
		PreStartGameEvent preStartEvent = new PreStartGameEvent();
		EventSys.BroadcastEvent(null, preStartEvent);

		m_waitToStart = preStartEvent.waitForScripts;
	}

	public void FinishedStarting(MonoBehaviour script)
	{
		if(script == null)
			return;
		
		m_waitToStart.Remove(script);

		if(m_waitToStart.Count <= 0)
		{
			EventSys.BroadcastEvent(null, new StartGameEvent());
		}
	}

	public bool IsStarting()
	{
		return m_waitToStart.Count > 0;
	}

	public void EndLevel()
	{
		PreEndGameEvent preEndEvent = new PreEndGameEvent();
		EventSys.BroadcastEvent(null, preEndEvent);

		m_waitToEnd = preEndEvent.waitForScripts;
	}

	public void FinishedEnding(MonoBehaviour script)
	{
		if(script == null)
			return;
		
		if(m_waitToEnd.Count <= 0)
			return;
		
		m_waitToEnd.Remove(script);

		if(m_waitToEnd.Count <= 0)
		{
			EventSys.BroadcastEvent(null, new GameEndEvent());
		}
	}
}
