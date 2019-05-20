using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Flags]
public enum LINKFLAGS 
{ 
	WALK 		= 1<<0,
	JUMPDOWN 	= 1<<1,
	CLAMBERUP 	= 1<<2,
	JUMPACROSS 	= 1<<3
};

public class GetTraverseFlags : EventBase
{
	public LINKFLAGS flags;

	public GetTraverseFlags()
	{
		eventType = EVENTTYPE.GETTRAVERSEFLAGS;
		flags = 0;
	}
}

[System.Serializable]
public class ObjectLink
{
	public GameObject from;
	public GameObject to;
	public LINKFLAGS linkType;
	public bool autoCreated;

	public void DrawLink()
	{
		if(from == null)
			return;

		if(to == null)
			return;

		Vector3 dir = to.transform.position - from.transform.position;
		Vector3 start = from.transform.position;
		Vector3 right = Vector3.Cross(dir.normalized, Vector3.up);
		right.Normalize();
		start += dir.normalized * 0.3f;
		start += right * 0.1f;
		start += Vector3.up * 0.02f;
		Vector3 end = start + dir;
		end -= dir.normalized * 0.6f;

		TileData tileData = to.GetComponent<TileData>();

		Color lineColour;
		if(tileData == null)
			lineColour = Color.green;
		else
			lineColour = (!tileData.CanTraverse(0)) ? Color.red : Color.blue;

		Gizmos.color = lineColour;
		Gizmos.DrawLine(start, end);
		Gizmos.DrawSphere(end, 0.05f);
	}
}