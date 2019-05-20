using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class EdTreasureWindow : EditorWindow 
{
	static Vector2 m_margin;
	static bool m_initialised;
	static List<EdTreasureSubWindow> m_treasures;
	static int m_fieldHeight;
	static float m_scrollHeight;
	static Vector2 m_scrollPos;

	[MenuItem ("Tools/Treasure Window")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		EdTreasureWindow window = (EdTreasureWindow)EditorWindow.GetWindow (typeof (EdTreasureWindow));

		Vector2 size = new Vector2(0, 0);
		window.minSize = size;

		size.x = 8000;
		size.y = 8000;
		m_fieldHeight = 20;

		window.maxSize = size;

		m_margin = new Vector2(10.0f, 10.0f);

		m_initialised = true;

		m_treasures = new List<EdTreasureSubWindow>();

		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath + "/TreasureData/");
		FileInfo[] info = dir.GetFiles("*.*");

		for(int i = 0; i < info.Length; i++)
		{
			if(info[i].Name.EndsWith(".meta"))
				continue;

			if(info[i].Name.Contains("TreasureNames"))
				continue;
			
			TreasureData loadedData = Serializer.Load<TreasureData>("TreasureData/" + info[i].Name);
			EdTreasureSubWindow newSubWindow = new EdTreasureSubWindow();
			newSubWindow.SetupWindow(m_margin, m_fieldHeight, loadedData);
			m_treasures.Add(newSubWindow);
		}
	}

	void OnGUI ()
	{
		if(!m_initialised)
		{
			Init();
		}

		Vector2 pos = m_margin;
		Vector2 windowSize;
		float prefWindowWidth = position.width - (pos.x + m_margin.x);

		if(GUI.Button(new Rect(pos, new Vector2(prefWindowWidth - m_margin.x, 20)), "New Treasure"))
		{
			EdTreasureSubWindow newSubWindow = new EdTreasureSubWindow();
			newSubWindow.SetupWindow(m_margin, m_fieldHeight, new TreasureData());
			m_treasures.Add(newSubWindow);
		}

		pos.y += 20 + m_margin.y;

		bool showScroll = m_scrollHeight > (position.height - (20 + (m_margin.y * 2.0f)));

		if(showScroll)
			m_scrollPos = GUI.BeginScrollView(new Rect(pos, new Vector2(position.width - m_margin.x, position.height - pos.y)), m_scrollPos, new Rect(0, 0, position.width - 50, m_scrollHeight), false, true);

		Vector2 insideScrollPos;

		if(showScroll)
			insideScrollPos = new Vector2(0, 0);
		else
			insideScrollPos = pos;

		for(int i = 0; i < m_treasures.Count; i++)
		{
			Rect windowRect = new Rect(insideScrollPos, new Vector2( showScroll ? position.width - m_margin.x : position.width, 0.0f));
			TreasureData copyData = null;

			if(m_treasures[i].RenderTreasureSubWindow(windowRect, out windowSize, out copyData))
			{
				insideScrollPos.y += windowSize.y;
				insideScrollPos.y += m_margin.y;
			}
			else
			{
				m_treasures.RemoveAt(i);
				i--;
			}

			if(copyData != null)
			{
				EdTreasureSubWindow newSubWindow = new EdTreasureSubWindow();
				newSubWindow.SetupWindow(m_margin, m_fieldHeight, new TreasureData(copyData));
				m_treasures.Add(newSubWindow);
			}
		}

		if(showScroll)
		{
			pos += insideScrollPos;
			GUI.EndScrollView();
		}
		else
		{
			pos = insideScrollPos;
		}

		m_scrollHeight = pos.y - (20 + (m_margin.y * 2.0f));
	}

	void OnDestroy()
	{
		SArray<SVar<string>> nameArray = new SArray<SVar<string>>(m_treasures.Count);

		string treasureDirectory = Application.streamingAssetsPath + "/TreasureData/";

		if(Directory.Exists(treasureDirectory))
			Directory.Delete(treasureDirectory, true);

		Directory.CreateDirectory(treasureDirectory);

		for(int i = 0; i < m_treasures.Count; i++)
		{
			m_treasures[i].Save();
			nameArray.arr[i] = new SVar<string>(m_treasures[i].GetName());
		}

		Serializer.Save("TreasureData/TreasureNames.xml", nameArray);

		AssetDatabase.Refresh();
	}
}
