using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ANIMTYPE { START, END };

public class AnimationDataEvent : EventBase
{
	public ANIMTYPE animType;

	public AnimationDataEvent()
	{
		eventType = EVENTTYPE.ANIMATION;
		animType = ANIMTYPE.END;
	}
}

public class AnimationEventNotifier : MonoBehaviour 
{
	public GameObject notifyGameobject;

	public void AnimationEndEvent(ANIMTYPE animType)
	{
		AnimationDataEvent dataEvent = new AnimationDataEvent();
		dataEvent.animType = animType;
		EventSys.BroadcastEvent(gameObject, notifyGameobject, dataEvent);
	}
}
