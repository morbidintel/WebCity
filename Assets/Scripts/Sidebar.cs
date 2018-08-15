using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DG.Tweening;
using PhpDB;
using GoogleMaps;

public class Sidebar : Gamelogic.Extensions.Singleton<Sidebar>
{
	[SerializeField]
	DOTweenAnimation[] HidingTweens, PagesTweens;

	[Space]
	[SerializeField]
	Transform itinerariesHolder = null;
	[SerializeField]
	Transform placesHolder = null;
	[SerializeField]
	GameObject itineraryItemPrefab = null, placeItemPrefab = null;

	[Space]
	[SerializeField]
	InputField itineraryNameInput = null;
	[SerializeField]
	Toggle itineraryRenameToggle = null;

	[Space]
	[SerializeField]
	PlaceDetailsPage placeDetailsPage = null;

	public enum Page { Itineraries, Places, PlaceDetails }
	public Page currentPage { get; private set; } = Page.Itineraries;

	List<ItineraryListItem> itineraries = new List<ItineraryListItem>();
	List<PlaceListItem> placesShown = new List<PlaceListItem>();

	ItineraryListItem currentItinerary = null;
	public DistanceMatrix currentDistanceMatrix { get; private set; } = null;

	public bool IsHidden { get; private set; } = false;
	public float TweenMaxX { get { return HidingTweens.FirstOrDefault().endValueV3.x; } }

	// Use this for initialization
	void Start()
	{
#if UNITY_EDITOR
		if (PersistentUser.Instance == null)
			PersistentUser.Create(new LoginResult("3fc82c00-7b92-11e8-a2a6-f04da27518b5", "alpha"));
#endif

		StartCoroutine(GetItinerariesCoroutine());

		float sidebarWidth = this.RectTransform().rect.width;
		for (int i = 0; i < PagesTweens.Length; ++i)
			PagesTweens[i].RectTransform().SetLocalPosX(sidebarWidth * i);
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
		PlaceListItem item = Instantiate(placeItemPrefab, placesHolder)
			.GetComponent<PlaceListItem>();
		item.Init(place);
		placesShown.Add(item);
		return item;
	}

	PlaceListItem AddPlaceListItem(PlaceListItemData data)
	{
		PlaceListItem item = Instantiate(placeItemPrefab, placesHolder)
			.GetComponent<PlaceListItem>();
		item.Init(data);
		placesShown.Add(item);
		return item;
	}

	void ClearPlaceItems()
	{
		foreach (var p in placesShown) Destroy(p);
		placesShown.Clear();
	}

	public void ToggleSidebar()
	{
		foreach (var a in HidingTweens)
			if (IsHidden) a.DOPlayForward();
			else a.DOPlayBackwards();
		IsHidden = !IsHidden;
	}

	public bool GoToPage(Page page)
	{
		if (Math.Abs(page - currentPage) != 1)
			return false;

		float sidebarWidth = this.RectTransform().rect.width;
		foreach (var a in PagesTweens)
		{
			a.endValueV3 = new Vector3(sidebarWidth * (page > currentPage ? -1 : 1), 0, 0);
			a.DORestart(true);
		}

		currentPage = page;

		if (page == Page.Places && currentItinerary)
			Flyby.Instance.StartFlyby(currentItinerary, 5f);
		else if (page == Page.PlaceDetails && placeDetailsPage?.currentPlace?.data?.placeDetails != null)
			Flyby.Instance.StartFlyby(placeDetailsPage.currentPlace.data.placeDetails);
		else
			Flyby.Instance.StopFlyby();

		return true;
	}

	public bool IsPlaceOnCurrentItinerary(PlaceDetails place)
	{
		return currentPage != Page.Itineraries ?
			false :
			placesShown.Any(i => i.data.placeDetails.result.place_id == place.result.place_id);
	}

	void ZoomToSeeAllPlaces()
	{
		if (placesShown.Count > 0)
		{
			// move camera to see all places
			var positions = placesShown.Select(p => p.data.pos);
			Vector3 center = positions
				.Aggregate(Vector3.zero, (total, next) => total + next)
				/ placesShown.Count;
			float maxDist = positions
				.Aggregate(0f, (total, next) => Mathf.Clamp((next - center).magnitude, total, float.MaxValue));
			MapCamera.Instance.Reset(center, Quaternion.Euler(90, 0, 0), maxDist * 2f);
		}
	}

