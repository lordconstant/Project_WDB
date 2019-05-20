using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;

public class UnlinkedTileData
{
	public GameObject tile;
}

public class EdTileWindow : EditorWindow
{
	static bool m_editorEnabled;
	static bool m_decorationMode;
	static int m_selectedObj;
	static int m_curLayer;
	static float m_layerHeight;
	static Vector2 m_margin;
	static List<GameObject> m_tiles;
	static bool m_initialised;
	[SerializeField]
	static int m_gridSize;
	[SerializeField]
	static float m_gridSquareSize;
	static bool m_wasEditing;
	static Quaternion m_oldRotation;
	static Vector3 m_oldPosition;
	static GameObject m_levelObj;
	static GameObject m_fromObj;
	static VISIBILITYLAYER m_shownLayers;
	static LINKFLAGS m_curLinkType;
	static List<UnlinkedTileData> m_unlinkedTiles;
	static string[] m_folderDirs;
	static int m_folderNum;
	static int m_oldFolderNum;
	static int m_tileCount;
	static EdTileWindow m_curWindow;
	[SerializeField]
	public GameObject objectPrefab;

	//---------------------------------------------------------------------------------------------------------------------
	[MenuItem ("Tools/Tile Editor Window")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		m_curWindow = (EdTileWindow)EditorWindow.GetWindow (typeof (EdTileWindow));

		Vector2 size = new Vector2(0, 0);
		m_curWindow.minSize = size;

		size.x = 8000;
		size.y = 8000;

		m_gridSize = 50;
		m_gridSquareSize = 1.0f;
		m_curLayer = 0;
		m_layerHeight = 0.96f;
		m_curLinkType = LINKFLAGS.WALK;

		m_curWindow.maxSize = size;

		m_margin = new Vector2(10.0f, 10.0f);

		DirectoryInfo dir = new DirectoryInfo("Assets/Resources/TileEditorPrefabs/");
		DirectoryInfo[] subDirs = dir.GetDirectories();

		if(subDirs.Length > 0)
		{
			m_folderDirs = new string[subDirs.Length];

			int nonEmptyFolder = 0;

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				m_folderDirs[i] = subDirs[i].Name;
			}

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				FileInfo[] info = subDirs[i].GetFiles();

				if(info == null)
					continue;

				if(info.Length <= 0)
					continue;

				nonEmptyFolder = i;

				break;
			}

