using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum VISIBILITYLAYER
{
	DEFAULT 	= 1<<0, 
	ROCK 		= 1<<1,
	TREE 		= 1<<2,
	DECORATION  = 1<<3,
	CHEST		= 1<<4
};

public enum OBSTACLETYPE { DEFAULT = 0, ROCK, TREE, CHEST };

public class CanMoveToEvent : EventBase
{
	public bool allowMove;
	public TRAVERSEFLAGS moveFlags;

	public CanMoveToEvent()
	{
		eventType = EVENTTYPE.CANMOVETO;
		allowMove = true;
		moveFlags = 0;
	}
}

public class MovedToEvent : EventBase
{
	public GameObject incObj;
	public TileData movedToTile;

	public MovedToEvent()
	{
		eventType = EVENTTYPE.MOVEDTO;
		movedToTile = null;
	}
}

public class MovedFromEvent : EventBase
{
	public GameObject leaveObj;

	public MovedFromEvent()
	{
		eventType = EVENTTYPE.MOVEDFROM;
	}
}

public class ArrivedEvent : EventBase
{
	public ArrivedEvent()
	{
		eventType = EVENTTYPE.ARRIVED;
	}
}

public class MoveTileEvent : EventBase
{
	public Vector3 pos;

	public MoveTileEvent()
	{
		eventType = EVENTTYPE.MOVETILEUPDATE;
	}

	public MoveTileEvent(Vector3 newPos)
	{
		pos = newPos;
		eventType = EVENTTYPE.MOVETILEUPDATE;
	}
}

public class ModelSwapEvent : EventBase
{
	public GameObject swapTo;
	public bool keepDecoration;

	public ModelSwapEvent()
	{
		eventType = EVENTTYPE.MODELSWAP;
		swapTo = null;
		keepDecoration = false;
	}

	public ModelSwapEvent(GameObject to)
	{
		eventType = EVENTTYPE.MODELSWAP;
		swapTo = to;
		keepDecoration = false;
	}
}

public class TileData : MonoBehaviour, EventInterface
{
	public List<ObjectLink> objLinks = new List<ObjectLink>();
	public GameObject obstacle;
	public GameObject occupant;
	public GameObject tileObj;
	public GameObject decoration;
	public OBSTACLETYPE obstacleType;
	public bool nonTraversable;

