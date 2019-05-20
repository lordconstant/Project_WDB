using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNode : MonoBehaviour 
{
	public LEVELNAMES level;
	public GameObject hoverObj;

	LevelData m_data;

	// Use this for initialization
	void Awake () 
	{
		hoverObj.SetActive(false);
		m_data = Serializer.Load<LevelData>("LevelData/" + level.ToString() + ".xml");
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void OnMouseDown()
	{
		IsStartingEvent isStartingEvent = new IsStartingEvent();
		EventSys.BroadcastEvent(gameObject, isStartingEvent);

		if(isStartingEvent.waitingToStart)
			return;

		EventSys.BroadcastEvent(gameObject, new EndLevelEvent());
		EventSys.BroadcastEvent(gameObject, new RequestSceneChangeEvent(m_data.levelName));
	}

	void OnMouseEnter()
	{
		if(hoverObj)
			hoverObj.SetActive(true);
	}

	void OnMouseExit()
	{
		if(hoverObj)
			hoverObj.SetActive(false);
	}
}
