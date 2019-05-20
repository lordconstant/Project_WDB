using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ScoreAchievedEvent : EventBase
{
	public ScoreAchievedEvent()
	{
		eventType = EVENTTYPE.SCOREACHIEVED;
	}
}

public class GetLevelDataEvent : EventBase
{
	public LevelData levelData;

	public GetLevelDataEvent()
	{
		eventType = EVENTTYPE.GETLEVELDATA;
	}

	public GetLevelDataEvent(LevelData data)
	{
		eventType = EVENTTYPE.GETLEVELDATA;
		levelData = data;
	}
}

[XmlRoot("LevelData")]
public class LevelData : ISerialize
{
	[XmlIgnore]
	public int fieldCount = 5;
	[XmlAttribute("LevelName")]
	public string levelName = "NewLevel";
	[XmlAttribute("WinScore")]
	public int winScore = 100;
	[XmlArray("TreasureArr"),XmlArrayItem("TreasureEle")]
	public TreasureData[] treasureArr;
	[XmlAttribute("MinTreasureCount")]
	public int minTreasureCount;
	[XmlAttribute("MaxTreasureCount")]
	public int maxTreasureCount;
	[XmlAttribute("LevelNumber")]
	public int levelNum;
	[XmlAttribute("NextScene")]
	public string nextScene;

	public LevelData()
	{
		levelNum = 0;
	}

	public LevelData(int num = 0)
	{
		levelNum = num;
	}

	public LevelData(LevelData copyData, int num = 0)
	{
		levelName = copyData.levelName;
		winScore = copyData.winScore;
		treasureArr = copyData.treasureArr;
		minTreasureCount = copyData.minTreasureCount;
		maxTreasureCount = copyData.maxTreasureCount;
		nextScene = copyData.nextScene;

		levelNum = num;
	}

	public void OnBeforeSerialize()
	{
		for(int i = 0; i < treasureArr.Length; i++)
		{
			treasureArr[i].OnBeforeSerialize();
		}
	}

	public void OnAfterDeserialize()
	{
		for(int i = 0; i < treasureArr.Length; i++)
		{
			treasureArr[i].OnAfterDeserialize();
		}
	}
}

public class CurrentLevelHandler : MonoBehaviour, EventInterface
{
	public string defaultNextScene = "Menu";
	public string levelSelectScene = "LevelSelect";
	public LEVELNAMES level;
	LevelData m_data;
	string m_sceneToLoad;

	// Use this for initialization
	void Awake () 
	{
		m_sceneToLoad = SceneManager.GetActiveScene().name;

		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TREASURECHANGED);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.HEALTHCHANGED);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.EXITINTERACT);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GETLEVELDATA);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);

		m_data = Serializer.Load<LevelData>("LevelData/" + level.ToString() + ".xml");

		EventSys.BroadcastEvent(gameObject, new LevelLoadedEvent());
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TREASURECHANGED);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.HEALTHCHANGED);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.EXITINTERACT);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GETLEVELDATA);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);

	}
		
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.TREASURECHANGED))
		{
			TreasureChangedEvent treasureChangedData = data as TreasureChangedEvent;

			if(treasureChangedData.totalTreasure >= m_data.winScore)
			{
				EventSys.BroadcastEvent(gameObject, new ScoreAchievedEvent());
			}
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.HEALTHCHANGED))
		{
			HealthChangedEvent healthChangeData = data as HealthChangedEvent;

			if(healthChangeData.health > 0)
				return;

			FinishLevel(false);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.EXITINTERACT))
		{
			FinishLevel(true);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.GETLEVELDATA))
		{
			GetLevelDataEvent levelDataEvent = data as GetLevelDataEvent;

			levelDataEvent.levelData = m_data;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			EventSys.BroadcastEvent(gameObject, new RequestSceneChangeEvent(m_sceneToLoad, m_sceneToLoad == defaultNextScene ? true : false));
		}
	}

	public void FinishLevel(bool won)
	{
		m_sceneToLoad = won ? m_data.nextScene : SceneManager.GetActiveScene().name;

		if(!SceneChanger.ScenesInBuild.Contains(m_sceneToLoad))
		{
			m_sceneToLoad = defaultNextScene;
		}

		EventSys.BroadcastEvent(gameObject, new EndLevelEvent());
	}
}
