using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEvents : MonoBehaviour 
{
	public UITransitionHandler uiHandler;
	public GameObject startUI;
	public GameObject endUI;

	// Use this for initialization
	void Awake () 
	{
		if(startUI)
			startUI.SetActive(false);
		if(endUI)
			endUI.SetActive(false);
	}

	void Start()
	{
		SendStartGameEvent();
		//uiHandler.StartTransitionUp(startUI);
	}

	void OnDestroy()
	{

	}

	public void SendStartGameEvent()
	{
		EventSys.BroadcastEvent(gameObject, new StartLevelEvent());
	}
}
