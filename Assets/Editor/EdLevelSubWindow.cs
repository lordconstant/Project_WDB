using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EdLevelSubWindow 
{
	bool m_collapsed;
	Vector2 m_margin;
	LevelData m_levelData;
	int m_fieldHeight;
	List<int> m_treasureDropDowns;
	string[] m_treasureNames;

	public void SetupWindow(Vector2 margin, int fieldHeight, string[] treasureNames, LevelData data)
	{
		m_margin = margin;
		m_levelData = data;
		m_fieldHeight = fieldHeight;
		m_collapsed = true;
		m_treasureDropDowns = new List<int>();
		m_treasureNames = treasureNames;

		if(m_levelData.treasureArr != null)
		{
			for(int i = 0; i < m_levelData.treasureArr.Length; i++)
			{
				for(int j = 0; j < m_treasureNames.Length; j++)
				{
					if(m_levelData.treasureArr[i].name == m_treasureNames[j])
					{
						m_treasureDropDowns.Add(j);
						break;
					}
				}
			}
		}
	}

	public bool RenderLevelSubWindow(Rect windowBounds, out Vector2 windowSize, out LevelData copyData)
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

	bool RenderCollapsed(Rect windowBounds, out Vector2 windowSize, out LevelData copyData)
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
			copyData = m_levelData;
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

		EditorGUILayout.LabelField(m_levelData.levelName, myStyle);

		GUILayout.EndVertical();

		GUILayout.EndArea();

		return retVal;
	}

	bool RenderUncollapsed(Rect windowBounds, out Vector2 windowSize, out LevelData copyData)
	{
		copyData = null;
		windowSize = new Vector2(windowBounds.width - (windowBounds.x + (m_margin.x * 2.0f) + 1), ((m_levelData.fieldCount + m_treasureDropDowns.Count + 2) * m_fieldHeight) + (m_margin.y * 0.5f));

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
			copyData = m_levelData;
		}

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x * 3.0f), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "_", new GUIStyle(GUI.skin.label)))
		{
			m_collapsed = true;
		}

		if(GUI.Button(new Rect(windowBounds.x + windowRect.width + (m_margin.x * 5.0f), windowBounds.y + (m_margin.y * 0.5f), 20, 20), "X", new GUIStyle(GUI.skin.label)))
		{
			retVal = false;
		}

		for(int i = 0; i < m_treasureDropDowns.Count; i++)
		{
			if(GUI.Button(new Rect(windowBounds.x + windowRect.width + m_margin.x, windowBounds.y + (m_margin.y * 0.5f) + (m_levelData.fieldCount * (m_fieldHeight + 2)) + (i * 18), 60, 15), "-", new GUIStyle(GUI.skin.button)))
			{
				m_treasureDropDowns.RemoveAt(i);
			}
		}

		GUILayout.BeginArea(windowRect);
		GUILayout.BeginVertical();
		GUIStyle myStyle = new GUIStyle(GUI.skin.label);
		myStyle.alignment = TextAnchor.MiddleCenter;

		m_levelData.levelName = EditorGUILayout.TextField("Level name:", m_levelData.levelName);

		m_levelData.winScore = EditorGUILayout.IntField("Win score:", m_levelData.winScore);

		m_levelData.minTreasureCount = EditorGUILayout.IntField("Min Treasure:", m_levelData.minTreasureCount);

		m_levelData.maxTreasureCount = EditorGUILayout.IntField("Max Treasure:", m_levelData.maxTreasureCount);

		m_levelData.nextScene = EditorGUILayout.TextField("Next Scene:", m_levelData.nextScene);

		myStyle.alignment = TextAnchor.MiddleRight;
		for(int i = 0; i < m_treasureDropDowns.Count; i++)
		{
			m_treasureDropDowns[i] = EditorGUILayout.Popup(m_treasureDropDowns[i], m_treasureNames);
		}

		myStyle.alignment = TextAnchor.MiddleCenter;

		GUILayout.EndVertical();

		GUILayout.EndArea();

		if(GUI.Button(new Rect(windowRect.x, windowRect.y + windowRect.height - m_margin.y, windowRect.width, 20.0f), "+"))
		{
			m_treasureDropDowns.Add(0);
		}

		return retVal;
	}

	public LevelData GetLevelData()
	{
		return m_levelData;
	}

	public void Save()
	{
		m_levelData.treasureArr = new TreasureData[m_treasureDropDowns.Count];

		for(int i = 0; i < m_levelData.treasureArr.Length; i++)
		{
			m_levelData.treasureArr[i] = Serializer.Load<TreasureData>("TreasureData/" + m_treasureNames[m_treasureDropDowns[i]] + ".xml");
		}

		Serializer.Save("LevelData/" + m_levelData.levelName + ".xml", m_levelData);
	}
}
