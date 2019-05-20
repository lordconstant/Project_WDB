using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Create Difficulty", order = 1)]
public class Difficulty : ScriptableObject 
{
	public int wormSpawnRate = 5;
	public float bombTimer = 4.0f;
	[Range(0.0f, 1.0f)]
	public float treasureValue = 1.0f;
}
