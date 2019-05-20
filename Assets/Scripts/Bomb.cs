using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTimerOverride : EventBase
{
	public bool done;
	float m_time;

	public BombTimerOverride()
	{
		eventType = EVENTTYPE.BOMBTIMEROVERRIDE;
		done = false;
	}

	public void SetTime(float time)
	{
		m_time = time;
		done = true;
	}

	public float GetTime()
	{
		return m_time;
	}
}

public class BombTimerStart : EventBase
{
	public float time;

	public BombTimerStart()
	{
		eventType = EVENTTYPE.BOMBTIMERSTART;
	}

	public BombTimerStart(float timer)
	{
		eventType = EVENTTYPE.BOMBTIMERSTART;
		time = timer;
	}
}

public class Bomb : MonoBehaviour, EventInterface
{
	class BombTile
	{
		public GameObject tile;
		public TileData tileData;
		public float distNorm;
	}

	public float detonateTime;
	public float damageRadius;
	public int damage;
	public float flashSpeed;
	[Range (0.0f, 1.0f)]
	public float flashAmount;

	bool m_detonate;
	List<BombTile> m_tileArr = new List<BombTile>();

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
	}

	// Use this for initialization
	void Start () 
	{
		BombTimerOverride btOver = new BombTimerOverride();

		EventSys.BroadcastEvent(gameObject, btOver);

		if(btOver.done)
			detonateTime = btOver.GetTime();
		
		Invoke("Detonate", detonateTime);
		EventSys.BroadcastEvent(gameObject, gameObject, new BombTimerStart(detonateTime));

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);

		for(int i = 0; i < hitColliders.Length; i++)
		{
			if(hitColliders[i].tag != "Tile")
				continue;

			TileData tileData = hitColliders[i].gameObject.GetComponent<TileData>();

			if(tileData == null)
				continue;

			if(tileData.tileObj == null)
				continue;

			BombTile newBombTile = new BombTile();
			newBombTile.tile = hitColliders[i].gameObject;
			newBombTile.tileData = tileData;
			newBombTile.distNorm = (hitColliders[i].gameObject.transform.position - transform.position).magnitude / damageRadius;
			m_tileArr.Add(newBombTile);
		}

		m_detonate = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
	}

	void Update()
	{
		if(!m_detonate)
		{
			if(flashSpeed != 0.0f)
			{
				float sinVal = Mathf.Cos(Time.time * flashSpeed);
				sinVal = sinVal < 0.0f ? -sinVal : sinVal;
				sinVal *= flashAmount;
				sinVal += 1.0f - flashAmount;
				SetExplosionTileColours(sinVal);
			}

			return;
		}

		DamageEvent damageEvent = new DamageEvent();
		damageEvent.damage = damage;
		damageEvent.dmgType = DAMAGETYPE.EXPLOSION;
		TileBounceData tbEvent = new TileBounceData();
		tbEvent.bounceSpeed = 5.0f;

		for(int i = 0; i < m_tileArr.Count; i++)
		{
			tbEvent.bounceDelay = 0.1f * m_tileArr[i].distNorm;
			EventSys.BroadcastEvent(gameObject, m_tileArr[i].tile, damageEvent);
			EventSys.BroadcastEvent(gameObject, m_tileArr[i].tile, tbEvent);
		}

		SetExplosionTileColours(1.0f);
		Destroy(gameObject);
	}

	void SetExplosionTileColours(float lerpTrans)
	{
		for(int i = 0; i < m_tileArr.Count; i++)
		{
			TileData tileData = m_tileArr[i].tileData;

			if(tileData.tileObj == null)
				continue;

			Renderer tileRenderer = tileData.tileObj.GetComponent<Renderer>();

			if(tileRenderer == null)
				continue;
			
			MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
			propBlock.SetFloat("_DamagedLerp", 1.0f - lerpTrans);
			tileRenderer.SetPropertyBlock(propBlock);
		}
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			SetExplosionTileColours(1.0f);
			Destroy(gameObject);
		}
	}

	public void Detonate()
	{
		m_detonate = true;
	}
}