	bool m_partiallyHidden;
	bool m_blockedInternal;
	bool m_rootOccupant;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.MOVEDTO);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.MOVEDFROM);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.MODELSWAP);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.DAMAGE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.MOVETILEUPDATE);
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.CANMOVETO);

		m_partiallyHidden = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.MOVEDTO);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.MOVEDFROM);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.MODELSWAP);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.DAMAGE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.MOVETILEUPDATE);
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.CANMOVETO);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.MOVEDTO))
		{
			MovedToEvent moveToData = data as MovedToEvent;

			moveToData.movedToTile = this;

			occupant = moveToData.incObj;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.MOVEDFROM))
		{
			MovedFromEvent moveFromEvent = data as MovedFromEvent;

			if(moveFromEvent.leaveObj == occupant)
				occupant = null;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.MODELSWAP))
		{
			ModelSwapEvent swapEvent = data as ModelSwapEvent;

			GameObject newTileObj = null;

			Debug.Assert(swapEvent.swapTo, "Missing Swap Object!");
			
			newTileObj = Instantiate(swapEvent.swapTo, tileObj.transform.position, tileObj.transform.rotation) as GameObject;

			Destroy(tileObj);

			if(!swapEvent.keepDecoration && decoration)
				Destroy(decoration);
			
			tileObj = newTileObj;

			newTileObj.transform.SetParent(transform);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.DAMAGE))
		{
			if(occupant == null)
				return;
			
			EventSys.BroadcastEvent(go, occupant, data);
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.MOVETILEUPDATE))
		{
			MoveTileEvent mtEvent = data as MoveTileEvent;

			transform.position = mtEvent.pos;

			if(occupant)
				occupant.transform.position = mtEvent.pos;
		}
		else if(data.IsTypeOfEvent(EVENTTYPE.CANMOVETO))
		{
			CanMoveToEvent moveToEvent = data as CanMoveToEvent;

			if(!CanTraverse(moveToEvent.moveFlags))
				moveToEvent.allowMove = false;
		}
	}

	void OnDrawGizmos()
	{
		for(int i = 0; i < objLinks.Count; i++)
		{
			if(!objLinks[i].to)
			{
				objLinks.RemoveAt(i);
				i--;
				continue;
			}
			else
			{
				TileData linkToTileData = objLinks[i].to.GetComponent<TileData>();

				if(linkToTileData == null)
				{
					objLinks.RemoveAt(i);
					i--;
					continue;
				}
			}

			objLinks[i].DrawLink();
		}

		if(m_partiallyHidden)
		{
			Vector3 centre = transform.position;
			centre.y += 0.2f;
			Vector3 extents = new Vector3(0.4f, 0.4f, 0.4f);
			Color oldColour = Gizmos.color;
			Gizmos.color = Color.blue;
			Gizmos.DrawCube(centre, extents);
			Gizmos.color = oldColour;
		}
	}

	public ObjectLink IsLinked(GameObject gameObj)
	{
		for(int i = 0; i < objLinks.Count; i++)
		{
			if(objLinks[i].to == gameObj)
				return objLinks[i];
		}

		return null;
	}

	public bool CanTraverse(TRAVERSEFLAGS flags)
	{
		if(occupant != null)
		{
			if(occupant.GetComponent<PlayerController>() == null)
			{
				if((flags&TRAVERSEFLAGS.OCCUPANT) != TRAVERSEFLAGS.OCCUPANT)
					return false;
			}
		}

		if(obstacle != null)
		{
			if((flags&TRAVERSEFLAGS.OBSTACLE) != TRAVERSEFLAGS.OBSTACLE)
				return false;

			if(obstacleType == OBSTACLETYPE.TREE && (flags&TRAVERSEFLAGS.TREES) != TRAVERSEFLAGS.TREES)
				return false;

			if(obstacleType == OBSTACLETYPE.ROCK && (flags&TRAVERSEFLAGS.ROCKS) != TRAVERSEFLAGS.ROCKS)
				return false;

			if(obstacleType == OBSTACLETYPE.CHEST && (flags&TRAVERSEFLAGS.CHEST) != TRAVERSEFLAGS.CHEST)
				return false;
		}


		return !nonTraversable && !m_blockedInternal;
	}

	public void DamageOccupant(int damage, GameObject fromObj)
	{
		if(occupant == null)
			return;

		DamageEvent damageEvent = new DamageEvent();
		damageEvent.damage = damage;

		EventSys.BroadcastEvent(fromObj ? fromObj : gameObject, occupant, damageEvent);
	}

	public bool IsOccupantPlayer()
	{
		if(occupant == null)
			return false;

		return occupant.GetComponent<PlayerController>() != null;
	}

	public GameObject IsPlayerTileLinked()
	{
		for(int i = 0; i < objLinks.Count; i++)
		{
			if(objLinks[i].to == null)
				continue;
			
			TileData linkTileData = objLinks[i].to.GetComponent<TileData>();

			if(linkTileData == null)
				continue;

			if(!linkTileData.IsOccupantPlayer())
				continue;

			return objLinks[i].to;
		}

		return null;
	}

	public GameObject IsTileWithOccupantLinked(GameObject findOcc)
	{
		if(findOcc == null)
			return null;
		
		for(int i = 0; i < objLinks.Count; i++)
		{
			if(objLinks[i].to == null)
				continue;

			TileData linkTileData = objLinks[i].to.GetComponent<TileData>();

			if(linkTileData == null)
				continue;

			if(linkTileData.occupant != findOcc)
				continue;

			return objLinks[i].to;
		}

		return null;
	}

	public float ExtraMoveToCost()
	{
		if(!obstacle)
			return 0.0f;

		return 1.0f;
	}

	public void SetTileChildVisibility(bool visible)
	{
		m_partiallyHidden = !visible;

		if(obstacle)
		{
			obstacle.SetActive(visible);
		}

		if(decoration)
		{
			decoration.SetActive(visible);
		}
	}

	public void SetVisiblity(VISIBILITYLAYER showLayers)
	{
		m_partiallyHidden = false;

		if(obstacle)
		{
			if(obstacleType == OBSTACLETYPE.DEFAULT)
			{
				bool flagSet = ((int)showLayers&(int)VISIBILITYLAYER.DEFAULT) != 0;
				obstacle.SetActive(flagSet);
				m_partiallyHidden |= !flagSet;
			}

			if(obstacleType == OBSTACLETYPE.ROCK)
			{
				bool flagSet = ((int)showLayers&(int)VISIBILITYLAYER.ROCK) != 0;
				obstacle.SetActive(flagSet);
				m_partiallyHidden |= !flagSet;
			}

			if(obstacleType == OBSTACLETYPE.TREE)
			{
				bool flagSet = ((int)showLayers&(int)VISIBILITYLAYER.TREE) != 0;
				obstacle.SetActive(flagSet);
				m_partiallyHidden |= !flagSet;
			}
		}
			
		if(decoration)
		{
			bool flagSet = ((int)showLayers&(int)VISIBILITYLAYER.DECORATION) != 0;
			decoration.SetActive(flagSet);
			m_partiallyHidden |= !flagSet;
		}
	}

	public GameObject GetTileFromPosition(Vector3 pos, LINKFLAGS flags = (LINKFLAGS)~0)
	{
		ObjectLink bestLink = null;
		float bestVal = -1.0f;

		RaycastHit hit;

		Vector3 posDir = pos - transform.position;
		posDir.y = 0.0f;
		posDir.Normalize();

		pos += Vector3.up * 0.2f;

		if(Physics.Raycast(pos, Vector3.down * 10.0f, out hit))
		{
			if(hit.collider.gameObject == gameObject)
				return gameObject;
		}

		for(int i = 0; i < objLinks.Count; i++)
		{
			if(objLinks[i].from == null)
				continue;

			if(objLinks[i].to == null)
				continue;

			if((flags&objLinks[i].linkType) == 0)
				continue;
			
			Vector3 linkDir = objLinks[i].to.transform.position - objLinks[i].from.transform.position;
			linkDir.y = 0.0f;
			linkDir.Normalize();

			float dotVal = Vector3.Dot(linkDir, posDir);

			if(dotVal < 0.5f)
				continue;

			if(dotVal < bestVal)
				continue;

			bestLink = objLinks[i];
			bestVal = dotVal;
		}

		if(bestLink == null)
			return null;
		
		return bestLink.to;
	}

	public void SetOccupantRooted(bool root)
	{
		m_rootOccupant = root;
	}

	public bool OccupantRooted()
	{
		return m_rootOccupant;
	}

	public void SetBlockedInternal(bool block)
	{
		m_blockedInternal = block;
	}
}

