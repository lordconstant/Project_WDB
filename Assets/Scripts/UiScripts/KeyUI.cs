using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour, EventInterface
{
	public class KeyCounter
	{
		public GameObject obj;
		public Key key;
	}

	public GameObject keyToken;
	public RectTransform keyHolder;
	List<KeyCounter> m_tokens;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GRANTKEY);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.REMOVEKEY);
	}

	void Start()
	{
		m_tokens = new List<KeyCounter>();
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GRANTKEY);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.REMOVEKEY);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.GRANTKEY))
		{
			GrantKeyEvent grantKeyEvent = data as GrantKeyEvent;

			CreateNewToken(grantKeyEvent.key);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.REMOVEKEY))
		{
			RemovekeyEvent removeKey = data as RemovekeyEvent;

			for(int i = 0; i < m_tokens.Count; i++)
			{
				if(m_tokens[i].key == removeKey.key)
				{
					Destroy(m_tokens[i].obj);
					m_tokens.RemoveAt(i);
					break;
				}
			}
		}
	}

	void CreateNewToken(Key key)
	{
		if(key == null)
			return;

		if(keyToken == null)
			return;

		if(keyHolder == null)
			return;

		GameObject newToken = Instantiate(keyToken, keyHolder) as GameObject;

		MeshRenderer renderer = newToken.GetComponentInChildren<MeshRenderer>();

		if(renderer)
		{
			renderer.material = key.keyMat;
		}

		KeyCounter newCounter = new KeyCounter();
		newCounter.key = key;
		newCounter.obj = newToken;
		m_tokens.Add(newCounter);
	}
}
