using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTraversal : MonoBehaviour, EventInterface
{
	float m_moveProg;
	float m_moveSpeed;
	float m_jumpHeight;
	ObjectLink m_curLink;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TRAVERSEBEGIN);

		m_moveProg = 0.0f;
		m_moveSpeed = 1.0f;
	}

	// Use this for initialization
	void OnDestroy ()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TRAVERSEBEGIN);
	}

	// Update is called once per frame
	void Update () 
	{
		m_moveProg += Time.deltaTime * m_moveSpeed;

		if(m_moveProg >= 1.0f)
			m_moveProg = 1.0f;

		Vector3 toDir = m_curLink.to.transform.position - m_curLink.from.transform.position;
		Vector3 jumpPos = m_curLink.from.transform.position + toDir * m_moveProg;
		jumpPos.y += Mathf.Sin(m_moveProg * Mathf.PI) * m_jumpHeight;
		transform.position = jumpPos;
		toDir.y = 0.0f;
		transform.forward = toDir.normalized;

		if(m_moveProg >= 1.0f)
		{
			EventSys.BroadcastEvent(gameObject, gameObject, new TraverseEndEvent());

			Destroy(this);
		}
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.TRAVERSEBEGIN))
		{
			TraverseBeginEvent traverseData = data as TraverseBeginEvent;
			JumpTraverseData jumpData = traverseData.data as JumpTraverseData;
			m_curLink = traverseData.link;
			m_moveSpeed = jumpData.traverseSpeed;
			m_jumpHeight = jumpData.height;
		}
	}
}