#if UNITY_EDITOR
public static class LinkSetup
{
	[MenuItem ("Tools/Reset Links")]
	public static void LinkObjectsInScene()
	{
		GameObject[] gameObjArr = GameObject.FindGameObjectsWithTag("Tile");

		if(gameObjArr.Length <= 0)
			return;

		for(int i = 0; i < gameObjArr.Length; i++)
		{
			Vector3 boundSize = gameObjArr[i].GetComponent<Collider>().bounds.size;
			float radius = boundSize.x < boundSize.z ? boundSize.z : boundSize.x;
			radius *= 0.5f;
			Vector3 sphereCentre = gameObjArr[i].transform.position;
			//sphereCentre.y -= radius;
			Collider[] hitColliders = Physics.OverlapSphere(sphereCentre, radius);

			TileData tileData = gameObjArr[i].GetComponent<TileData>();

			if(tileData == null)
				continue; 

			if(tileData.objLinks != null)
			{
				for(int j = 0; j < tileData.objLinks.Count; j++)
				{
					if(!tileData.objLinks[j].autoCreated)
						continue;

					tileData.objLinks.RemoveAt(j);
					j--;
				}
			}

			tileData.objLinks.Clear();

			for(int j = 0; j < hitColliders.Length; j++)
			{
				if(hitColliders[j].gameObject == gameObjArr[i])
					continue;

				if(hitColliders[j].tag != "Tile")
					continue;
				
				ObjectLink newLink = new ObjectLink();
				newLink.from = gameObjArr[i];
				newLink.to = hitColliders[j].gameObject;
				newLink.linkType = LINKFLAGS.WALK;
				newLink.autoCreated = true;
				tileData.objLinks.Add(newLink);
			}

			EditorUtility.SetDirty(tileData);
		}

		SceneView.RepaintAll();
	}
}
#endif