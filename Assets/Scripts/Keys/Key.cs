using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Key", menuName = "Create Key", order = 1)]
public class Key : ScriptableObject 
{
	public GameObject spawnObject;
	public Material keyMat;
}
