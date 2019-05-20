using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LinkTraversal))]
public class CustomInspector : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		LinkTraversal curLink = target as LinkTraversal;

		curLink.traversalClass = ((MonoScript)curLink.traversalMethod).GetClass();

		EditorUtility.SetDirty(target);
	}
}
#endif

public class TraverseBeginEvent : EventBase
{
	public ObjectLink link;
	public TraverseData data;

	public TraverseBeginEvent()
	{
		eventType = EVENTTYPE.TRAVERSEBEGIN;
		link = null;
		data = null;
	}
}

public class TraverseEndEvent : EventBase
{
	public TraverseEndEvent()
	{
		eventType = EVENTTYPE.TRAVERSEEND;
	}
}

[CreateAssetMenu(fileName = "LinkTraversal", menuName = "Create LinkTraversal", order = 1)]
public class LinkTraversal : ScriptableObject 
{
	public LINKFLAGS typeOfLink;
	public TextAsset traversalMethod;
	public TraverseData data;
	[SerializeField]
	public System.Type traversalClass;
}
