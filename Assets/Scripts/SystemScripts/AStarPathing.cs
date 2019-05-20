using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
public class EnumFlagsAttribute : PropertyAttribute
{
	public EnumFlagsAttribute() {}
}

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		_property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
	}
}
#endif

[System.Flags]
public enum TRAVERSEFLAGS
{
	OCCUPANT 	= 1<<0,
	OBSTACLE	= 1<<1,
	TREES 		= 1<<2,
	ROCKS 		= 1<<3,
	CHEST		= 1<<4
};

public class PathNode
{
	public TileData tileData;
	public PathNode parent;
	public float hValue;
	public float gValue;
	public float fValue;
}

public static class AStarPathing
{
	public class AStarPathingEventListener : EventInterface
	{
		public AStarPathingEventListener()
		{
			EventSys.RegisterDelegate(this, EVENTTYPE.LEVELLOADED);
		}

		~AStarPathingEventListener()
		{
			EventSys.UnRegisterDelegate(this, EVENTTYPE.LEVELLOADED);
		}

		public void EventReceive(GameObject go, EventBase data)
		{
			if(data.IsTypeOfEvent(EVENTTYPE.LEVELLOADED))
			{
				AStarPathing.SetupPathFinder();
			}
		}
	}

	static PathNode[] pathNodes;
	static PathNode m_bestNode;
	static AStarPathingEventListener m_eventListener;

	static AStarPathing()
	{
		SetupPathFinder();
		m_eventListener = new AStarPathingEventListener();
	}



	static void SetupPathFinder()
	{
		TileData[] foundTiles = GameObject.FindObjectsOfType<TileData>() as TileData[];
		pathNodes = new PathNode[foundTiles.Length];

		for(int i = 0; i < foundTiles.Length; i++)
		{
			PathNode node = new PathNode();
			node.tileData = foundTiles[i];
			node.parent = null;
			node.hValue = 0.0f;
			node.gValue = 0.0f;
			node.fValue = 0.0f;
			pathNodes[i] = node;
		}
	}

	public static List<TileData> FindPath(GameObject fromObj, GameObject toObj, TRAVERSEFLAGS flags = 0, LINKFLAGS linkFlags = (LINKFLAGS)~0)
	{
		List<TileData> tileList = new List<TileData>();;

		if(!fromObj)
			return tileList;

		if(!toObj)
			return tileList;
		
		TileTracker fromTracker = fromObj.GetComponent<TileTracker>();

		if(fromTracker == null)
			return tileList;
		
		TileTracker toTracker = toObj.GetComponent<TileTracker>();

		if(toTracker == null)
			return tileList;

		tileList = FindPath(fromTracker.curTile, toTracker.curTile, flags, linkFlags);

		return tileList;
	}

	public static List<TileData> FindPath(TileData fromTile, TileData toTile, TRAVERSEFLAGS flags = 0, LINKFLAGS linkFlags = (LINKFLAGS)~0)
	{
		List<PathNode> closedList = new List<PathNode>();
		List<PathNode> openList = new List<PathNode>();
		List<TileData> completePath = new List<TileData>();

		PathNode startNode = null;
		PathNode endNode = null;
		m_bestNode = null;

		for(int i = 0; i < pathNodes.Length; i++)
		{
			if(pathNodes[i].tileData == fromTile)
			{
				startNode = pathNodes[i];
			}
			
			if(pathNodes[i].tileData == toTile)
			{
				endNode = pathNodes[i];
			}
		}

		if(startNode == null || endNode == null)
			return completePath;
		
		startNode.fValue = 0.0f;
		startNode.hValue = 0.0f;
		startNode.gValue = 0.0f;

		PathNode cur = startNode;
		PathNode lastNode = startNode;

		closedList.Add(startNode);

		bool foundNode = true;

		while(foundNode && cur.tileData != toTile)
		{
			cur = FindNextNode(startNode, endNode, cur, closedList, openList, ref foundNode, flags, linkFlags);

			if(foundNode)
			{
				closedList.Add(cur);
				lastNode = cur;
			}
			else
			{
				lastNode = m_bestNode;
			}
		}
			
		bool atEnd = false;
		PathNode pathPos = lastNode;

		if(lastNode.tileData != toTile)
			return completePath;
		
		while(!atEnd)
		{
			completePath.Add(pathPos.tileData);

			if(pathPos.fValue > 0)
				pathPos = pathPos.parent;
			else
				atEnd = true;
		}

		completePath.Reverse();
		return completePath;
	}
		
