using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindCanvasToCamera : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		Canvas canvas = GetComponent<Canvas>();

		if(canvas == null)
			return;

		canvas.worldCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
