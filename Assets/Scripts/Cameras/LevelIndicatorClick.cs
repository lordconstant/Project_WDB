using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorClickEvent : EventBase
{
	public CameraNodeLink link;

	public IndicatorClickEvent()
	{
		eventType = EVENTTYPE.INDICATORCLICK;
		link = null;
	}

	public IndicatorClickEvent(CameraNodeLink nodeLink)
	{
		eventType = EVENTTYPE.INDICATORCLICK;
		link = nodeLink;
	}
}

public class LevelIndicatorClick : MonoBehaviour 
{
	public GameObject ownerNode;
	public CameraNodeLink link;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown()
	{
		EventSys.BroadcastEvent(gameObject, ownerNode, new IndicatorClickEvent(link));
	}
}
