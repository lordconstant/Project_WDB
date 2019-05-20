using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour, EventInterface
{
	public int damage;

	#if UNITY_EDITOR
	[SerializeField] [EnumFlagsAttribute]
	#endif
	public TRAVERSEFLAGS traverseFlags;
	public LINKFLAGS linkFlags;
	TileTracker m_tileTracker;
	bool m_delayTurn;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ARRIVED);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.DAMAGE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.ADDTOPROGRESS);
	}

	void Start()
	{
		m_tileTracker = GetComponent<TileTracker>();

		Invoke("AddEnemyDelayed", Time.deltaTime);

		GetTraverseFlags getTraverse = new GetTraverseFlags();
		EventSys.BroadcastEvent(gameObject, gameObject, getTraverse);
		linkFlags = getTraverse.flags;
	}

	void OnDestroy()
	{
		EnemySystem.RemoveEnemy(gameObject);

		EndProgressEvent endProgressEvent = new EndProgressEvent();
		endProgressEvent.endScript = this;

		EventSys.BroadcastEvent(gameObject, endProgressEvent);

		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ARRIVED);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.DAMAGE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.ADDTOPROGRESS);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.ARRIVED))
		{
			AttemptAttack();

			EndProgressEvent endProgressEvent = new EndProgressEvent();
			endProgressEvent.endScript = this;

			EventSys.BroadcastEvent(gameObject, endProgressEvent);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.DAMAGE))
		{
			DamageEvent dmgEvent = data as DamageEvent;

			switch(dmgEvent.dmgType)
			{
			case DAMAGETYPE.DEFAULT:
				{
					Destroy(gameObject);
				}
				break;
			case DAMAGETYPE.EXPLOSION:
				{
					ThrowOffData throwData = new ThrowOffData();
					throwData.fromPos = go.transform.position;
					EventSys.BroadcastEvent(go, gameObject, throwData);
				}
				break;
			}
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.ADDTOPROGRESS))
		{
			AddToProgressEvent addToProgressData = data as AddToProgressEvent;

			if(addToProgressData.progressEvent != null)
			{
				addToProgressData.progressEvent.waitForScripts.Add(this);
			}
		}
	}

	public void AddEnemyDelayed()
	{
		EnemySystem.AddNewEnemy(gameObject);
	}

	public void AttemptAttack()
	{
		if(m_tileTracker == null)
			return;
		
		if(m_tileTracker.curTile == null)
			return;

		if(m_tileTracker.curTile.obstacle != null)
			return;
		
		GameObject playerTile = m_tileTracker.curTile.IsPlayerTileLinked();

		if(playerTile == null)
			return;
		
		DamageEvent damageEvent = new DamageEvent();
		damageEvent.damage = damage;

		EventSys.BroadcastEvent(gameObject, playerTile, damageEvent);

		Destroy(gameObject);
	}

	public bool CanMove()
	{
		if(!m_tileTracker)
			return false;

		if(!m_tileTracker.curTile)
			return false;
		
		return !m_tileTracker.curTile.OccupantRooted();
	}
}
