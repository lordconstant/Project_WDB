using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwapper : MonoBehaviour 
{
	public Renderer rend;
	public float lerpTime = 1.0f;
	float m_curLerp;

	// Use this for initialization
	void Start () 
	{
		m_curLerp = 0.0f;
	}

	public void ToggleMatSwap()
	{
		StartCoroutine(LerpCoroutine());
	}

	IEnumerator LerpCoroutine()
	{
		if(m_curLerp == 0.0f)
		{
			while(m_curLerp < 1.0f)
			{
				m_curLerp += Time.deltaTime / lerpTime;

				if(m_curLerp > 1.0f)
					m_curLerp = 1.0f;

				MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
				propBlock.SetFloat("_Lerp", m_curLerp);
				rend.SetPropertyBlock(propBlock);

				yield return new WaitForSeconds(Time.deltaTime);
			}
		}
		else
		{
			while(m_curLerp > 0.0f)
			{
				m_curLerp -= Time.deltaTime / lerpTime;

				if(m_curLerp < 0.0f)
					m_curLerp = 0.0f;

				MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
				propBlock.SetFloat("_Lerp", m_curLerp);
				rend.SetPropertyBlock(propBlock);

				yield return new WaitForSeconds(Time.deltaTime);
			}
		}
	}
}
