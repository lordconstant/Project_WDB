using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthChangedEvent : EventBase
{
	public int maxHealth;
	public int health;

	public HealthChangedEvent()
	{
		eventType = EVENTTYPE.HEALTHCHANGED;
	}
}

public class Health : MonoBehaviour, EventInterface
{
	public int maxHealth;

	int curHealth;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.DAMAGE);
	}

	// Use this for initialization
	void Start () 
	{
		curHealth = maxHealth;
		HealthChangedEvent healthChangeEvent = new HealthChangedEvent();
		healthChangeEvent.maxHealth = maxHealth;
		healthChangeEvent.health = curHealth;

		EventSys.BroadcastEvent(gameObject, healthChangeEvent);
	}
		
	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.DAMAGE);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.DAMAGE))
		{
			DamageEvent damageData = data as DamageEvent;

			DealDamage(damageData.damage);
		}
	}

	public void DealDamage(int damage)
	{
		curHealth -= Mathf.Abs(damage);

		if(curHealth < 0)
			curHealth = 0;

		HealthChangedEvent healthChangeEvent = new HealthChangedEvent();
		healthChangeEvent.maxHealth = maxHealth;
		healthChangeEvent.health = curHealth;

		EventSys.BroadcastEvent(gameObject, healthChangeEvent);
	}
}