	void ValidateArrivalTimings(IOrderedEnumerable<PlaceListItem> placesOrdered)
	{
		List<DateTime> arrivals = placesOrdered
			.Select(p => p.data.place.ArrivalDateTime())
			.ToList();
		for (int i = 0; i < arrivals.Count; ++i)
		{
			bool isValid = true;
			for (int j = 0; j < i; ++j)
			{
				if (arrivals[i] < arrivals[j])
				{
					isValid = false;
					break;
				}
			}
			placesOrdered.ElementAt(i).SetArrivalValid(isValid);
		}
	}

	#region Triggers
	public void OnClickItineraryItem(ItineraryListItem item)
	{
		itineraryNameInput.text = item.itinerary.name;
		StartCoroutine(GetPlacesInItineraryCoroutine(item));
	}

	public void OnClickPlaceItem(PlaceListItem item)
	{
		if (item.data.placeDetails != null)
			MapCamera.Instance.SetCameraViewport(item.data.placeDetails.result.geometry);

		placeDetailsPage.Load(item);
		// Place Details Page will call GoToPage
	}

	public void OnClickReturnToItineraries()
	{
		currentItinerary = null;
		currentDistanceMatrix = null;
		GoToPage(Page.Itineraries);
		ClearPlaceItems();
	}

	public void OnClickReturnToPlaces()
	{
		GoToPage(Page.Places);
		ZoomToSeeAllPlaces();
	}

	public void OnSubmitNewItinerary(string itineraryName)
	{
		StartCoroutine(AddItineraryCoroutine(itineraryName));
	}

	public void OnSubmitRenameItinerary()
	{
		itineraryRenameToggle.isOn = false;
		currentItinerary.itinerary.name = itineraryNameInput.text;
		StartCoroutine(EditItineraryCoroutine(currentItinerary.itinerary));
	}

	public void OnCancelRenameItinerary()
	{
		itineraryNameInput.text = currentItinerary.itinerary.name;
		itineraryRenameToggle.isOn = false;
	}

	public void OnClickAddPlaceTooltip(PlaceDetails place)
	{
		StartCoroutine(AddPlaceCoroutine(currentItinerary, place));
	}

	public void OnClickRemovePlaceTooltip(PlaceDetails place)
	{
		StartCoroutine(RemovePlaceCoroutine(currentItinerary, place));
	}

	public void OnElementAdded(ReorderableList.ReorderableListEventStruct e)
	{
		var placesOrdered = placesShown.OrderBy(p => p.transform.GetSiblingIndex());

		// move around elements in the distance matrix
		// don't have to call Google again, also instant change
		var orig = new System.Collections.ObjectModel.ObservableCollection<string>(
			currentDistanceMatrix.origin_addresses);
		var dest = new System.Collections.ObjectModel.ObservableCollection<string>(
			currentDistanceMatrix.destination_addresses);
		var rows = new System.Collections.ObjectModel.ObservableCollection<DistanceMatrix.Row>(
			currentDistanceMatrix.rows);
		orig.Move(e.FromIndex, e.ToIndex);
		dest.Move(e.FromIndex, e.ToIndex);
		rows.Move(e.FromIndex, e.ToIndex);
		foreach (var r in rows)
		{
			var elems = new System.Collections.ObjectModel.ObservableCollection<DistanceMatrix.Element>(
				r.elements);
			elems.Move(e.FromIndex, e.ToIndex);
			r.elements = elems.ToArray();
		}
		currentDistanceMatrix.origin_addresses = orig.ToArray();
		currentDistanceMatrix.destination_addresses = dest.ToArray();
		currentDistanceMatrix.rows = rows.ToArray();

		//currentDistanceMatrix = null;
		//StartCoroutine(GetTravelTimesCoroutine(placesOrdered.Select(p => p.data.place)));

		foreach (var p in placesOrdered)
		{
			int index = p.transform.GetSiblingIndex();
			index = index > e.ToIndex ? index - 1 : index;

			p.data.place.itineraryindex = index;
			StartCoroutine(EditPlaceCoroutine(p));
			StartCoroutine(p.GetTravelTimes());
		}

		ValidateArrivalTimings(placesOrdered);
	}
	#endregion

	#region Coroutines
	IEnumerator GetItinerariesCoroutine()
	{
		LoginResult user = PersistentUser.User;
		string url = string.Format(
			GetItinerariesResult.URL,
			WWW.EscapeURL(user.userid));
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

		foreach (var itinerary in json.itineraries.OrderBy(i => i.name))
		{
			AddItineraryListItem(itinerary);
		}

		currentItinerary = null;

	}

