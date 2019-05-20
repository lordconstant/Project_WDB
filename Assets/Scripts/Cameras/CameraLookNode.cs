using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraNodeLink
{
	public AnimationCurve linkCurve = new AnimationCurve();
	public GameObject from;
	public GameObject to;

	public void DrawLink()
	{
		if(from == null)
			return;

		if(to == null)
			return;

		Vector3 start = from.transform.position;
		start.y += 0.01f;
		Vector3 end = to.transform.position;
		end.y += 0.01f;
		Vector3 length = end - start;
		Vector3 right = Vector3.Cross(length.normalized, Vector3.up);
		float stepSize = 1.0f / (length.magnitude * 4.0f);
		float curStep = 0.0f;

		Gizmos.color = Color.blue;

		while(curStep <= 1.0f)
		{
			float lastStep = curStep;
			curStep += stepSize;
			float curveStart = linkCurve.Evaluate(lastStep);
			float curveEnd = linkCurve.Evaluate(curStep);

			Vector3 lineStart = start + (length * lastStep);
			lineStart += right * curveStart;
			Vector3 lineEnd = start + (length * curStep);
			lineEnd += right * curveEnd;

			Gizmos.DrawLine(lineStart, lineEnd);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(start, 0.2f);
		Gizmos.DrawSphere(end, 0.2f);
	}

	public Vector3 GetLinkDir()
	{
		if(!from)
			return Vector3.zero;

		if(!to)
			return Vector3.zero;

		Vector3 toPos = GetPointOnLink(0.05f);

		return (toPos - from.transform.position).normalized;
	}

	public Vector3 GetPointOnLink(float prog)
	{
		Vector3 start = from.transform.position;
		Vector3 end = to.transform.position;
		Vector3 length = end - start;
		Vector3 right = Vector3.Cross(length.normalized, Vector3.up);

		float curveProg = linkCurve.Evaluate(prog);

		Vector3 point = start + (length * prog);
		return point + (right * curveProg);
	}
}

public class CameraLookNode : MonoBehaviour, EventInterface
{
	public GameObject nextLevelIndicator;
	public float radius = 1.0f;
	public List<CameraNodeLink> nodeLinks;
	List<GameObject> m_indicators = new List<GameObject>();
	bool m_activeNode = false;
	GameObject m_activator;

	// Use this for initialization
	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.INDICATORCLICK);
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.INDICATORCLICK);
	}
	
	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.INDICATORCLICK))
		{
			EventSys.BroadcastEvent(gameObject, m_activator, data);
		}
	}

	void OnDrawGizmos()
	{
		if(nodeLinks != null)
		{
			for(int i = 0; i < nodeLinks.Count; i++)
			{
				if(nodeLinks[i] != null)
					nodeLinks[i].DrawLink();
			}
		}
	}

	public void SetNodeActive(bool active, GameObject setBy)
	{
		if(m_activeNode && !active)
		{
			while(m_indicators.Count > 0)
			{
				Destroy(m_indicators[0]);
				m_indicators.RemoveAt(0);
			}
		}
		else if(!m_activeNode && active)
		{
			if(nodeLinks != null)
			{
				for(int i = 0; i < nodeLinks.Count; i++)
				{
					if(nodeLinks[i] == null)
						continue;

					Quaternion lookQuat = Quaternion.LookRotation(nodeLinks[i].GetLinkDir(), transform.up);
					Vector3 spawnPos = transform.position;
					spawnPos += nodeLinks[i].GetLinkDir() * radius;
					GameObject newIndicator = Instantiate(nextLevelIndicator, spawnPos, lookQuat);

					m_indicators.Add(newIndicator);
					newIndicator.transform.SetParent(gameObject.transform);
					LevelIndicatorClick indicatorClick = newIndicator.GetComponent<LevelIndicatorClick>();

					if(indicatorClick != null)
					{
						indicatorClick.ownerNode = gameObject;
						indicatorClick.link = nodeLinks[i];
					}
				}
			}
		}

		m_activator = setBy;
		m_activeNode = active;
	}
}
