using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombUI : MonoBehaviour, EventInterface
{
	public GameObject canvas;
	public Text timerText;
	public Image timerImage;

	float m_timer;
	float m_curTime;
	bool m_begin;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.BOMBTIMERSTART);
		m_begin = false;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.BOMBTIMERSTART);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.BOMBTIMERSTART))
		{
			BombTimerStart timerStartData = data as BombTimerStart;

			m_timer = timerStartData.time;
			m_curTime = m_timer;
			m_begin = true;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		FaceCamera(canvas);

		if(!m_begin)
			return;

		if(timerText != null)
			timerText.text = Mathf.CeilToInt(m_curTime).ToString();

		if(timerImage != null)
			timerImage.fillAmount = m_timer != 0.0f ? m_curTime / m_timer : 0.0f;

		m_curTime -= Time.deltaTime;
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
