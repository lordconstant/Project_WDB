using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingTreasureUI : MonoBehaviour, EventInterface
{
	public Image treasureImage;
	public Text treasureText;
	public float moveSpeed;
	public float lifeTime;

	float m_destroyTime;
	bool m_beenSet;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TREASUREINCREASE);

		m_destroyTime = lifeTime;
		m_beenSet = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TREASUREINCREASE);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.TREASUREINCREASE))
		{
			if(m_beenSet)
				return;
			
			TreasureIncreaseEvent treasureIncData = data as TreasureIncreaseEvent;

			if(treasureText)
				treasureText.text = "+" + treasureIncData.data.GetValue().ToString();

			if(treasureImage && treasureIncData.data.img)
			{
				treasureImage.sprite = treasureIncData.data.img;
			}

			m_beenSet = true;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		FaceCamera(gameObject);

		Vector3 curPos = gameObject.transform.position;
		curPos.y += Time.deltaTime * moveSpeed;
		gameObject.transform.position = curPos;

		Color curColour;

		if(treasureText)
		{
			curColour = treasureText.color;
			curColour.a = lifeTime != 0.0f ? (m_destroyTime/lifeTime) : 0.0f;
			treasureText.color = curColour;
		}
		if(treasureImage)
		{
			curColour = treasureImage.color;
			curColour.a = lifeTime != 0.0f ? (m_destroyTime/lifeTime) : 0.0f;
			treasureImage.color = curColour;
		}

		m_destroyTime -= Time.deltaTime;

		if(m_destroyTime <= 0.0f)
		{
			Destroy(gameObject);
		}
	}

	void FaceCamera(GameObject obj)
	{
		if(!obj)
			return;

		Camera mainCam = Camera.main;

		if(!mainCam)
			return;

		Quaternion aimRot = Quaternion.LookRotation(mainCam.transform.forward);

		obj.transform.rotation = aimRot;
	}
}
