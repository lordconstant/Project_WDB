using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITransitionHandler : MonoBehaviour 
{
	class TransitionPosition
	{
		public Vector3 fromPos;
		public Vector3 toPos;
	}

	public GameObject initialObj;
	public float transitionTime;

	GameObject m_activeObj;
	GameObject m_transitionTo;

	TransitionPosition m_fromTrans;
	TransitionPosition m_totrans;

	float m_transAmount;
	bool m_transition;

	// Use this for initialization
	void Awake () 
	{
		m_fromTrans = new TransitionPosition();
		m_totrans = new TransitionPosition();
		m_activeObj = initialObj;
		m_transition = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!m_transition)
			return;
		
		if(!m_activeObj && !m_transitionTo)
			return;

		m_transAmount += Time.deltaTime / transitionTime;

		if(m_transAmount > 1.0f)
			m_transAmount = 1.0f;

		RectTransform rectTrans = null;

		if(m_activeObj)
			rectTrans = m_activeObj.GetComponent<RectTransform>();

		if(rectTrans != null)
		{
			rectTrans.localPosition = Vector3.Lerp(m_fromTrans.fromPos, m_fromTrans.toPos, m_transAmount);
		}

		if(m_transitionTo)
			rectTrans = m_transitionTo.GetComponent<RectTransform>();
		else
			rectTrans = null;
		
		if(rectTrans != null)
		{
			rectTrans.localPosition = Vector3.Lerp(m_totrans.fromPos, m_totrans.toPos, m_transAmount);
		}

		if(m_transAmount >= 1.0f)
		{
			if(m_activeObj)
				m_activeObj.SetActive(false);
			m_activeObj = m_transitionTo;
			m_transitionTo = null;
			m_transAmount = 0.0f;
			m_transition = false;
		}
	}

	public void StartTransitionleft(GameObject transTo)
	{
		m_transitionTo = transTo;
		if(transTo)
			m_transitionTo.SetActive(true);
		m_transAmount = 0.0f;
		m_transition = true;

		if(m_activeObj)
		{
			RectTransform rectTrans = null;

			if(m_activeObj)
				rectTrans = m_activeObj.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_fromTrans.fromPos = rectTrans.localPosition;
				m_fromTrans.toPos = m_fromTrans.fromPos - (Vector3.right * Screen.width);
			}
		}

		if(m_transitionTo)
		{
			RectTransform rectTrans = null;

			if(m_transitionTo)
				rectTrans = m_transitionTo.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_totrans.fromPos = rectTrans.localPosition;
				m_totrans.fromPos.x = Screen.width * 0.5f;
				rectTrans.localPosition = m_totrans.fromPos;
				m_totrans.toPos = new Vector3(0, 0, 0);
			}
		}
	}

	public void StartTransitionRight(GameObject transTo)
	{
		m_transitionTo = transTo;
		if(transTo)
			m_transitionTo.SetActive(true);
		m_transAmount = 0.0f;
		m_transition = true;

		if(m_activeObj)
		{
			RectTransform rectTrans = null;

			if(m_activeObj)
				rectTrans = m_activeObj.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_fromTrans.fromPos = rectTrans.localPosition;
				m_fromTrans.toPos = m_fromTrans.fromPos + (Vector3.right * Screen.width);
			}
		}

		if(m_transitionTo)
		{
			RectTransform rectTrans = null;

			if(m_transitionTo)
				rectTrans = m_transitionTo.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_totrans.fromPos = rectTrans.localPosition;
				m_totrans.fromPos.x = -Screen.width * 0.5f;
				rectTrans.localPosition = m_totrans.fromPos;
				new Vector3(0, 0, 0);
			}
		}
	}

	public void StartTransitionUp(GameObject transTo)
	{
		m_transitionTo = transTo;
		if(transTo)
			m_transitionTo.SetActive(true);
		m_transAmount = 0.0f;
		m_transition = true;

		if(m_activeObj)
		{
			RectTransform rectTrans = null;

			if(m_activeObj)
				rectTrans = m_activeObj.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_fromTrans.fromPos = rectTrans.localPosition;
				m_fromTrans.toPos = m_fromTrans.fromPos + (Vector3.up * Screen.height);
			}
		}

		if(m_transitionTo)
		{
			RectTransform rectTrans = null;

			if(m_transitionTo)
				rectTrans = m_transitionTo.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_totrans.fromPos = rectTrans.localPosition;
				m_totrans.fromPos.y = -Screen.height * 0.5f;
				rectTrans.localPosition = m_totrans.fromPos;
				m_totrans.toPos = new Vector3(0, 0, 0);
			}
		}
	}

	public void StartTransitionDown(GameObject transTo)
	{
		m_transitionTo = transTo;
		if(transTo)
			m_transitionTo.SetActive(true);
		m_transAmount = 0.0f;
		m_transition = true;

		if(m_activeObj)
		{
			RectTransform rectTrans = null;

			if(m_activeObj)
				rectTrans = m_activeObj.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_fromTrans.fromPos = rectTrans.localPosition;
				m_fromTrans.toPos = m_fromTrans.fromPos - (Vector3.up * Screen.height);
			}
		}

		if(m_transitionTo)
		{
			RectTransform rectTrans = null;

			if(m_transitionTo)
				rectTrans = m_transitionTo.GetComponent<RectTransform>();

			if(rectTrans)
			{
				m_totrans.fromPos = rectTrans.localPosition;
				m_totrans.fromPos.y = Screen.height * 0.5f;
				rectTrans.localPosition = m_totrans.fromPos;
				m_totrans.toPos = new Vector3(0, 0, 0);
			}
		}
	}
}
