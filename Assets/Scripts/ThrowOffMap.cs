using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowOffData : EventBase
{
	public Vector3 fromPos;

	public ThrowOffData()
	{
		eventType = EVENTTYPE.THROWOFF;
	}
}

public class ThrowOffMap : MonoBehaviour, EventInterface
{
	public float height;
	public float speed;

	Animator m_animator;
	TileTracker m_tileTracker;
	Vector3 m_thrownFrom;
	Vector3 m_thrownTo;
	bool m_thrown;
	float m_travelLerp;

	// Use this for initialization
	void Awake ()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.THROWOFF);
		m_thrown = false;
	}

	void Start()
	{
		m_tileTracker = GetComponent<TileTracker>();
		m_animator = GetComponentInChildren<Animator>();
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.THROWOFF);
	}

	// Update is called once per frame
	void Update () 
	{
		if(!m_thrown)
			return;

		m_travelLerp += Time.deltaTime * speed;

		float arcPos = 0.0f;

		if(m_travelLerp <= 1.0f)
			arcPos = Mathf.Sin(m_travelLerp * Mathf.PI);
		else
			arcPos = (-(m_travelLerp - 1.0f) * m_travelLerp) * 2.0f;
		
		Vector3 newPos = Vector3.zero;

		if(m_travelLerp <= 1.0f)
			newPos = m_thrownFrom + ((m_thrownTo - m_thrownFrom) * m_travelLerp);
		else
			newPos = m_thrownTo;
		
		newPos.y += arcPos * height;

		if(m_travelLerp > 1.5f)
		{
			Destroy(gameObject);
		}

		transform.position = newPos;
	}
		
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.THROWOFF))
		{
			ThrowOffData throwData = data as ThrowOffData;
			EnemyController enemyCont = GetComponent<EnemyController>();

			if(enemyCont != null)
			{
				Destroy(enemyCont);
			}

			m_thrownFrom = transform.position;
			Vector3 throwDir = transform.position - throwData.fromPos;
			if(throwDir.sqrMagnitude == 0.0f)
			{
				float xDir = Random.Range(-1.0f, 1.0f);
				float zDir = Random.Range(-1.0f, 1.0f);
				throwDir = new Vector3(xDir, 0.0f, zDir);
			}
			throwDir.Normalize();
			m_thrownTo = FindWorldEdgeInDirection(throwDir);
			m_thrown = true;
			m_travelLerp = 0.0f;

			if(m_tileTracker != null)
			{
				MovedFromEvent moveFrom = new MovedFromEvent();
				moveFrom.leaveObj = gameObject;
				EventSys.BroadcastEvent(gameObject, m_tileTracker.curTile.gameObject, moveFrom);
			}

			if(m_animator != null)
			{
				m_animator.SetBool("Thrown", true);
			}
		}
	}

	Vector3 FindWorldEdgeInDirection(Vector3 direction)
	{
		if(m_tileTracker == null)
			return transform.position;

		direction.y = 0.0f;
		direction.Normalize();
		Vector3 curPos = transform.position + direction;
		TileData curTile = m_tileTracker.curTile;
		GameObject lastFoundObj = curTile.gameObject;

		while(true)
		{
			GameObject foundObj = curTile.GetTileFromPosition(curPos);

			if(foundObj == null)
				break;

			curTile = foundObj.GetComponent<TileData>();
			lastFoundObj = foundObj;
			curPos = foundObj.transform.position + direction;
		}

		Collider objColl = lastFoundObj.GetComponent<Collider>();

		if(objColl != null)
		{
			float objRadius = objColl.bounds.size.x;
			curPos = lastFoundObj.transform.position + (direction * objRadius);
		}

		return curPos;
	}
}
