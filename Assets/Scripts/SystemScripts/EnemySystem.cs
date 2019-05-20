using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyData
{
	public GameObject go;
	public EnemyController controller;
	public List<TileData> path;
}

public static class EnemySystem
{
	public class EnemySystemEventListener : EventInterface
	{
		public EnemySystemEventListener()
		{
			EventSys.RegisterDelegate(this, EVENTTYPE.PROGRESS);
		}

		~EnemySystemEventListener()
		{
			EventSys.UnRegisterDelegate(this, EVENTTYPE.PROGRESS);
		}

		public void EventReceive(GameObject go, EventBase data)
		{
			EnemySystem.EventReceive(go, data);
		}
	}

	static List<EnemyData> m_enemies = new List<EnemyData>();
	static GameObject m_targetObject;
	static EnemySystemEventListener m_eventListener;

	static EnemySystem()
	{
		m_eventListener = new EnemySystemEventListener();
	}

	public static void SetTarget(GameObject target)
	{
		m_targetObject = target;
	}

	public static void AddNewEnemy(GameObject enemy)
	{
		EnemyData newEnemyData = new EnemyData();
		newEnemyData.go = enemy;
		newEnemyData.controller = enemy.GetComponent<EnemyController>();
		newEnemyData.path = AStarPathing.FindPath(enemy, m_targetObject, TRAVERSEFLAGS.OCCUPANT);

		bool added = false;

		for(int i = 0; i < m_enemies.Count; i++)
		{
			if(newEnemyData.path.Count > m_enemies[i].path.Count)
				continue;
			
			m_enemies.Insert(i, newEnemyData);

			added = true;
			break;
		}

		if(added)
			return;

		m_enemies.Add(newEnemyData);
	}

	public static void RemoveEnemy(GameObject enemy)
	{
		for(int i = 0; i < m_enemies.Count; i++)
		{
			if(m_enemies[i].go != enemy)
				continue;

			m_enemies.RemoveAt(i);

			break;
		}
	}

	public static void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.PROGRESS))
		{
			ProgressEvent progressEvent = data as ProgressEvent;

			BusyEvent busyEvent = new BusyEvent();

			for(int i = 0; i < m_enemies.Count; i++)
			{
				busyEvent.isBusy = false;

				EventSys.BroadcastEvent(m_enemies[i].go, m_enemies[i].go, busyEvent);

				if(busyEvent.isBusy)
				{
					for(int j = 0; j < busyEvent.busyScripts.Count; j++)
					{
						progressEvent.waitForScripts.Add(busyEvent.busyScripts[j]);
					}
					continue;
				}

				if(!m_enemies[i].controller.CanMove())
					continue;
				
				List<TileData> path = AStarPathing.FindPath(m_enemies[i].go, m_targetObject, m_enemies[i].controller.traverseFlags, m_enemies[i].controller.linkFlags);

				if(path.Count <= 0)
				{
					m_enemies[i].controller.AttemptAttack();
					continue;
				}

				if(path[1].occupant && !path[1].IsOccupantPlayer())
				{
					continue;
				}

				if(path[path.Count-1].IsOccupantPlayer() && path.Count <= 2)
				{
					m_enemies[i].controller.AttemptAttack();
					continue;
				}

				m_enemies[i].path = path;

				ObjectLink foundLink = path[0].IsLinked(path[1].gameObject);

				if(foundLink == null)
					continue;

				MoveEvent moveEvent = new MoveEvent();
				moveEvent.moveLink = foundLink;

				EventSys.BroadcastEvent(m_enemies[i].go, m_enemies[i].go, moveEvent);

				UpdateTileEvent updateTileEvent = new UpdateTileEvent();
				updateTileEvent.newTile = foundLink.to.GetComponent<TileData>();

				EventSys.BroadcastEvent(m_enemies[i].go, m_enemies[i].go, updateTileEvent);
				EventSys.BroadcastEvent(m_enemies[i].go, m_enemies[i].go, new AddToProgressEvent(progressEvent));
			}
		}
	}
}
