using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathingTester : MonoBehaviour 
{
	public GameObject fromObj;
	public GameObject toObj;

	List<TileData> m_tiles;

	void Start()
	{

	}

	// Update is called once per frame
	void Update () 
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit))
		{
			if(Input.GetMouseButtonDown(0))
			{
				if(fromObj == null)
					fromObj = hit.collider.gameObject;
				else
					toObj = hit.collider.gameObject;
			}
		}

		if(Input.GetKeyDown(KeyCode.Space))
		{
			m_tiles = AStarPathing.FindPath(fromObj.GetComponent<TileData>(), toObj.GetComponent<TileData>());
			fromObj = null;
			toObj = null;
		}

		if(m_tiles == null)
			return;

		if(m_tiles.Count <= 1)
			return;
		
		for(int i = 0; i < m_tiles.Count-1; i++)
		{
			Vector3 toVec = m_tiles[i+1].gameObject.transform.position - m_tiles[i].gameObject.transform.position;

			Debug.DrawRay(m_tiles[i].gameObject.transform.position, toVec, Color.red);
		}
	}
}
