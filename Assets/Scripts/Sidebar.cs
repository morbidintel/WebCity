using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PhpDB;
using GoogleMaps;

public class Sidebar : Gamelogic.Extensions.Singleton<Sidebar>
{
	[SerializeField]
	DOTweenAnimation sidebarTween = null, arrowTween = null,
		itinerariesTween = null, placesTween = null;

	[SerializeField]
	Transform itinerariesHolder = null, placeHolder = null;
	[SerializeField]
	GameObject itineraryItemPrefab = null, placeItemPrefab = null;
	[SerializeField]
	Text itineraryTitle = null;

	List<ItineraryListItem> itineraries = new List<ItineraryListItem>();
	List<PlaceListItem> placesShown = new List<PlaceListItem>();

	public bool IsHidden { get; private set; } = false;
	public bool ShowItineraries { get; private set; } = true;
	public float TweenMaxX { get { return sidebarTween.endValueV3.x; } }

	// Use this for initialization
	void Start()
	{
#if UNITY_EDITOR
		if (PersistentUser.Instance == null)
			PersistentUser.Create(new LoginResult("3fc82c00-7b92-11e8-a2a6-f04da27518b5", "alpha"));
#endif

		StartCoroutine(GetItinerariesCoroutine());
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

	ItineraryListItem AddItineraryListItem(Itinerary itinerary)
	{
		ItineraryListItem item = Instantiate(itineraryItemPrefab, itinerariesHolder)
			.GetComponent<ItineraryListItem>();
		item.Init(itinerary);
		itineraries.Add(item);
		return item;
	}

	PlaceListItem AddPlaceListItem(Place place)
	{
		PlaceListItem item = Instantiate(placeItemPrefab, placeHolder)
			.GetComponent<PlaceListItem>();
		item.Init(place);
		placesShown.Add(item);
		return item;
	}

	void ClearPlaceItems()
	{
		foreach (var p in placesShown) Destroy(p);
		placesShown.Clear();
	}

	public void OnClickItineraryItem(ItineraryListItem item)
	{
		itineraryTitle.text = item.itinerary.name;
		StartCoroutine(GetPlacesInItineraryCoroutine(item));
	}

	public void OnClickPlaceItem(PlaceListItem item)
	{
		if (item.placeDetails != null)
			MapCamera.Instance.SetCameraViewport(item.placeDetails.result.geometry);
	}

	public void OnClickReturnToItineraries()
	{
		itinerariesTween.DOPlayBackwards();
		placesTween.DOPlayBackwards();
		ShowItineraries = true;
		ClearPlaceItems();
		MapTagManager.Instance.ClearMapTags();
	}

	IEnumerator GetItinerariesCoroutine()
	{
		LoginResult user = PersistentUser.User;
		string url = string.Format(GetItinerariesResult.URL, WWW.EscapeURL(user.userid));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		GetItinerariesResult json = JsonUtility.FromJson<GetItinerariesResult>(www.text);
		if (json.error != null)
		{
			Debug.Log(json.error);
			yield break;
		}

		foreach (var itinerary in json.itineraries)
		{
			AddItineraryListItem(itinerary);
		}
	}

	IEnumerator GetPlacesInItineraryCoroutine(ItineraryListItem item)
	{
		if (item.places.Count == 0)
		{
			string url = string.Format(GetPlacesResult.URL, WWW.EscapeURL(item.itinerary.itineraryid));
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
				item.places.Add(AddPlaceListItem(place));
			}

			yield return new WaitUntil(() => placesShown.All(p => !p.isLoading));
		}
		else
		{
			foreach (var place in item.places)
			{
				AddPlaceListItem(place.place);
			}
		}

		itinerariesTween.DOPlayForward();
		placesTween.DOPlayForward();
		ShowItineraries = false;
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