			SetupTilesFromFolder("TileEditorPrefabs/" + m_folderDirs[nonEmptyFolder]);
			m_folderNum = nonEmptyFolder;
			m_oldFolderNum = nonEmptyFolder;
		}
		else
		{
			m_folderNum = 0;
			m_oldFolderNum = 0;
		}

		m_initialised = true;
		m_levelObj = GameObject.Find("Level");

		if(m_levelObj == null)
		{
			m_levelObj = PrefabUtility.InstantiatePrefab(m_curWindow.objectPrefab) as GameObject;
			m_levelObj.name = "Level";
		}

		m_shownLayers = (VISIBILITYLAYER)(-1);
		m_wasEditing = false;

		m_unlinkedTiles = new List<UnlinkedTileData>();

		GameObject[] tileCountArr = GameObject.FindGameObjectsWithTag("Tile");

		if(tileCountArr != null)
			m_tileCount = tileCountArr.Length;
		else 
			m_tileCount = 0;
	}

	//---------------------------------------------------------------------------------------------------------------------
	void OnFocus()
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		Undo.undoRedoPerformed -= this.OnUndoCallback;
		Undo.undoRedoPerformed += this.OnUndoCallback;
	}

	//---------------------------------------------------------------------------------------------------------------------
	void OnDestroy()
	{
		ResetScene();
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		Undo.undoRedoPerformed -= this.OnUndoCallback;
		m_levelObj = null;
	}

	//---------------------------------------------------------------------------------------------------------------------
	void OnUndoCallback()
	{
		CleanUnlinkList();

		GameObject[] gameObjArr = GameObject.FindGameObjectsWithTag("Tile");

		for(int i = 0; i < gameObjArr.Length; i++)
		{
			if(gameObjArr[i] == null)
				continue;

			if(gameObjArr[i].GetComponent<TileData>() != null)
				continue;

			UnlinkTile(gameObjArr[i]);
		}
	}

	//---------------------------------------------------------------------------------------------------------------------
	void OnGUI ()
	{
		if(!m_initialised)
		{
			Init();
		}

		float verticalPos = 0.0f;

		verticalPos += m_margin.y;

		//Create fields for grid settings
		GUILayout.BeginArea(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 80.0f));
		{
			EditorGUILayout.BeginHorizontal();
			{
				m_gridSize = EditorGUILayout.IntField("GridSize", m_gridSize, GUILayout.MaxWidth(((position.width - (m_margin.x * 2.0f)) * 0.5f)));
				m_gridSquareSize = EditorGUILayout.FloatField("GridSpaceSize", m_gridSquareSize, GUILayout.MaxWidth(((position.width - (m_margin.x * 2.0f)) * 0.5f) - m_margin.x));
			}
			EditorGUILayout.EndHorizontal();
			verticalPos += 20.0f;
			m_curWindow.objectPrefab = EditorGUILayout.ObjectField("Level Prefab:", m_curWindow.objectPrefab, typeof(GameObject), false) as GameObject;
			verticalPos += 20.0f;
			m_curLayer = EditorGUILayout.IntField("Current Layer:", m_curLayer);
			verticalPos += 20.0f;
			m_layerHeight = EditorGUILayout.FloatField("Current Layer Height:", m_layerHeight);
			verticalPos += 20.0f;
		}
		GUILayout.EndArea();

		//Draw the Editor enable button
		if(!m_editorEnabled)
		{
			verticalPos += m_margin.y;
			GUILayout.BeginArea(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f));
			{
				if(GUILayout.Button("Enabled Editing"))
				{
					m_editorEnabled = true;
					SceneView.RepaintAll();
				}
			}
			GUILayout.EndArea();
			verticalPos += 60.0f;
		
			return;
		}
		else
		{
			//Draw the Editor disable button
			verticalPos += m_margin.y;
			GUILayout.BeginArea(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f));
			{
				if(GUILayout.Button("Disable Editing"))
				{
					m_editorEnabled = false;
					SceneView.RepaintAll();
					return;
				}
			}
			GUILayout.EndArea();
			verticalPos += 20.0f + m_margin.y;

			GUILayout.BeginArea(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f));
			{
			if(!m_decorationMode)
			{
				if(GUILayout.Button("Enable Decorating"))
				{
						EnableDecorating();
				}
			}
			else
			{
				if(GUILayout.Button("Disable Decorating"))
				{
						DisableDecorating();
				}
			}
			}
			GUILayout.EndArea();

			verticalPos += 20.0f + m_margin.y;