	static PathNode FindNextNode(PathNode start, PathNode end, PathNode cur, List<PathNode> closedList, List<PathNode> openList, ref bool foundNode, TRAVERSEFLAGS flags, LINKFLAGS linkFlags)
	{
		for(int i = 0; i < cur.tileData.objLinks.Count; i++)
		{
			if(cur.tileData.objLinks[i] == null)
				continue;

			if(cur.tileData.objLinks[i].to == null)
				continue;

			if((linkFlags&cur.tileData.objLinks[i].linkType) == 0)
				continue;
			
			TileData linkTileData = cur.tileData.objLinks[i].to.GetComponent<TileData>();
			PathNode linkNode = null;
			for(int j = 0; j < pathNodes.Length; j++)
			{
				if(pathNodes[j].tileData == linkTileData)
				{
					linkNode = pathNodes[j];
					break;
				}
			}

			if(IsValidNode(linkNode, closedList, flags))
			{
				AddNodeToOpen(linkNode, cur, end, openList, closedList);
			}
		}

		PathNode bestNode = cur;
		float lastFValue = float.MaxValue;

		for(int i = 0; i < openList.Count; i++)
		{
			PathNode nodeVal = openList[i];

			if(nodeVal.fValue < lastFValue)
			{
				lastFValue = nodeVal.fValue;
				bestNode = nodeVal;
			}
		}

		if(bestNode != cur)
			foundNode = true;
		else
			foundNode = false;

		if(!foundNode)
		{
			if(m_bestNode == null || bestNode.fValue < m_bestNode.fValue)
				m_bestNode = bestNode;
			
			return bestNode;
		}

		for(int i = 0; i < bestNode.tileData.objLinks.Count; i++)
		{
			ObjectLink curLink = bestNode.tileData.objLinks[i];
			PathNode linkNode = openList.Find(node => node.tileData.gameObject == curLink.to);

			if(linkNode == null)
				continue;

			float gValue = bestNode.gValue;

			gValue += Vector3.Magnitude(bestNode.tileData.transform.position - curLink.to.transform.position);
			gValue += linkNode.tileData.ExtraMoveToCost();

			if(gValue < linkNode.gValue)
			{
				linkNode.gValue = gValue;
				linkNode.fValue = linkNode.hValue + gValue;
				linkNode.parent = bestNode;
			}
		}

		openList.Remove(bestNode);

		if(m_bestNode == null || bestNode.fValue < m_bestNode.fValue)
			m_bestNode = bestNode;
		
		return bestNode;
	}

	static void AddNodeToOpen(PathNode node, PathNode parent, PathNode end, List<PathNode> openList, List<PathNode> closedList)
	{
		if(openList.Find(searchNode => searchNode == node) != null)
			return;

		float gValue = parent.gValue;
		gValue += Vector3.Magnitude(node.tileData.transform.position - parent.tileData.transform.position);
		gValue += node.tileData.ExtraMoveToCost();

		node.parent = parent;

		node.hValue = Vector3.Magnitude(node.tileData.transform.position - end.tileData.transform.position);

		node.gValue = gValue;

		node.fValue = node.hValue + node.gValue;

		openList.Add(node);
	}

	static bool IsValidNode(PathNode node, List<PathNode> closedList, TRAVERSEFLAGS flags)
	{
		if(node == null)
		{
			return false;
		}

		if(!node.tileData.CanTraverse(flags))
			return false;

		if(closedList.Find(curNode => curNode == node) != null)
			return false;

		return true;
	}
}