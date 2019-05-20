using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour, EventInterface
{
	public UITransitionHandler uiHandler;
	public GameObject levelUI;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);

		levelUI.SetActive(false);
	}

	void Start()
	{
		uiHandler.StartTransitionDown(levelUI);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			uiHandler.StartTransitionUp(null);
		}
	}
}
