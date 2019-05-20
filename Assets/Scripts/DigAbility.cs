using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigEnemySpawnOverride : EventBase
{
	public bool done;
	int m_spawnCount;

	public DigEnemySpawnOverride()
	{
		eventType = EVENTTYPE.DIGENEMYSPAWNCOUNT;
		done = false;
	}

	public void SetSpawnCount(int count)
	{
		m_spawnCount = count;
		done = true;
	}

	public int GetSpawnCount()
	{
		return m_spawnCount;
	}
}

public class DigEvent : EventBase
{
	public bool hadReward;

	public DigEvent()
	{
		eventType = EVENTTYPE.DIG;
		hadReward = false;
	}
}

public class NoRewardEvent : EventBase
{
	public NoRewardEvent()
	{
		eventType = EVENTTYPE.NOREWARD;
	}
}
	
public class DigAbility : MonoBehaviour, EventInterface
{
	public GameObject enemyObj;
	public GameObject bombObj;
	public int enemySpawnDigCount = 3;
	public float enemyNoSpawnRadius = 3.0f;
	public float nothingChance = 0.5f;
	public float bombChance = 0.2f;

	float m_totalChance;

	TileTracker m_tileTracker;
	int m_digCount;
	float m_treasureRarityTotal;
	LevelData m_levelData;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.NOREWARD);

		m_totalChance = nothingChance + bombChance;
		m_tileTracker = GetComponent<TileTracker>();
		m_digCount = 0;
	}

	void Start()
	{
		DigEnemySpawnOverride digESOver = new DigEnemySpawnOverride();

		EventSys.BroadcastEvent(gameObject, digESOver);

		if(digESOver.done)
			enemySpawnDigCount = digESOver.GetSpawnCount();
		
		GetLevelDataEvent getLevelData = new GetLevelDataEvent();

		EventSys.BroadcastEvent(gameObject, getLevelData);

		m_levelData = getLevelData.levelData;

		m_treasureRarityTotal = 0.0f;

		if(m_levelData != null)
		{
			for(int i = 0; i < m_levelData.treasureArr.Length; i++)
			{
				m_treasureRarityTotal += m_levelData.treasureArr[i].rarity;
			}
		}
	
		SetupTreasureTiles();
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.NOREWARD);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.NOREWARD))
		{
			m_digCount++;

			float curSpawnChance = Random.Range(0.0f, 1.0f);

			curSpawnChance *= m_totalChance;

//			DigEvent digEvent = new DigEvent();
//
//			EventSystem.BroadcastEvent(gameObject, m_tileTracker.curTile.gameObject, digEvent);
//
			if(curSpawnChance > nothingChance)
			{
				SpawnObjOnCurrentTile(bombObj);
			}

			if(m_digCount % enemySpawnDigCount == 0)
			{
				SpawnObjOnRandomTile(enemyObj);
			}
		}
	}

	void SpawnObjOnRandomTile(GameObject obj)
	{
		if(!obj)
			return;
		 
		//Find a valid tile
		GetTilesEvent getTilesEvent = new GetTilesEvent();
		EventSys.BroadcastEvent(gameObject, getTilesEvent);

		if(getTilesEvent.allTiles == null || getTilesEvent.allTiles.Count <= 0)
			return;

		List<GameObject> tileList = new List<GameObject>();

		for(int i = 0; i < getTilesEvent.allTiles.Count; i++)
		{
			TileData tileData = getTilesEvent.allTiles[i].GetComponent<TileData>();

			if(!tileData)
				continue;

			if(!tileData.CanTraverse(0))
				continue;

			if(m_tileTracker.curTile.IsLinked(getTilesEvent.allTiles[i]) != null)
				continue;

			if(tileData.occupant == gameObject)
				continue;

			Vector3 tileDist = transform.position - getTilesEvent.allTiles[i].transform.position;

			if(tileDist.sqrMagnitude < (enemyNoSpawnRadius * enemyNoSpawnRadius))
				continue;
			
			tileList.Add(getTilesEvent.allTiles[i]);
		}

		if(tileList.Count <= 0)
			return;

		//We've got our valid tiles, which shall we spawn on?
		int chosenTile = Random.Range(0, tileList.Count);

		Vector3 spawnPos = tileList[chosenTile].transform.position;
		Instantiate(obj, spawnPos, Quaternion.identity);
	}

	void SpawnObjOnCurrentTile(GameObject obj)
	{
		if(obj == null)
			return;
		
		if(m_tileTracker == null)
			return;
		
		Vector3 spawnPos = m_tileTracker.curTile.gameObject.transform.position;
		Instantiate(obj, spawnPos, Quaternion.identity);
	}

	void SetupTreasureTiles()
	{
		if(m_levelData == null)
			return;
		
		GetTilesEvent getTilesEvent = new GetTilesEvent();
		EventSys.BroadcastEvent(gameObject, getTilesEvent);

		if(getTilesEvent.rewardFreetiles == null || getTilesEvent.rewardFreetiles.Count <= 0)
			return;
		
		//Find a valid tile
		List<GameObject> tileListA = new List<GameObject>();
		List<GameObject> tileListB = new List<GameObject>();

		for(int i = 0; i < getTilesEvent.rewardFreetiles.Count; i++)
		{
			TileData tileData = getTilesEvent.rewardFreetiles[i].GetComponent<TileData>();

			if(!tileData)
				continue;

			if(!tileData.CanTraverse(0))
				continue;
			
			tileListA.Add(getTilesEvent.rewardFreetiles[i]);
		}

		TreasureData bestTreasure = null;

		for(int i = 0; i < m_levelData.treasureArr.Length; i++)
		{
			TreasureData curData = m_levelData.treasureArr[i];

			if(curData == null)
				continue;

			if(bestTreasure == null)
			{
				bestTreasure = curData;
				continue;
			}

			if(bestTreasure.GetValue() > curData.GetValue())
				continue;

			bestTreasure = curData;
		}

		int treasureToPlace = Mathf.CeilToInt(m_levelData.winScore/bestTreasure.GetValue());

		int treasureCount = Random.Range(m_levelData.minTreasureCount, m_levelData.maxTreasureCount);
		int treasurePlaced = 0;

		if(treasureCount < treasureToPlace)
			treasureCount = treasureToPlace;

		if(treasureCount > tileListA.Count)
			treasureCount = tileListA.Count;

		while(treasureCount != treasurePlaced && tileListA.Count > 0)
		{
			int randTile = Random.Range(0, tileListA.Count);

			GameObject curTile = tileListA[randTile];

			tileListA.RemoveAt(randTile);

			if(!curTile)
				continue;

			TileData tileData = curTile.GetComponent<TileData>();

			if(tileData == null)
				continue;

			for(int i = 0; i < tileData.objLinks.Count; i++)
			{
				if(tileData.objLinks[i] == null)
					continue;

				GameObject toObj = tileData.objLinks[i].to;

				if(!toObj)
					continue;

				tileListA.Remove(toObj);
				tileListB.Add(toObj);
			}
				
			TreasureData randTreasure = null;

			if(treasureToPlace > treasurePlaced)
			{
				randTreasure = bestTreasure;
			}
			else
			{
				float treasureChosen = Random.Range(0.0f, m_treasureRarityTotal);
				float curRarity = 0.0f;

				if(m_levelData != null)
				{
					for(int i = 0; i < m_levelData.treasureArr.Length; i++)
					{
						curRarity += m_levelData.treasureArr[i].rarity;

						if(curRarity >= treasureChosen)
						{
							randTreasure = m_levelData.treasureArr[i];
							break;
						}
					}
				}
			}
				
			TreasureReward newTreasureComp = curTile.AddComponent<TreasureReward>();
			newTreasureComp.SetTreasure(randTreasure);
			getTilesEvent.rewardTiles.Add(curTile);
			getTilesEvent.rewardFreetiles.Remove(curTile);
			treasurePlaced++;

			if(tileListA.Count <= 0)
			{
				tileListA = new List<GameObject>(tileListB);
				tileListB.Clear();
			}
		}
	}
}
