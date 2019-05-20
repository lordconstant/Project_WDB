using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureUI : MonoBehaviour, EventInterface
{
	class TokenCounter
	{
		public string name;
		public int count;
		public Text text;
	}

	public GameObject treasureToken;
	public RectTransform treasureHolder;
	public Text treasureText;
	LevelData m_levelData;
	List<TokenCounter> m_tokens;

	void Awake () 
	{
		EventSys.RegisterDelegate(gameObject, this, EVENTTYPE.TREASURECHANGED);
	}

	void Start()
	{
		GetLevelDataEvent getLDEvent = new GetLevelDataEvent();

		EventSys.BroadcastEvent(gameObject, getLDEvent);
		m_levelData = getLDEvent.levelData;

		m_tokens = new List<TokenCounter>();

		if(treasureText != null)
		{
			treasureText.text = "0/" + m_levelData.winScore.ToString();
		}

		for(int i = 0; i < m_levelData.treasureArr.Length; i++)
		{
			CreateNewToken(m_levelData.treasureArr[i]);
		}
	}

	void OnDestroy()
	{
		EventSys.UnRegisterDelegate(gameObject, this, EVENTTYPE.TREASURECHANGED);
	}

	public void EventReceive(GameObject go, EventBase data)
	{
		if(treasureText == null)
			return;

		if(data.IsTypeOfEvent(EVENTTYPE.TREASURECHANGED))
		{
			TreasureChangedEvent treasureChangedData = data as TreasureChangedEvent;

			treasureText.text = treasureChangedData.totalTreasure.ToString() + "/" + m_levelData.winScore.ToString();

			for(int i = 0; i < m_tokens.Count; i++)
			{
				if(m_tokens[i].name == treasureChangedData.data.name)
				{
					m_tokens[i].count++;
					m_tokens[i].text.text = m_tokens[i].count.ToString();
					Rect tokenRect = m_tokens[i].text.rectTransform.rect;
					tokenRect.width = m_tokens[i].text.fontSize * m_tokens[i].text.text.Length;
					m_tokens[i].text.rectTransform.rect.Set(tokenRect.x, tokenRect.y, tokenRect.width, tokenRect.height);
				}
			}
		}
	}

	void CreateNewToken(TreasureData data)
	{
		if(data == null)
			return;

		if(treasureToken == null)
			return;

		if(treasureHolder == null)
			return;
		
		GameObject newToken = Instantiate(treasureToken, treasureHolder) as GameObject;

		newToken.GetComponent<Image>().sprite = data.img;

		TokenCounter newCounter = new TokenCounter();
		newCounter.name = data.name;
		newCounter.count = 0;
		newCounter.text = newToken.transform.Find("Counter").gameObject.GetComponent<Text>();
		m_tokens.Add(newCounter);
	}
}
