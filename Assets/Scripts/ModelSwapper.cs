using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSwapper : MonoBehaviour 
{
	public GameObject swapFrom;
	public GameObject swapTo;

	public void SwapModel()
	{
		if(swapFrom)
		{
			swapFrom.SetActive(!swapFrom.activeSelf);
		}

		if(swapTo)
		{
			swapTo.SetActive(!swapTo.activeSelf);
		}
	}
}
