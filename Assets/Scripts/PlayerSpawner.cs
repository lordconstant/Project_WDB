using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, EventInterface
{
	public GameObject playerToSpawn;
	public Transform  spawnPoint;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.GAMESTART);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.GAMESTART);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.GAMESTART))
		{
			if(!playerToSpawn)
				return;

			Transform curSpawnTransform = spawnPoint;

			if(!curSpawnTransform)
				curSpawnTransform = transform;

			GameObject player = Instantiate(playerToSpawn, curSpawnTransform.position, curSpawnTransform.rotation) as GameObject;
			EnemySystem.SetTarget(player);
		}
	}

	void OnDrawGizmos()
	{
		Vector3 size = new Vector3(0.5f, 1.0f, 0.5f);
		Vector3 pos = transform.position + (Vector3.up * 0.5f);
		Color oldColour = Gizmos.color;
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(pos, size);
		Gizmos.color = oldColour;
	}
}
