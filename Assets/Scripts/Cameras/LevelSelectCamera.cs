using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectCamera : MonoBehaviour, EventInterface
{
	public CameraLookNode currentNode;
	CameraNodeLink m_followLink;
	public float moveSpeed;
	bool m_following;
	float m_prog;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INDICATORCLICK);
	}

	// Use this for initialization
	void Start ()
	{
		transform.position = currentNode.transform.position;
		currentNode.SetNodeActive(true, gameObject);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.INDICATORCLICK);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.INDICATORCLICK))
		{
			IndicatorClickEvent clickEvent = data as IndicatorClickEvent;

			m_followLink = clickEvent.link;
			m_following = true;
			m_prog = 0.0f;
			currentNode.SetNodeActive(false, gameObject);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if(!m_following)
			return;

		if(m_followLink == null)
		{
			m_following = false;
			return;
		}

		m_prog += moveSpeed * Time.deltaTime;

		if(m_prog > 1.0f)
			m_prog = 1.0f;
		
		Vector3 curPoint = m_followLink.GetPointOnLink(m_prog);

		transform.position = curPoint;

		if(m_prog >= 1.0f)
		{
			currentNode = m_followLink.to.GetComponent<CameraLookNode>();

			if(currentNode != null)
				currentNode.SetNodeActive(true, gameObject);
			
			m_following = false;
			m_followLink = null;
		}
	}
}
