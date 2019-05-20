using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeEvent : EventBase
{
	public GameObject requester;
	public bool fading;
	public bool fadeIn;
	public float fadeTime;
	public Color fadeColour;

	public FadeEvent()
	{
		eventType = EVENTTYPE.FADE;
		fadeTime = 0.5f;
		fadeIn = false;
		fadeColour = Color.black;
	}
}

public class FadeFinishEvent : EventBase
{
	public FadeFinishEvent()
	{
		eventType = EVENTTYPE.FADEFINISH;
	}
}

public class ScreenTransitioner : MonoBehaviour, EventInterface
{
	public Image m_transitionImage;
	GameObject m_requester;
	GameObject m_from;
	GameObject m_to;
	float m_transitionTime;
	float m_curTransitionTime;
	Vector2 m_transitionDir;
	bool m_transitioning;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.FADE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.FADEFINISH);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.FADE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.FADEFINISH);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(m_transitionImage == null)
			return;
		
		if(data.IsTypeOfEvent(EVENTTYPE.FADE))
		{
			FadeEvent fadeData = data as FadeEvent;

			fadeData.fading = true;

			if(m_curTransitionTime > 0.0f)
				return;

			m_requester = fadeData.requester ? fadeData.requester : go;
			m_transitionTime = fadeData.fadeTime;
			m_curTransitionTime = m_transitionTime;
			m_transitioning = fadeData.fadeIn;

			m_transitionImage.color = fadeData.fadeColour;

			StartCoroutine("FadeCoroutine");
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.FADEFINISH))
		{
			if(!m_transitioning)
			{
				FadeEvent fadeEvent = new FadeEvent();
				fadeEvent.fadeIn = true;
				EventSys.BroadcastEvent(gameObject, gameObject, fadeEvent);
			}
		}
	}

	public IEnumerator FadeCoroutine()
	{
		while(m_curTransitionTime > 0.0f)
		{
			m_curTransitionTime -= Time.deltaTime;

			if(m_curTransitionTime < 0.0f)
				m_curTransitionTime = 0.0f;
			
			yield return null;

			Color fadeColour = m_transitionImage.color;
			fadeColour.a = m_transitioning ? (m_curTransitionTime / m_transitionTime) : 1.0f - (m_curTransitionTime / m_transitionTime);
			m_transitionImage.color = fadeColour;
		}

		EventSys.BroadcastEvent(m_requester, m_requester, new FadeFinishEvent());
		EventSys.BroadcastEvent(m_requester, new FadeFinishEvent());
	}
}
