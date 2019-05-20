using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour, EventInterface
{
	string m_levelName;
	bool m_fadingOut;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.FADEFINISH);
		m_fadingOut = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.FADEFINISH);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.FADEFINISH))
		{
			m_fadingOut = false;
			SceneManager.LoadScene(m_levelName);
		}
	}

	public void StartLevel(string levelName)
	{
		if(m_fadingOut)
			return;
		
		m_levelName = levelName;
		m_fadingOut = true;

		FadeEvent newFadeEvent = new FadeEvent();
		EventSys.BroadcastEvent(gameObject, newFadeEvent);

		if(!newFadeEvent.fading)
		{
			SceneManager.LoadScene(m_levelName);
		}
	}

	public void SetDifficulty(Difficulty difficulty)
	{
		EventSys.BroadcastEvent(gameObject, new SetDifficultyEvent(difficulty));
	}
}
