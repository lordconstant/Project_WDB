using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour, EventInterface
{
	public RectTransform uiArea;
	public GameObject healthPrefab;

	List<GameObject> m_fullHealthSymbols;
	List<GameObject> m_emptyHealthSymbols;

	int m_curSymbolCount;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.HEALTHCHANGED);

		m_curSymbolCount = 0;
		m_fullHealthSymbols = new List<GameObject>();
		m_emptyHealthSymbols = new List<GameObject>();
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.HEALTHCHANGED);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(data.IsTypeOfEvent(EVENTTYPE.HEALTHCHANGED))
		{
			HealthChangedEvent healthChangedData = data as HealthChangedEvent;
			//float healthPerc = healthChangedData.health / healthChangedData.maxHealth;

			if(m_curSymbolCount < healthChangedData.maxHealth)
			{
				int symbolsToAdd = healthChangedData.maxHealth;
				symbolsToAdd -= m_emptyHealthSymbols.Count;
				symbolsToAdd -= m_fullHealthSymbols.Count;

				for(int i = 0; i < symbolsToAdd; i++)
				{
					GameObject newHealthSymbol = Instantiate(healthPrefab, uiArea);
					m_fullHealthSymbols.Insert(0, newHealthSymbol);
				}
			}

			int healthChange = healthChangedData.health - m_fullHealthSymbols.Count;
			if(healthChange < 0)
			{
				healthChange = -healthChange;

				while(healthChange > 0)
				{
					GameObject curSymbol = m_fullHealthSymbols[0];
					Animator symbolAnimator = curSymbol.GetComponent<Animator>();

					if(symbolAnimator)
					{
						symbolAnimator.SetTrigger("BeginFlip");
					}

					m_emptyHealthSymbols.Add(curSymbol);
					m_fullHealthSymbols.Remove(curSymbol);
					healthChange--;
				}
			}
			else if(healthChange > 0)
			{
				
				while(healthChange > 0)
				{
					GameObject curSymbol = m_emptyHealthSymbols[0];
					Animator symbolAnimator = curSymbol.GetComponent<Animator>();

					if(symbolAnimator)
					{
						symbolAnimator.SetTrigger("BeginFlip");
					}

					m_fullHealthSymbols.Insert(0, curSymbol);
					m_emptyHealthSymbols.Remove(curSymbol);
					healthChange--;
				}
			}
		}
	}

}
