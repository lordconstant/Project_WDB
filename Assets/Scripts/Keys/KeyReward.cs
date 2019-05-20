using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyReward : MonoBehaviour, EventInterface
{
	public Key key;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.DIG);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.DIG);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.DIG))
		{
			DigEvent digEvent = data as DigEvent;

			digEvent.hadReward = true;

			GrantKeyEvent grantKeyEvent = new GrantKeyEvent();
			grantKeyEvent.key = key;

			EventSys.BroadcastEvent(gameObject, grantKeyEvent);

			if(!key.spawnObject)
				return;

			GameObject newKeyObj = Instantiate(key.spawnObject, transform.position, Quaternion.identity) as GameObject;

			MeshRenderer renderer = newKeyObj.GetComponentInChildren<MeshRenderer>();

			if(renderer != null)
				renderer.material = key.keyMat;
		}
	}

	public void SetKey(Key newKey)
	{
		key = newKey;
	}
}
