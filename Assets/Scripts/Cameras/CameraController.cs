using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, EventInterface
{
	public GameObject target;
	public float rotSpeed = 45.0f;
	public float resetSpeed = 90.0f;
	public KeyCode rotLeftInput = KeyCode.Q;
	public KeyCode rotRightInput = KeyCode.E;
	public float nearDist = 1.5f;
	public float farDist = 10.0f;
	public float zoomSpeed = 2.0f;
	public float zoomResetSpeed = 20.0f;
	public float catchUpSpeed = 0.1f;

	Quaternion m_initialRot;
	Vector3 m_initialPos;
	GameObject m_camChild;
	Camera m_camComp;
	float m_intialZoom;
	bool m_gotNewTarget;

	void Awake()
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
	}

	// Use this for initialization
	void Start () 
	{
		m_initialRot = transform.rotation;
		m_camChild   = transform.GetChild(0).gameObject;

		if(!m_camChild)
		{
			Debug.Assert(m_camChild != null, "Needs a Camera as a child object! This script should be on an empty object with a Camera child!");
			return;
		}

		m_initialPos = m_camChild.transform.localPosition;
		m_camComp = m_camChild.GetComponent<Camera>();
		m_intialZoom = m_camChild.GetComponent<Camera>().orthographicSize;
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.PREGAMEEND);
	}

	// Update is called once per frame
	void Update () 
	{
		if(!m_camChild)
		{
			return;
		}

		if(!target)
		{
			target = GameObject.FindGameObjectWithTag("Player");

			if(!target)
				return;

			m_gotNewTarget = true;
		}

		if(m_gotNewTarget)
		{
			transform.position = Vector3.MoveTowards(transform.position, target.transform.position, catchUpSpeed);

			if(transform.position == target.transform.position)
				m_gotNewTarget = false;
		}
		else
		{
			transform.position = target.transform.position;
		}

		if(Input.GetKey(rotLeftInput))
		{
			Vector3 rotBy = new Vector3(0.0f, rotSpeed * Time.deltaTime, 0.0f);
			transform.Rotate(rotBy);
		}
		else if(Input.GetKey(rotRightInput))
		{
			Vector3 rotBy = new Vector3(0.0f, -rotSpeed * Time.deltaTime, 0.0f);
			transform.Rotate(rotBy);
		}
			
		if(Input.GetKey(KeyCode.Space))
		{
			Quaternion resetQuat = Quaternion.RotateTowards(transform.rotation, m_initialRot, resetSpeed * Time.deltaTime);
			transform.rotation = resetQuat;
			Vector3 newPos = Vector3.MoveTowards(m_camChild.transform.localPosition, m_initialPos, zoomResetSpeed * Time.deltaTime);
			m_camChild.transform.localPosition = newPos;
			m_camComp.orthographicSize = Mathf.MoveTowards(m_camComp.orthographicSize, m_intialZoom, Time.deltaTime * zoomResetSpeed);
		}

		if(!m_camChild)
		{	
			return;
		}

		Camera camComp = m_camChild.GetComponent<Camera>();

		if(camComp == null)
		{
			return;
		}

		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

		if(scrollWheel < 0.0f)
		{
			if(farDist > camComp.orthographicSize)
			{
				camComp.orthographicSize += zoomSpeed * Time.deltaTime;

				if(camComp.orthographicSize > farDist)
					camComp.orthographicSize = farDist;
			}
		}
		else if(scrollWheel > 0.0f)
		{
			if(nearDist < camComp.orthographicSize)
			{
				camComp.orthographicSize -= zoomSpeed * Time.deltaTime;

				if(camComp.orthographicSize < nearDist)
					camComp.orthographicSize = nearDist;
			}
		}
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.PREGAMEEND))
		{
			Destroy(this);
		}
	}
}
