using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBounceData : EventBase
{
	public float bounceSpeed;
	public float bounceDelay;

	public TileBounceData()
	{
		eventType = EVENTTYPE.TILEBOUNCEDATA;
	}
}

public class TileBounce : MonoBehaviour, EventInterface
{
	bool m_bouncing;
	bool m_movingUp;
	float m_bounceSpeed;
	float m_bounceDelay;
	float m_bounceLerp;
	float m_initialHeight;
	float m_tileHeight;
	TileData m_data;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TILEBOUNCEDATA);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GAMESTART);

		m_bounceLerp = 0.0f;
		m_bounceSpeed = 1.0f;
		m_bounceDelay = 0.0f;
		m_bouncing = false;
	}

	void Start()
	{
		m_data = GetComponent<TileData>();
		m_initialHeight = transform.position.y;

		if(!m_data || !m_data.tileObj)
			return;

		Renderer myRenderer = m_data.tileObj.GetComponent<Renderer>();

		if(!myRenderer)
			return;

		m_tileHeight = myRenderer.bounds.size.y;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TILEBOUNCEDATA);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GAMESTART);

	}

	// Update is called once per frame
	void Update () 
	{
		if(!m_bouncing)
		{
			if(m_bounceDelay == 0.0f)
				return;

			m_bounceDelay -= Time.deltaTime;

			if(m_bounceDelay <= 0.0f)
			{
				m_bounceDelay = 0.0f;
				m_bouncing = true;
			}

			return;
		}

		float bounceAmount = m_movingUp ? Time.deltaTime * m_bounceSpeed : Time.deltaTime * -m_bounceSpeed;
		m_bounceLerp += bounceAmount;

		if(m_data)
		{
			m_data.SetBlockedInternal(true);
			m_data.SetOccupantRooted(true);
		}

		if(m_movingUp)
		{
			if(m_bounceLerp >= 1.0f)
			{
				m_bounceLerp = 1.0f;
				m_movingUp = false;
			}
		}
		else
		{
			if(m_bounceLerp <= 0.0f)
			{
				m_bounceLerp = 0.0f;
				m_movingUp = true;
				m_bouncing = false;

				if(m_data)
				{
					m_data.SetBlockedInternal(false);
					m_data.SetOccupantRooted(false);
				}
			}
		}

		Vector3 newPos = transform.position;
		newPos.y = m_initialHeight + (m_tileHeight * m_bounceLerp);

		EventSys.BroadcastEvent(gameObject, gameObject, new MoveTileEvent(newPos));
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.TILEBOUNCEDATA))
		{
			TileBounceData tbData = data as TileBounceData;

			m_bounceSpeed = tbData.bounceSpeed;

			if(!m_bouncing)
				m_bounceDelay = tbData.bounceDelay;

			if(m_bounceDelay == 0.0f)
				m_bouncing = true;
			
			m_movingUp = true;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.GAMESTART))
		{
			m_initialHeight = transform.position.y;
		}
	}
}
