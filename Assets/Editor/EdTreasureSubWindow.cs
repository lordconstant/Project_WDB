using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EdTreasureSubWindow 
{
	bool m_collapsed;
	Vector2 m_margin;
	int m_fieldHeight;
	TreasureData m_treasureData;

	public void SetupWindow(Vector2 margin, int fieldHeight, TreasureData data)
	{
		m_margin = margin;
		m_treasureData = data;
		m_fieldHeight = fieldHeight;
		m_collapsed = true;
	}

	public bool RenderTreasureSubWindow(Rect windowBounds, out Vector2 windowSize, out TreasureData copyData)
	{
		if(m_collapsed)
		{
			return RenderCollapsed(windowBounds, out windowSize, out copyData);
		}
		else
		{
			return RenderUncollapsed(windowBounds, out windowSize, out copyData);
		}
	}


	bool RenderCollapsed(Rect windowBounds, out Vector2 windowSize, out TreasureData copyData)
	{
		copyData = null;
		windowSize = new Vector2(windowBounds.width - (windowBounds.x + (m_margin.x * 2.0f) + 1), m_fieldHeight + (m_margin.y * 0.5f));

		bool retVal = true;

		Rect windowRect = new Rect(windowBounds.x, windowBounds.y, windowSize.x, windowSize.y);
		GUI.Box(windowRect, "");
		windowRect.x += m_margin.x;
		windowRect.y += m_margin.y * 0.5f;
		windowRect.width -= m_margin.x * 2.0f;
		windowRect.width -= 40 + (m_margin.x);

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "+", new GUIStyle(GUI.skin.label)))
		{
			copyData = m_treasureData;
		}

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x * 3.0f), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "V", new GUIStyle(GUI.skin.label)))
		{
			m_collapsed = false;
		}

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x * 5.0f), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "X", new GUIStyle(GUI.skin.label)))
		{
			retVal = false;
		}

		GUILayout.BeginArea(windowRect);
		GUILayout.BeginVertical();
		GUIStyle myStyle = new GUIStyle(GUI.skin.label);
		myStyle.alignment = TextAnchor.MiddleCenter;

		EditorGUILayout.LabelField(m_treasureData.name, myStyle);

		GUILayout.EndVertical();

		GUILayout.EndArea();

		return retVal;
	}

	bool RenderUncollapsed(Rect windowBounds, out Vector2 windowSize, out TreasureData copyData)
	{
		copyData = null;
		windowSize = new Vector2(windowBounds.width - (windowBounds.x + (m_margin.x * 2.0f) + 1), (9 * m_fieldHeight) + (m_margin.y * 0.5f));

		bool retVal = true;

		Rect windowRect = new Rect(windowBounds.x, windowBounds.y, windowSize.x, windowSize.y);
		GUI.Box(windowRect, "");
		windowRect.x += m_margin.x;
		windowRect.y += m_margin.y * 0.5f + m_fieldHeight;
		windowRect.width -= m_margin.x * 2.0f;
		windowRect.width -= 40 + (m_margin.x);
		windowRect.height -= 40;

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "+", new GUIStyle(GUI.skin.label)))
		{
			copyData = m_treasureData;
		}

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x * 3.0f), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "_", new GUIStyle(GUI.skin.label)))
		{
			m_collapsed = true;
		}

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x * 5.0f), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "X", new GUIStyle(GUI.skin.label)))
		{
			retVal = false;
		}

		GUILayout.BeginArea(windowRect);
		GUILayout.BeginVertical();
		GUIStyle myStyle = new GUIStyle(GUI.skin.label);
		myStyle.alignment = TextAnchor.MiddleCenter;

		m_treasureData.name = EditorGUILayout.TextField("Treasure Name:", m_treasureData.name);

		m_treasureData.value = EditorGUILayout.IntField("Value:", m_treasureData.value);

		m_treasureData.rarity = EditorGUILayout.FloatField("Rarity:", m_treasureData.rarity);

		m_treasureData.img = EditorGUILayout.ObjectField("Treasure Sprite:", m_treasureData.img, typeof(Sprite), false) as Sprite;

		myStyle.alignment = TextAnchor.MiddleCenter;

		GUILayout.EndVertical();

		GUILayout.EndArea();

		return retVal;
	}

	public string GetName()
	{
		return m_treasureData.name;
	}

	public void Save()
	{
		Serializer.Save("TreasureData/" + m_treasureData.name + ".xml", m_treasureData);
	}
}
