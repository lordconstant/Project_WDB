using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class EdLevelWindow : EditorWindow
{
	static Vector2 m_margin;
	static bool m_initialised;
	static List<EdLevelSubWindow> m_levels;
	static int m_fieldHeight;
	static float m_scrollHeight;
	static Vector2 m_scrollPos;
	static string[] m_treasureNames;
	static int m_levelId;

	[MenuItem ("Tools/Level Window")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		EdLevelWindow window = (EdLevelWindow)EditorWindow.GetWindow (typeof (EdLevelWindow));

		Vector2 size = new Vector2(0, 0);
		window.minSize = size;

		size.x = 8000;
		size.y = 8000;
		m_fieldHeight = 20;

		window.maxSize = size;

		m_margin = new Vector2(10.0f, 10.0f);

		m_initialised = true;

		m_levels = new List<EdLevelSubWindow>();

		SVar<int> loadedLevelId = Serializer.Load<SVar<int>>("LevelData/LevelsInfo.xml");

		if(loadedLevelId != null)
			m_levelId = loadedLevelId.var;
		else
			m_levelId = 0;
		
		SArray<SVar<string>> loadedNames = Serializer.Load<SArray<SVar<string>>>("TreasureData/TreasureNames.xml");

		if(loadedNames != null)
		{
			m_treasureNames = new string[loadedNames.arr.Length];

			for(int i = 0; i < m_treasureNames.Length; i++)
			{
				m_treasureNames[i] = loadedNames.arr[i].var;
			}
		}
		else
		{
			m_treasureNames = new string[0];
		}

		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath + "/LevelData/");
		FileInfo[] info = dir.GetFiles("*.*");

		for(int i = 0; i < info.Length; i++)
		{
			if(info[i].Name.EndsWith(".meta"))
				continue;

			if(info[i].Name.Contains("LevelsInfo"))
				continue;

			LevelData loadedData = Serializer.Load<LevelData>("LevelData/" + info[i].Name);

			if(loadedData == null)
				continue;
			
			EdLevelSubWindow newSubWindow = new EdLevelSubWindow();
			newSubWindow.SetupWindow(m_margin, m_fieldHeight, m_treasureNames, loadedData);
			m_levels.Add(newSubWindow);
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

		if(GUI.Button(new Rect(pos, new Vector2(prefWindowWidth - m_margin.x, 20)), "New Level"))
		{
			EdLevelSubWindow newSubWindow = new EdLevelSubWindow();
			newSubWindow.SetupWindow(m_margin, m_fieldHeight, m_treasureNames, new LevelData(m_levelId));
			m_levelId++;
			m_levels.Add(newSubWindow);
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
		
		for(int i = 0; i < m_levels.Count; i++)
		{
			Rect windowRect = new Rect(insideScrollPos, new Vector2( showScroll ? position.width - m_margin.x : position.width, 0.0f));
			LevelData copyData = null;

			if(m_levels[i].RenderLevelSubWindow(windowRect, out windowSize, out copyData))
			{
				insideScrollPos.y += windowSize.y;
				insideScrollPos.y += m_margin.y;
			}
			else
			{
				m_levels.RemoveAt(i);
				i--;
			}

			if(copyData != null)
			{
				EdLevelSubWindow newSubWindow = new EdLevelSubWindow();
				newSubWindow.SetupWindow(m_margin, m_fieldHeight, m_treasureNames, new LevelData(copyData, m_levelId));
				m_levelId++;
				m_levels.Add(newSubWindow);
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
		string[] enumEntries = new string[m_levels.Count];
		int[] enumValues = new int[m_levels.Count];

		string levelDirectory = Application.streamingAssetsPath + "/LevelData/";

		if(Directory.Exists(levelDirectory))
			Directory.Delete(levelDirectory, true);

		Directory.CreateDirectory(levelDirectory);

		SVar<int> saveLevelId = new SVar<int>(m_levelId);
		Serializer.Save<SVar<int>>("LevelData/LevelsInfo.xml", saveLevelId);

		for(int i = 0; i < m_levels.Count; i++)
		{
			m_levels[i].Save();
			enumEntries[i] = m_levels[i].GetLevelData().levelName;
			enumValues[i] = m_levels[i].GetLevelData().levelNum;
		}

		string enumName = "LEVELNAMES";
		string filePathAndName = "Assets/Scripts/GeneratedEnums/" + enumName + ".cs"; //The folder Scripts/Enums/ is expected to exist
		
		using ( StreamWriter streamWriter = new StreamWriter( filePathAndName ) )
		{
			streamWriter.WriteLine( "public enum " + enumName );
			streamWriter.WriteLine( "{" );
			for( int i = 0; i < enumEntries.Length; i++ )
			{
				streamWriter.WriteLine( "\t" + enumEntries[i] + " = " + enumValues[i] + "," );
			}
			streamWriter.WriteLine( "}" );
		}
		AssetDatabase.Refresh();
	}
}