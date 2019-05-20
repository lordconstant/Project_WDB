using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour, EventInterface
{
	public GameObject displayObject;
	public Animator displayAnimator;
	public Text treasureText;
	public Text chestText;
	public Text dugText;
	public Text killText;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ANIMATION);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ANIMATION);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			PreEndGameEvent preEndEvent = data as PreEndGameEvent;
			preEndEvent.waitForScripts.Add(this);
			displayObject.SetActive(true);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ANIMATION))
		{
			AnimationDataEvent animDataEvent = data as AnimationDataEvent;

			if(animDataEvent.animType == ANIMTYPE.END)
			{
				FinishedEndEvent finishEndEvent = new FinishedEndEvent();
				finishEndEvent.finishedScript = this;

				EventSys.BroadcastEvent(gameObject, finishEndEvent);
			}
		}
	}

	public void NextLevelButtonPress()
	{
		displayAnimator.SetTrigger("CloseUI");
	}
}