//			GUILayout.BeginArea(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f));
//			{
//				if(m_childrenVisble)
//				{
//					if(GUILayout.Button("Hide Tile Children"))
//					{
//						m_childrenVisble = false;
//					}
//				}
//				else
//				{
//					if(GUILayout.Button("Show Tile Children"))
//					{
//						m_childrenVisble = true;
//					}
//				}
//			}
//			GUILayout.EndArea();
//
//			verticalPos += 20.0f + m_margin.y;

			EditorGUI.LabelField(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f), "TileCount - " + m_tileCount);

			verticalPos += 20.0f + m_margin.y;

			m_shownLayers = (VISIBILITYLAYER)EditorGUI.EnumMaskPopup(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f), "Visible Layers", m_shownLayers);

			verticalPos += 20.0f + m_margin.y;

			m_curLinkType = (LINKFLAGS)EditorGUI.EnumPopup(new Rect(m_margin.x, verticalPos, position.width - (m_margin.x * 2.0f), 20.0f), "LinkType", m_curLinkType);

			SetOccupantsVisibility(m_shownLayers);

			verticalPos += 20.0f + m_margin.y;
		}
			
		float actualWidth = position.width - (m_margin.x * 2.0f);

		int gridXCount = Mathf.CeilToInt(actualWidth / 189.0f);

		if(gridXCount > m_folderDirs.Length)
			gridXCount = m_folderDirs.Length;
		
		float gridYCount = Mathf.Ceil(m_folderDirs.Length / (float)gridXCount);
		float gridHeight = gridYCount * 20.0f + ((gridYCount - 1) * 2.0f);
		GUILayout.BeginArea(new Rect(m_margin.x, verticalPos, actualWidth, gridHeight));
		m_folderNum = GUILayout.SelectionGrid(m_folderNum, m_folderDirs, gridXCount);
		GUILayout.EndArea();

		if(m_oldFolderNum != m_folderNum)
		{
			m_oldFolderNum = m_folderNum;
			SetupTilesFromFolder((m_decorationMode ? "DecorationEditorPrefab/" : "TileEditorPrefabs/") + m_folderDirs[m_folderNum]);
		}

		if(m_tiles == null || m_tiles.Count <= 0)
			return;
		
		verticalPos += gridHeight + m_margin.y;

		//We want to child tiles to the Level obj to keep our hierarchy cleaner
		if(m_levelObj == null)
		{
			m_levelObj = GameObject.Find("Level");

			if(m_levelObj == null)
			{
				m_levelObj = PrefabUtility.InstantiatePrefab(m_curWindow.objectPrefab) as GameObject;
			}
		}

		Selection.activeGameObject = m_tiles[m_selectedObj];

		float curX = m_margin.x;
		float curY = verticalPos;

		Color savedColour = GUI.backgroundColor;

		float iconWidth = 128.0f;
		float iconHeight = 128.0f;

		int iconsPerLine = Mathf.FloorToInt((actualWidth + m_margin.x) / (iconWidth + m_margin.x));
		iconsPerLine = iconsPerLine > m_tiles.Count ? m_tiles.Count : iconsPerLine;
		float xAlign = iconsPerLine <= 0 ? -(m_margin.x * 0.5f) : (actualWidth - (iconsPerLine * (iconWidth + m_margin.x))) * 0.5f;

		curX += xAlign;

		for(int i = 0; i < m_tiles.Count; i++)
		{
			Texture2D assetIcon = null;

			assetIcon = AssetPreview.GetAssetPreview(m_tiles[i]);

			if(assetIcon == null)
			{
				assetIcon = AssetPreview.GetMiniThumbnail(m_tiles[i]);
			}
			else
			{
				iconWidth = assetIcon.width;
				iconHeight = assetIcon.height;
			}

			if(assetIcon == null)
			{
				continue;
			}

			Vector2 size = new Vector2(iconWidth + m_margin.x, iconHeight + m_margin.y + 10.0f);
			Vector2 pos = size;
			pos.x = curX;
			pos.y = curY;

			GUIStyle newStyle = new GUIStyle();

			if(m_tiles[i] == m_tiles[m_selectedObj])
				GUI.color = Color.yellow;
			newStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.BeginArea(new Rect(pos, size));
			{
				if(GUILayout.Button(assetIcon, newStyle))
					m_selectedObj = i;

				GUILayout.Label(m_tiles[i].name, newStyle);
			}
			GUILayout.EndArea();


			curX += size.x;

			if(curX + iconWidth > position.width - m_margin.x)
			{
				curY += size.y;
				curX = xAlign + m_margin.x;
			}

			GUI.color = savedColour;
		}

		verticalPos = curY + iconHeight + m_margin.y;
	}

	//---------------------------------------------------------------------------------------------------------------------
	void OnSceneGUI(SceneView sceneView)
	{
		if(!EdTileWindow.IsEditorEnabled())
		{
			ResetScene();
			return;
		}
			
		if(!m_wasEditing)
		{
			m_oldRotation = SceneView.lastActiveSceneView.rotation;
			m_oldPosition = SceneView.lastActiveSceneView.pivot;
			SceneView.lastActiveSceneView.rotation = Quaternion.LookRotation(Vector3.down);
			SceneView.lastActiveSceneView.pivot = new Vector3(0.0f, 7.0f, 0.0f);
			SceneView.lastActiveSceneView.orthographic = true;
			SetOccupantsVisibility(m_shownLayers);
			m_wasEditing = true;

			GameObject[] gameObjArr = GameObject.FindGameObjectsWithTag("Tile");

			for(int i = 0; i < gameObjArr.Length; i++)
			{
				if(gameObjArr[i] == null)
					continue;

				if(gameObjArr[i].GetComponent<TileData>() != null)
					continue;

				UnlinkTile(gameObjArr[i]);
			}
		}

		if(Event.current.type == EventType.layout)
		{
			HandleUtility.AddDefaultControl(0);
			Rect screenRect = SceneView.lastActiveSceneView.position;
			EditorGUIUtility.AddCursorRect(screenRect, MouseCursor.Link);
			Tools.current = Tool.None;
		}

		SceneView.currentDrawingSceneView.isRotationLocked = true;

		Selection.activeGameObject = EdTileWindow.GetSelectedGameObject();

		Event curEvent = Event.current;

		if(curEvent.type != EventType.MouseDown && curEvent.type != EventType.MouseDrag && curEvent.type != EventType.DragUpdated && curEvent.type != EventType.DragPerform)
			return;

		Ray camRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		RaycastHit hit;

		if(Physics.Raycast(camRay, out hit))
		{
			if(!m_decorationMode)
			{
				if(curEvent.button < 1)
				{
					if(curEvent.button != 0)
						return;

					if(curEvent.shift)
					{
						if(hit.collider.gameObject.GetComponent<TileData>() == null)
							RelinkTile(hit.collider.gameObject);

						return;
					}
					//else if(m_fromObj)
					//{
					//	if(m_fromObj == hit.collider.gameObject)
					//	{
					//		SetFromObject(null);
					//	}
					//	else
					//	{
					//		ToggleTileLinks(m_fromObj, hit.collider.gameObject);
					//		SetFromObject(null);
					//	}
					//}
					//else
					//{
					//	SetFromObject(hit.collider.gameObject);
					//}

					if(hit.point.y >= -0.001f)
						return;
				}
				else if(curEvent.button == 1)
				{
					if(curEvent.shift)
					{
						if(hit.collider.gameObject.GetComponent<TileData>() != null)
							UnlinkTile(hit.collider.gameObject);
					}
					else
					{
						DestroyImmediate(hit.collider.gameObject);
						m_tileCount--;
					}

					Event.current.Use();

					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				GameObject hitObj = hit.collider.gameObject;

				if(curEvent.button == 1)
				{
					TileData curTile = hitObj.GetComponent<TileData>();

					if(!curTile)
						return;

					Dig digScript = hitObj.GetComponent<Dig>();

					if(curTile.decoration)
					{
						DestroyImmediate(curTile.decoration);
						curTile.decoration = null;
						Event.current.Use();

						if(digScript)
							digScript.enabled = true;

						EditorUtility.SetDirty(digScript);
						EditorUtility.SetDirty(curTile);

						return;
					}

					if(curTile.obstacle)
					{
						DestroyImmediate(curTile.obstacle);
						curTile.obstacle = null;
						curTile.obstacleType = OBSTACLETYPE.DEFAULT;
						Event.current.Use();
						if(digScript)
							digScript.enabled = true;

						EditorUtility.SetDirty(digScript);
						EditorUtility.SetDirty(curTile);

						return;
					}
				}
				else if(curEvent.button == 0)
				{
					TileData curTile = hitObj.GetComponent<TileData>();

					if(!curTile)
						return;

					if(curEvent.shift)
					{
						GameObject rotObj = curTile.decoration;

						if(!rotObj)
							rotObj = curTile.obstacle;

						if(rotObj)
						{
							rotObj.transform.Rotate(hitObj.transform.up, 90.0f);
						}

						return;
					}

					if(curTile.decoration)
					{
						DestroyImmediate(curTile.decoration);
						curTile.decoration = null;
					}
					else if(curTile.obstacle)
					{
						DestroyImmediate(curTile.obstacle);
						curTile.obstacle = null;
						curTile.obstacleType = OBSTACLETYPE.DEFAULT;
					}

					EditorUtility.SetDirty(curTile);

					GameObject decoObj = PrefabUtility.InstantiatePrefab(EdTileWindow.GetSelectedGameObject()) as GameObject;

					if(!decoObj)
						return;

					decoObj.transform.SetParent(hitObj.transform);
					decoObj.transform.localPosition = Vector3.zero;

					DecorationData decoData = decoObj.GetComponent<DecorationData>();

					if(decoData == null)
						return;

					TileInteraction tileInteraction = hitObj.GetComponent<TileInteraction>();

					if(tileInteraction)
					{
						tileInteraction.range = decoData.interactRange;

						EditorUtility.SetDirty(tileInteraction);
					}
					
					curTile.obstacleType = decoData.decoType;

					Dig digScript = hitObj.GetComponent<Dig>();

					if(curTile.obstacleType == OBSTACLETYPE.DEFAULT)
					{
						curTile.decoration = decoObj;

						if(digScript)
							digScript.enabled = true;
					}
					else
					{
						curTile.obstacle = decoObj;

						if(digScript)
							digScript.enabled = false;
					}

					Event.current.Use();

					EditorUtility.SetDirty(digScript);
					EditorUtility.SetDirty(curTile);
					return;
				}

			}
		}
		else
		{
			if(curEvent.shift)
			{
				return;
			}
		}

		if(curEvent.button != 0)
		{
			if(curEvent.button == 1)
				Event.current.Use();

			return;
		}

		if(m_decorationMode)
			return;
		
		float layerAdj = m_curLayer * m_layerHeight;
		Vector3 mouseGridPos = camRay.origin + (camRay.direction * -((camRay.origin.y - layerAdj) * (1.0f / camRay.direction.y)));

		mouseGridPos.x /= EdTileWindow.GetGridSquareSize();
		mouseGridPos.x = Mathf.Ceil(mouseGridPos.x - (EdTileWindow.GetGridSquareSize() * 0.5f));
		mouseGridPos.x *= EdTileWindow.GetGridSquareSize();
		mouseGridPos.z /= EdTileWindow.GetGridSquareSize();
		mouseGridPos.z = Mathf.Ceil(mouseGridPos.z - (EdTileWindow.GetGridSquareSize() * 0.5f));
		mouseGridPos.z *= EdTileWindow.GetGridSquareSize();

		Debug.DrawRay(mouseGridPos, Vector3.up, Color.red);
		GameObject spawnedObj = PrefabUtility.InstantiatePrefab(EdTileWindow.GetSelectedGameObject()) as GameObject;
		m_tileCount++;

		if(spawnedObj)
		{
			spawnedObj.transform.position = mouseGridPos;
			spawnedObj.transform.rotation = Quaternion.identity;

			TileData myTileData = spawnedObj.GetComponent<TileData>();

			if(myTileData)
			{
				myTileData.SetVisiblity(m_shownLayers);
			}
		}

		if(spawnedObj && m_levelObj)
		{
			spawnedObj.transform.SetParent(m_levelObj.transform);
		}

		Event.current.Use();
	}

	//---------------------------------------------------------------------------------------------------------------------
	void ResetScene()
	{
		if(!m_wasEditing)
			return;

		SceneView.lastActiveSceneView.isRotationLocked = false;
		SceneView.lastActiveSceneView.rotation = m_oldRotation;
		SceneView.lastActiveSceneView.pivot = m_oldPosition;
		SceneView.lastActiveSceneView.orthographic = false;
		SetOccupantsVisibility((VISIBILITYLAYER)(-1));

		CleanUnlinkList();
		SetFromObject(null);
		m_decorationMode = false;

		DirectoryInfo dir = new DirectoryInfo("Assets/Resources/TileEditorPrefabs/");
		DirectoryInfo[] subDirs = dir.GetDirectories();

		if(subDirs.Length > 0)
		{
			m_folderDirs = new string[subDirs.Length];

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				m_folderDirs[i] = subDirs[i].Name;
			}

			if(m_folderNum > subDirs.Length)
				m_folderNum = 0;
			
			SetupTilesFromFolder("TileEditorPrefabs/" + m_folderDirs[m_folderNum]);
		}

		m_wasEditing = false;
	}

	//---------------------------------------------------------------------------------------------------------------------
	static void SetupTilesFromFolder(string folderName)
	{
		Object[] foundObjs = Resources.LoadAll(folderName, typeof(GameObject));

		if(foundObjs == null || foundObjs.Length <= 0)
		{
			Debug.Assert(foundObjs != null && foundObjs.Length > 0, "No gameobjects found in Assets/Resources/TileEditorPrefabs/" + folderName);
		}
		else
		{
			if(m_tiles != null)
				m_tiles.Clear();
			else
				m_tiles = new List<GameObject>();

			for(int i = 0; i < foundObjs.Length; i++)
			{
				GameObject foundObj = (GameObject)foundObjs[i];
//				if(!foundObj.CompareTag("Tile"))
//					continue;

				m_tiles.Add(foundObj);
			}

			m_selectedObj = 0;
		}
	}

	static void CleanUnlinkList()
	{
		while(m_unlinkedTiles.Count > 0)
		{
			UnlinkedTileData unlinkData = m_unlinkedTiles[0];

			if(!unlinkData.tile)
			{
				m_unlinkedTiles.Remove(unlinkData);
				continue;
			}

			Renderer[] rendererArr = unlinkData.tile.GetComponentsInChildren<Renderer>();

			for(int j = 0; j < rendererArr.Length; j++)
			{
				Renderer curRend = rendererArr[j];

				if(curRend == null)
					continue;

				MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
				curRend.GetPropertyBlock(propBlock);
				propBlock.SetFloat("_DisableLerp", 0.0f);
				curRend.SetPropertyBlock(propBlock);
			}

			m_unlinkedTiles.Remove(unlinkData);
		}

	}

	//---------------------------------------------------------------------------------------------------------------------
	static void SetOccupantsVisibility(VISIBILITYLAYER shownLayers)
	{
		GameObject[] tileObjs = GameObject.FindGameObjectsWithTag("Tile");

		for(int i = 0; i < tileObjs.Length; i++)
		{
			TileData myTileData = tileObjs[i].GetComponent<TileData>();

			if(!myTileData)
				continue;

			myTileData.SetVisiblity(shownLayers);
		}
	}

	//---------------------------------------------------------------------------------------------------------------------
	[DrawGizmo(GizmoType.NonSelected)]
	static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType)
	{
		if(!m_editorEnabled)
			return;
		
		RenderGrid();

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(new Vector3(0.0f, 0.0f, 0.0f), 0.1f);
	}

	//---------------------------------------------------------------------------------------------------------------------
	static void RenderGrid()
	{
		float layerAdj = m_curLayer * m_layerHeight;
		Vector3 startPos = new Vector3(-(((m_gridSize * 0.5f) * m_gridSquareSize) + (m_gridSquareSize * 0.5f)), layerAdj, -(((m_gridSize * 0.5f) * m_gridSquareSize) + (m_gridSquareSize * 0.5f)));
		Vector3 endPos;

		Gizmos.color = Color.black;
		//Draw horizontal grid lines
		for(int i = 0; i <= m_gridSize; i++)
		{
			endPos = startPos;
			endPos.z += m_gridSize * m_gridSquareSize;
			Gizmos.DrawLine(startPos, endPos);

			startPos.x += m_gridSquareSize;
		}

		startPos = new Vector3(-(((m_gridSize * 0.5f) * m_gridSquareSize) + (m_gridSquareSize * 0.5f)), layerAdj, -(((m_gridSize * 0.5f) * m_gridSquareSize) + (m_gridSquareSize * 0.5f)));

		//Draw vertical grid lines
		for(int i = 0; i <= m_gridSize; i++)
		{
			endPos = startPos;
			endPos.x += m_gridSize * m_gridSquareSize;
			Gizmos.DrawLine(startPos, endPos);

			startPos.z += m_gridSquareSize;
		}
	}
		
	//---------------------------------------------------------------------------------------------------------------------
	public static bool IsEditorEnabled()
	{
		return m_editorEnabled;
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static GameObject GetSelectedGameObject()
	{
		if(m_tiles == null || m_tiles.Count <= 0)
			return null;

		return m_tiles[m_selectedObj];
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static int GetGridSize()
	{
		return m_gridSize;
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static float GetGridSquareSize()
	{
		return m_gridSquareSize;
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static void UnlinkTile(GameObject tile)
	{
		Undo.RecordObject(tile, "UnlinkTile");

		UnlinkedTileData unlinkData = new UnlinkedTileData();
		unlinkData.tile = tile;

		Renderer[] rendererArr = tile.GetComponentsInChildren<Renderer>();

		for(int j = 0; j < rendererArr.Length; j++)
		{
			Renderer curRend = rendererArr[j];

			if(curRend == null)
				continue;

			MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
			curRend.GetPropertyBlock(propBlock);
			propBlock.SetFloat("_DisableLerp", 1.0f);
			curRend.SetPropertyBlock(propBlock);
		}

		m_unlinkedTiles.Add(unlinkData);

		MonoBehaviour[] tileComps = tile.GetComponents<MonoBehaviour>();

		for(int i = 0; i < tileComps.Length; i++)
		{
			Undo.DestroyObjectImmediate(tileComps[i]);
		}
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static void RelinkTile(GameObject tile)
	{
		Undo.RecordObject(tile, "RelinkTile");

		UnlinkedTileData foundUnlinked = null;

		for(int i = 0; i < m_unlinkedTiles.Count; i++)
		{
			if(m_unlinkedTiles[i].tile == tile)
			{
				foundUnlinked = m_unlinkedTiles[i];
				break;
			}
		}

		if(foundUnlinked == null)
			return;
		
		Renderer[] rendererArr = tile.GetComponentsInChildren<Renderer>();

		for(int j = 0; j < rendererArr.Length; j++)
		{
			Renderer curRend = rendererArr[j];

			if(curRend == null)
				continue;

			MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
			curRend.GetPropertyBlock(propBlock);
			propBlock.SetFloat("_DisableLerp", 0.0f);
			curRend.SetPropertyBlock(propBlock);
		}

		m_unlinkedTiles.Remove(foundUnlinked);

		PrefabUtility.RevertPrefabInstance(tile);
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static void ToggleTileLinks(GameObject fromTile, GameObject toTile)
	{
		if(!fromTile)
			return;

		if(!toTile)
			return;

		if(fromTile == toTile)
			return;
		
		TileData curTileData = fromTile.GetComponent<TileData>();

		if(curTileData == null)
			return;

		for(int i = 0; i < curTileData.objLinks.Count; i++)
		{
			if(curTileData.objLinks[i].to != toTile)
				continue;

			curTileData.objLinks.RemoveAt(i);

			EditorUtility.SetDirty(curTileData);
			SceneView.RepaintAll();

			return;
		}

		ObjectLink newLink = new ObjectLink();
		newLink.from = fromTile;
		newLink.to = toTile;
		newLink.linkType = m_curLinkType;
		curTileData.objLinks.Add(newLink);

		EditorUtility.SetDirty(curTileData);
		SceneView.RepaintAll();
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static void SetFromObject(GameObject from)
	{
		if(m_fromObj == from)
			return;

		if(m_fromObj)
		{
			Renderer[] rendererArr = m_fromObj.GetComponentsInChildren<Renderer>();

			for(int j = 0; j < rendererArr.Length; j++)
			{
				Renderer curRend = rendererArr[j];

				if(curRend == null)
					continue;

				MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
				curRend.GetPropertyBlock(propBlock);
				propBlock.SetFloat("_SelectionLerp", 0.0f);
				curRend.SetPropertyBlock(propBlock);
			}
		}

		if(from)
		{
			Renderer[] rendererArr = from.GetComponentsInChildren<Renderer>();

			for(int j = 0; j < rendererArr.Length; j++)
			{
				Renderer curRend = rendererArr[j];

				if(curRend == null)
					continue;

				MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
				curRend.GetPropertyBlock(propBlock);
				propBlock.SetFloat("_SelectionLerp", 1.0f);
				curRend.SetPropertyBlock(propBlock);
			}
		}

		m_fromObj = from;
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static void EnableDecorating()
	{
		DirectoryInfo dir = new DirectoryInfo("Assets/Resources/DecorationEditorPrefab/");
		DirectoryInfo[] subDirs = dir.GetDirectories();

		if(subDirs.Length > 0)
		{
			m_decorationMode = true;

			m_folderDirs = new string[subDirs.Length];

			int nonEmptyFolder = 0;

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				m_folderDirs[i] = subDirs[i].Name;
			}

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				FileInfo[] info = subDirs[i].GetFiles();

				if(info == null)
					continue;

				if(info.Length <= 0)
					continue;

				nonEmptyFolder = i;

				break;
			}

			SetupTilesFromFolder("DecorationEditorPrefab/" + m_folderDirs[nonEmptyFolder]);
			m_folderNum = nonEmptyFolder;
			m_oldFolderNum = nonEmptyFolder;
		}
	}

	//---------------------------------------------------------------------------------------------------------------------
	public static void DisableDecorating()
	{
		DirectoryInfo dir = new DirectoryInfo("Assets/Resources/TileEditorPrefabs/");
		DirectoryInfo[] subDirs = dir.GetDirectories();

		if(subDirs.Length > 0)
		{
			m_decorationMode = false;

			m_folderDirs = new string[subDirs.Length];

			int nonEmptyFolder = 0;

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				m_folderDirs[i] = subDirs[i].Name;
			}

			for(int i = 0; i < m_folderDirs.Length; i++)
			{
				FileInfo[] info = subDirs[i].GetFiles();

				if(info == null)
					continue;

				if(info.Length <= 0)
					continue;

				nonEmptyFolder = i;

				break;
			}

			SetupTilesFromFolder("TileEditorPrefabs/" +m_folderDirs[nonEmptyFolder]);
			m_folderNum = nonEmptyFolder;
			m_oldFolderNum = nonEmptyFolder;
		}
	}
}