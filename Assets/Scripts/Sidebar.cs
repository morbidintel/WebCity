using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PhpDB;
using GoogleMaps;

public class Sidebar : Gamelogic.Extensions.Singleton<Sidebar>
{
	[SerializeField]
	DOTweenAnimation sidebarTween = null, arrowTween = null;

	public bool IsHidden { get; private set; } = false;
	public float TweenMaxX { get { return sidebarTween.endValueV3.x; } }

	// Use this for initialization
	void Start()
	{
		StartCoroutine(GetPlacesInItineraryCoroutine());
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void ToggleSidebar()
	{
		if (IsHidden)
		{
			sidebarTween.DOPlayForward();
			arrowTween.DOPlayForward();
		}
		else
		{
			sidebarTween.DOPlayBackwards();
			arrowTween.DOPlayBackwards();
		}
		
		IsHidden = !IsHidden;
	}

	IEnumerator GetPlacesInItineraryCoroutine()
	{
		string url = string.Format(PhpDB.GetPlacesResult.URL, WWW.EscapeURL("f7b56edb-793c-11e8-8405-f04da27518b5"));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		GetPlacesResult json = JsonUtility.FromJson<GetPlacesResult>(www.text);
		if (json.error != null)
		{
			Debug.Log(json.error);
			yield break;
		}

		foreach (var place in json.places)
		{
			StartCoroutine(GetPlaceCoroutine(place.googleid));
		}
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		string url = string.Format(PlaceDetails.URL, WWW.EscapeURL(place_id), "name,geometry");
		WWW www = new WWW(PHPProxy.Escape(url));
		yield return www;
		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		PlaceDetails place = JsonUtility.FromJson<PlaceDetails>(www.text);

		if (place.status != "OK")
		{
			Debug.Log(place.error_message);
			yield break;
		}

		MapTagManager.Instance.ShowPlaceOnMap(place);
	}
}
