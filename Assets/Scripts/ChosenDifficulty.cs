using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDifficultyEvent : EventBase
{
	public Difficulty difficulty;

	public SetDifficultyEvent()
	{
		eventType = EVENTTYPE.SETDIFFICULTYEVENT;
	}

	public SetDifficultyEvent(Difficulty newDifficulty)
	{
		eventType = EVENTTYPE.SETDIFFICULTYEVENT;
		difficulty = newDifficulty;
	}
}

public class ChosenDifficulty : MonoBehaviour, EventInterface
{
	public Difficulty m_curDifficulty;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.BOMBTIMEROVERRIDE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.DIGENEMYSPAWNCOUNT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TREASUREOVERRIDEVALUEEVENT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.SETDIFFICULTYEVENT);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.BOMBTIMEROVERRIDE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.DIGENEMYSPAWNCOUNT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TREASUREOVERRIDEVALUEEVENT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.SETDIFFICULTYEVENT);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.BOMBTIMEROVERRIDE))
		{
			BombTimerOverride btOver = data as BombTimerOverride;
			btOver.SetTime(m_curDifficulty.bombTimer);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.DIGENEMYSPAWNCOUNT))
		{
			DigEnemySpawnOverride digESOver = data as DigEnemySpawnOverride;
			digESOver.SetSpawnCount(m_curDifficulty.wormSpawnRate);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.TREASUREOVERRIDEVALUEEVENT))
		{
			TreasureOverrideValueEvent treasureOverValEvent = data as TreasureOverrideValueEvent;
			treasureOverValEvent.curVal = Mathf.CeilToInt(treasureOverValEvent.curVal * m_curDifficulty.treasureValue);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.SETDIFFICULTYEVENT))
		{
			SetDifficultyEvent difficultyEvent = data as SetDifficultyEvent;

			SetDifficulty(difficultyEvent.difficulty);
		}
	}

	public void SetDifficulty(Difficulty difficulty)
	{
		m_curDifficulty = difficulty;
	}
}
