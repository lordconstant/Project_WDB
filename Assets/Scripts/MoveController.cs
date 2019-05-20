using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveController : MonoBehaviour, EventInterface
{
	public float moveSpeed = 1.0f;
	#if UNITY_EDITOR
	[SerializeField] [EnumFlagsAttribute]
	#endif
	public LINKFLAGS traverseFlags;
	public List<LinkTraversal> traversalSkills = new List<LinkTraversal>();
	Queue<ObjectLink> m_linkQueue = new Queue<ObjectLink>();
	ObjectLink m_curLink;
	float m_moveProg;
	bool m_traversing;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.MOVE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TRAVERSEEND);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GETTRAVERSEFLAGS);

		m_traversing = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.MOVE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TRAVERSEEND);
	}

	public void QueueTraverseLink(ObjectLink newLink)
	{
		m_linkQueue.Enqueue(newLink);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.MOVE))
		{
			MoveEvent moveData = data as MoveEvent;
			QueueTraverseLink(moveData.moveLink);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.TRAVERSEEND))
		{
			EventSys.BroadcastEvent(gameObject, gameObject, new ArrivedEvent());

			m_curLink = null;
			m_moveProg = 0.0f;
			m_traversing = false;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.GETTRAVERSEFLAGS))
		{
			GetTraverseFlags flagData = data as GetTraverseFlags;

			flagData.flags |= traverseFlags;
		}
	}

	void Update()
	{
		if(m_curLink == null || !m_traversing)
		{
			if(m_linkQueue.Count <= 0)
				return;
			
			m_curLink = m_linkQueue.Dequeue();
			m_traversing = false;
		}

		if(m_traversing)
			return;
		
		if(m_curLink == null)
			return;

		if(m_curLink.from == null)
			return;

		if(m_curLink.to == null)
			return;

		LinkTraversal traverseLink = traversalSkills.Find(x => x.typeOfLink == m_curLink.linkType);

		if(traverseLink != null)
		{
			gameObject.AddComponent<MoveTraversal>();

			TraverseBeginEvent traverseBegin = new TraverseBeginEvent();
			traverseBegin.link = m_curLink;
			traverseBegin.data = traverseLink.data;

			EventSys.BroadcastEvent(gameObject, gameObject, traverseBegin);
			m_traversing = true;
		}

//		if(m_curLink.linkType == LINKTYPE.WALK)
//		{
//			m_moveProg += Time.deltaTime * moveSpeed;
//
//			if(m_moveProg >= 1.0f)
//				m_moveProg = 1.0f;
//			
//			Vector3 toDir = m_curLink.to.transform.position - m_curLink.from.transform.position;
//			transform.position = m_curLink.from.transform.position + toDir * m_moveProg;
//			toDir.y = 0.0f;
//			transform.forward = toDir.normalized;
//
//			if(m_moveProg >= 1.0f)
//			{
//				EventSys.BroadcastEvent(gameObject, gameObject, new ArrivedEvent());
//
//				m_curLink = null;
//				m_moveProg = 0.0f;
//			}
//		}
	}

	public bool IsMoving()
	{
		return m_curLink != null && m_moveProg < 1.0f;
	}
}