	IEnumerator GetPlacesInItineraryCoroutine(ItineraryListItem itinerary)
	{
		if (itinerary.placesData.Count == 0)
		{
			string url = string.Format(
				GetPlacesResult.URL,
				WWW.EscapeURL(itinerary.itinerary.itineraryid));
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
				var placesOrdered = json.places.OrderBy(p => p.itineraryindex);
				StartCoroutine(GetTravelTimesCoroutine(placesOrdered.ToList()));
				foreach (var place in placesOrdered)
				{
					var newItem = AddPlaceListItem(place);
					itinerary.placesData.Add(newItem.data);
				}

				ValidateArrivalTimings(placesShown.OrderBy(p => p.data.place.itineraryindex));
			}
		}
		else
		{
			foreach (var data in itinerary.placesData)
			{
				AddPlaceListItem(data);
			}
			StartCoroutine(GetTravelTimesCoroutine(itinerary.placesData.Select(d => d.place).ToList()));
		}

		yield return new WaitUntil(() => placesShown.All(p => !p.isLoading));

		currentItinerary = itinerary;
		GoToPage(Page.Places);
		ZoomToSeeAllPlaces();
		ItineraryLabels.Instance.Init(itinerary.itinerary);
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		string url = string.Format(
			PlaceDetails.URL,
			WWW.EscapeURL(place_id),
			"name,geometry,place_id");
		WWW www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(
			place_id,
			PlaceDetails.Fields.name |
			PlaceDetails.Fields.geometry |
			PlaceDetails.Fields.place_id)));
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
		string url = string.Format(
			AddItineraryResult.URL,
			WWW.EscapeURL(user.userid),
			WWW.EscapeURL(itineraryName),
			AddItineraryResult.DefaultColors);
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
		string url = string.Format(
			AddPlaceResult.URL,
			WWW.EscapeURL(itinerary.itinerary.itineraryid),
			WWW.EscapeURL(place.result.place_id));
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
			itinerary.placesData.Add(newItem.data);
		}
	}

	IEnumerator RemovePlaceCoroutine(ItineraryListItem itineraryItem, PlaceDetails placeDetails)
	{
		string url = string.Format(RemovePlaceResult.URL,
			WWW.EscapeURL(itineraryItem.itinerary.itineraryid),
			WWW.EscapeURL(placeDetails.result.place_id));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		RemovePlaceResult json = JsonUtility.FromJson<RemovePlaceResult>(www.text);
		if (json.error != null)
		{
			Debug.Log(json.error);
			yield break;
		}
		else if (www.text == "OK")
		{
			Destroy(itineraryItem.gameObject);
		}
	}

	IEnumerator EditItineraryCoroutine(Itinerary itinerary)
	{
		string url = string.Format(EditItineraryResult.URL,
			WWW.EscapeURL(itinerary.itineraryid),
			WWW.EscapeURL(itinerary.name),
			WWW.EscapeURL(itinerary.rating.ToString()),
			WWW.EscapeURL(itinerary.is_public.ToString()),
			WWW.EscapeURL(itinerary.deleted.ToString()),
			WWW.EscapeURL(itinerary.colors));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		EditItineraryResult result = JsonUtility.FromJson<EditItineraryResult>(www.text);
		if (result.error != null)
		{
			Debug.Log(result.error);
			yield break;
		}
		else
		{
		}
	}

	IEnumerator GetTravelTimesCoroutine(IEnumerable<Place> places)
	{
		string[] placeIDs = places.Select(p => p.googleid).ToArray();
		WWW www = new WWW(PHPProxy.Escape(DistanceMatrix.BuildURL(placeIDs, placeIDs)));
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		DistanceMatrix dm = new DistanceMatrix();
		dm = JsonUtility.FromJson<DistanceMatrix>(www.text);

		if (dm.status != "OK")
		{
			Debug.Log(dm.error_message);
			yield break;
		}

		for (int i = 0; i < places.Count(); ++i)
			dm.origin_addresses[i] = dm.destination_addresses[i] = places.ElementAt(i).googleid;

		currentDistanceMatrix = dm;
	}

	IEnumerator EditPlaceCoroutine(PlaceListItem place)
	{
		string url = EditPlaceResult.BuildURL(place.data.place);
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		EditPlaceResult result = JsonUtility.FromJson<EditPlaceResult>(www.text);
		if (result.error != null)
		{
			Debug.Log(result.error);
			yield break;
		}
	}
	#endregion
}