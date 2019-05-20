using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RequestSceneChangeEvent : EventBase
{
	public string sceneName;
	public bool fade;

	public RequestSceneChangeEvent()
	{
		eventType = EVENTTYPE.REQUESTSCENECHANGE;
		sceneName = "";
		fade = false;
	}

	public RequestSceneChangeEvent(string newName)
	{
		eventType = EVENTTYPE.REQUESTSCENECHANGE;
		sceneName = newName;
		fade = false;
	}

	public RequestSceneChangeEvent(string newName, bool newFade)
	{
		eventType = EVENTTYPE.REQUESTSCENECHANGE;
		sceneName = newName;
		fade = newFade;
	}
}


public class SceneChanger : MonoBehaviour, EventInterface
{
	public static List<string> ScenesInBuild = new List<string>();
	AsyncOperation m_loadingOperation;

	string m_sceneToLoad;

	bool m_loadingLevel;
	bool m_nextLevelLoaded;
	bool m_fade;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.FADEFINISH);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.REQUESTSCENECHANGE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GAMEEND);

		m_loadingLevel = false;
		m_nextLevelLoaded = false;

		ScenesInBuild.Clear();

		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
			int lastSlash = scenePath.LastIndexOf("/");
			ScenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
		}
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.FADEFINISH);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.REQUESTSCENECHANGE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GAMEEND);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.FADEFINISH))
		{
			if(!m_nextLevelLoaded)
				return;
			
			if(m_loadingOperation != null)
				m_loadingOperation.allowSceneActivation = true;
			else
				SceneManager.LoadScene(m_sceneToLoad);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.REQUESTSCENECHANGE))
		{
			RequestSceneChangeEvent changeEvent = data as RequestSceneChangeEvent;

			if(GoToLevel(changeEvent.sceneName))
				m_fade = changeEvent.fade;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			PreEndGameEvent preEndEvent = data as PreEndGameEvent;
			preEndEvent.waitForScripts.Add(this);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.GAMEEND))
		{
			GoToLevel(m_sceneToLoad);
		}
	}

	public bool GoToLevel(string levelname)
	{
		if(m_loadingLevel && !m_nextLevelLoaded)
		{
			return false;
		}
		else if(m_nextLevelLoaded)
		{
			if(m_fade)
			{
				FadeEvent fadeEvent = new FadeEvent();

				EventSys.BroadcastEvent(gameObject, fadeEvent);

				if(fadeEvent.fading)
					return true;
			}

			if(m_loadingOperation != null)
				m_loadingOperation.allowSceneActivation = true;
			else
				SceneManager.LoadScene(m_sceneToLoad);
			return true;
		}

		m_sceneToLoad = levelname;

		m_loadingOperation = SceneManager.LoadSceneAsync(m_sceneToLoad);
		m_loadingOperation.allowSceneActivation = false;

		StartCoroutine(LoadingScene());

		return true;
	}

	IEnumerator LoadingScene()
	{
		m_loadingLevel = true;
		m_nextLevelLoaded = false;

		while(m_loadingOperation.progress < 0.9f)
			yield return null;

		m_loadingLevel = false;
		m_nextLevelLoaded = true;

		FinishedEndEvent finishEndEvent = new FinishedEndEvent();
		finishEndEvent.finishedScript = this;

		EventSys.BroadcastEvent(gameObject, finishEndEvent);
	}
}
