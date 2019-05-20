using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Xml;
using System.Xml.Serialization;

public class TreasureIncreaseEvent : EventBase
{
	public TreasureData data;

	public TreasureIncreaseEvent()
	{
		eventType = EVENTTYPE.TREASUREINCREASE;
	}

	public TreasureIncreaseEvent(TreasureData newData)
	{
		eventType = EVENTTYPE.TREASUREINCREASE;
		data = newData;
	}
}

public class TreasureChangedEvent : EventBase
{
	public TreasureData data;
	public int totalTreasure;

	public TreasureChangedEvent()
	{
		eventType = EVENTTYPE.TREASURECHANGED;
	}

	public TreasureChangedEvent(TreasureData newData, int total)
	{
		eventType = EVENTTYPE.TREASURECHANGED;
		data = newData;
		totalTreasure = total;
	}
}

public class TreasureOverrideValueEvent : EventBase
{
	public int curVal;

	public TreasureOverrideValueEvent()
	{
		eventType = EVENTTYPE.TREASUREOVERRIDEVALUEEVENT;
	}
}

[XmlRoot("TreasureData")]
[System.Serializable]
public class TreasureData : ISerialize
{
	[XmlAttribute("Name")]
	public string name;
	[XmlAttribute("Value")]
	public int value;
	[XmlIgnore]
	public Sprite img;
	[XmlAttribute("Rarity")]
	public float rarity;
	[XmlAttribute("SpriteName")]
	[System.NonSerialized]
	public string spritePath;

	int m_adjustedValue;

	public TreasureData()
	{
		name = "NewTreasure";
		value = 10;
		img = null;
		rarity = 10;
		spritePath = "";
		m_adjustedValue = 0;
	}

	public TreasureData(TreasureData copyData)
	{
		name = copyData.name;
		value = copyData.value;
		img = copyData.img;
		rarity = copyData.rarity;
		spritePath = copyData.spritePath;
		m_adjustedValue = 0;
	}
		
	public void OnBeforeSerialize()
	{
		#if UNITY_EDITOR
		if(img)
		{
			spritePath = AssetDatabase.GetAssetPath(img);
			string[] splitPath = spritePath.Split(new string[] {"Resources/"}, System.StringSplitOptions.None);
			spritePath = splitPath[splitPath.Length-1];
			spritePath = spritePath.Replace(".png", "");
		}
		#endif
	}

	public void OnAfterDeserialize()
	{
		img = Resources.Load<Sprite>(spritePath);
	}

	public int GetValue()
	{
		if(m_adjustedValue == 0)
		{
			TreasureOverrideValueEvent treasureOverValEvent = new TreasureOverrideValueEvent();
			treasureOverValEvent.curVal = value;

			EventSys.BroadcastEvent(null, treasureOverValEvent);

			m_adjustedValue = treasureOverValEvent.curVal;
		}

		return m_adjustedValue;
	}
}

public class TreasureCollector : MonoBehaviour, EventInterface
{
	public GameObject treasureIncObj;
	int m_treasureCollected;
	int m_lastIncAmount;
	TreasureData m_lastTreasure;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TREASUREINCREASE);
	}

	// Use this for initialization
	void Start () 
	{
		m_treasureCollected = 0;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TREASUREINCREASE);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.TREASUREINCREASE))
		{
			TreasureIncreaseEvent treasureIncEvent = data as TreasureIncreaseEvent;

			m_lastTreasure = treasureIncEvent.data;
			IncreaseTreasure(treasureIncEvent.data);
		}
	}

	public void IncreaseTreasure(TreasureData data)
	{
		m_lastIncAmount = Mathf.Abs(data.GetValue());
		m_treasureCollected += m_lastIncAmount;

		if(treasureIncObj)
		{
			SpawnTreasureObj();
		}

		EventSys.BroadcastEvent(gameObject, new TreasureChangedEvent(data, m_treasureCollected));
	}

	void SpawnTreasureObj()
	{
		GameObject newTreasureObj = Instantiate(treasureIncObj, transform.position, Quaternion.identity) as GameObject;

		if(newTreasureObj)
		{
			EventSys.BroadcastEvent(gameObject, newTreasureObj, new TreasureIncreaseEvent(m_lastTreasure));
		}
	}
}
