﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PhpDB;
using GoogleMaps;
using System;

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

	ItineraryListItem currentItinerary = null;

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

	public void ToggleLists()
	{
		if (ShowItineraries)
		{
			itinerariesTween.DOPlayForward();
			placesTween.DOPlayForward();
		}
		else
		{
			itinerariesTween.DOPlayBackwards();
			placesTween.DOPlayBackwards();
		}
		ShowItineraries = !ShowItineraries;
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
		if (item.data.placeDetails != null)
			MapCamera.Instance.SetCameraViewport(item.data.placeDetails.result.geometry);
	}

	public void OnClickReturnToItineraries()
	{
		currentItinerary = null;
		ToggleLists();
		ClearPlaceItems();
		MapTagManager.Instance.ClearMapTags();
	}

	public void OnSubmitNewItinerary(string itineraryName)
	{
		StartCoroutine(AddItineraryCoroutine(itineraryName));
	}

	public void OnClickAddPlaceTooltip(PlaceDetails place)
	{
		StartCoroutine(AddPlaceCoroutine(currentItinerary, place));
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

		currentItinerary = null;
	}

	IEnumerator GetPlacesInItineraryCoroutine(ItineraryListItem item)
	{
		if (item.placesData.Count == 0)
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
			}
			else
			{
				foreach (var place in json.places)
				{
					var newItem = AddPlaceListItem(place);
					var data = new PlaceListItemData();
					data = newItem.data;
					item.placesData.Add(data);
				}

				yield return new WaitUntil(() => placesShown.All(p => !p.isLoading));
			}
		}
		else
		{
			foreach (var place in item.placesData)
			{
				AddPlaceListItem(place.place);
			}
		}

		currentItinerary = item;
		ToggleLists();
		if (placesShown.Count > 0)
		{
			// move camera to see all places
			var positions = item.placesData.Select(p => p.pos);
			Vector3 center = positions
			.Aggregate(Vector3.zero, (total, next) => total + next)
			/ item.placesData.Count;
			float maxDist = positions
			.Aggregate(0f, (total, next) =>
			{
				float dist = (next - center).magnitude;
				return dist > total ? dist : total;
			});
			MapCamera.Instance.Reset(center, Quaternion.Euler(90, 0, 0), maxDist * 2f);
		}
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		string url = string.Format(PlaceDetails.URL, WWW.EscapeURL(place_id), "name,geometry,place_id");
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

	IEnumerator AddItineraryCoroutine(string itineraryName)
	{
		LoginResult user = PersistentUser.User;
		string url = string.Format(AddItineraryResult.URL, WWW.EscapeURL(user.userid), WWW.EscapeURL(itineraryName), AddItineraryResult.DefaultColors);
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		AddItineraryResult json = JsonUtility.FromJson<AddItineraryResult>(www.text);
		if (json.error != null)
		{
			Debug.Log(json.error);
			yield break;
		}

		AddItineraryListItem(json.itineraries[0]);
	}

	IEnumerator AddPlaceCoroutine(ItineraryListItem itinerary, PlaceDetails place)
	{
		LoginResult user = PersistentUser.User;
		string url = string.Format(AddPlaceResult.URL, WWW.EscapeURL(itinerary.itinerary.itineraryid), WWW.EscapeURL(place.result.place_id));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		AddPlaceResult json = JsonUtility.FromJson<AddPlaceResult>(www.text);
		if (json.error != null)
		{
			Debug.Log(json.error);
			yield break;
		}
		else if (json.places.Length > 0)
		{
			var newItem = AddPlaceListItem(json.places[0]);
			var data = new PlaceListItemData();
			data = newItem.data;
			itinerary.placesData.Add(data);
		}
	}

	IEnumerator EditItineraryCoroutine()
	{
		yield break;
	}
}