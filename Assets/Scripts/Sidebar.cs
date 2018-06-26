using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PhpDB;
using System.Linq;

public class Sidebar : MonoBehaviour
{
	[SerializeField]
	DOTweenAnimation sidebarTween = null, arrowTween = null;

	public bool IsHidden { get; private set; } = false;

	// Use this for initialization
	void Start()
	{
		StartCoroutine(GetPlaces());
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void HideSidebar()
	{
		if (IsHidden)
		{
			sidebarTween.DOPlayBackwards();
			arrowTween.DOPlayBackwards();
		}
		else
		{
			sidebarTween.DOPlayForward();
			arrowTween.DOPlayForward();
		}

		IsHidden = !IsHidden;
	}

	IEnumerator GetPlaces()
	{
		string url = string.Format(PhpDB.GetPlaces.URL, WWW.EscapeURL("f7b56edb-793c-11e8-8405-f04da27518b5"));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
		}
		else
		{
			GetPlaces json = JsonUtility.FromJson<GetPlaces>(www.text);
			if (json.error == null)
			{
				Debug.Log(json.places[0].googleid);
			}
			else
			{
				Debug.Log(json.error);
			}
		}
	}
}
