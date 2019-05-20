using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuInteraction : MonoBehaviour, EventInterface
{
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.INTERACT);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.INTERACT))
		{
			TreasureData newTreasureData = new TreasureData();
			newTreasureData.value = 1000;
			EventSys.BroadcastEvent(gameObject, new TreasureChangedEvent(newTreasureData, 1000));
		}
	}
}
