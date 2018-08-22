using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;
using GoogleMaps;
using UnityEngine.UI.Extensions;

/// <summary>
/// Record class to hold both the DB Place and Google Place
/// </summary>
public class PlaceData
{
	public Place place = null;
	public PlaceDetails placeDetails = null;
	public Vector3 pos = Vector3.zero; // Unity position
	public PlaceData() { }
	public PlaceData(Place place, PlaceDetails placeDetails, Vector3 pos)
	{
		this.place = place;
		this.placeDetails = placeDetails;
		this.pos = pos;
	}
	public PlaceData(PlaceData data)
	{
		place = data.place;
		placeDetails = data.placeDetails;
		pos = data.pos;
	}
}

/// <summary>
/// Instantiated class that handles each entry in the Places list.
/// </summary>
public class PlaceListItem : MonoBehaviour
{
	/// <summary>
	/// The DB Place and Google Place data represented by this item.
	/// </summary>
	public PlaceData data { get; private set; } = new PlaceData();

	/// <summary>
	/// Text fields to be shown.
	/// </summary>
	[SerializeField]
	Text nameLabel = null, arrivalTimeLabel = null, travelTimeLabel = null;
	/// <summary>
	/// Button under this place item that goes to the Place Details page in the Sidebar.
	/// </summary>
	[SerializeField]
	Button button = null;
	/// <summary>
	/// Loading icon which is shown when loading the Place Details.
	/// </summary>
	[SerializeField]
	GameObject loading = null;
	/// <summary>
	/// Small color strip at the side that represents the label assigned to the Place.
	/// </summary>
	[SerializeField]
	Image labelColor = null;
	/// <summary>
	/// Component that allows dragging and reordering of this item.
	/// </summary>
	[SerializeField]
	ReorderableListElement reorderableListElement = null;

	// Created map tag on the world map that represents this Place.
	MapTag mapTag = null;

	/// <summary>
	/// Is the Place List Item still retrieving Google Place information?
	/// </summary>
	public bool IsLoading { get; private set; } = false;
	/// <summary>
	/// Is the Place List Item being dragged out of the list to be reordered?
	/// </summary>
	public bool IsReordering { get; private set; } = false;

	void OnDestroy()
	{
		// remove the map tag for this place also
		if (mapTag) MapTagManager.Instance?.ClearMapTag(mapTag);
		Destroy(gameObject);
	}

	void Update()
	{
		// prevent clicking of the place item when item is being dragged
		button.interactable = !reorderableListElement._isDragging;
	}

	/// <summary>
	/// Initialize the item with Place data from database
	/// and retrieve information of the Google Place
	/// </summary>
	/// <param name="place">DB Place to be represented by this item.</param>
	public void Init(Place place)
	{
		data.place = place;
		// start getting the Google Place information
		StartCoroutine(GetPlaceCoroutine(place.googleid));
		// set the label color
		SetLabel(place.labelid);
		loading.gameObject.SetActive(false);
		IsLoading = true;

		// show arrival time if there's any
		if (!string.IsNullOrEmpty(place?.arrivaltime))
			arrivalTimeLabel.text = 
				place.ArrivalDateTime()
				.ToString(Place.timeDisplayFormat);
	}

	/// <summary>
	/// Initialize the item with a PlaceData
	/// and retrieve information of the Google Place
	/// </summary>
	/// <param name="data">PlaceData that has a valid Place from the database</param>
	public void Init(PlaceData data)
	{
		Init(data.place);
	}

	/// <summary>
	/// Start the process of retrieving data for all the details for the Place.
	/// </summary>
	/// <remarks>
	/// Called by Item => Itinerary Name -> Button.OnClick event.
	/// </remarks>
	public void OnClickPlaceItem()
	{
		loading.gameObject.SetActive(true);
		Sidebar.Instance.OnClickPlaceItem(this);
		StartCoroutine(StopLoadingImageCoroutine());
	}

	/// <summary>
	/// Update information of the Place.
	/// </summary>
	/// <param name="place">DB Place to be represented by this item.</param>
	public void UpdatePlace(Place place)
	{
		data.place = place;
		if (place?.arrivaltime != "")
		{
			arrivalTimeLabel.text = place.ArrivalDateTime().ToString(Place.timeDisplayFormat);
		}
	}

	/// <summary>
	/// Set the arrival text according to whether the arrival text
	/// is valid (is earlier than previous Place's arrival time).
	/// </summary>
	/// <param name="valid">Is the arrival time valid?</param>
	public void SetArrivalTextValid(bool valid)
	{
		// use rich text so as to not directly change label color
		// label color not directly changed to preserve original color
		if (valid)
		{
			arrivalTimeLabel.text =
				arrivalTimeLabel.text
				.Replace("<color=red>", "")
				.Replace("</color>", "");
		}
		else
		{
			arrivalTimeLabel.text =
				"<color=red>" +
				arrivalTimeLabel.text +
				"</color>";
		}
	}

	/// <summary>
	/// Set the label color strip according to the label id.
	/// </summary>
	/// <param name="labelid">Index of the label assigned to this Place. Must be from 0 to 9.</param>
	public void SetLabel(int labelid)
	{
		labelColor.color = labelid != -1 && labelid < ItineraryLabels.Colors.Length ?
			ItineraryLabels.Colors[labelid] : Color.white;
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		if (place_id == "") yield break;

		// do we need to retrieve Place Details?
		if (data.placeDetails == null || place_id != data.placeDetails.result.place_id)
		{
			WWW www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(place_id,
				PlaceDetails.Fields.name | PlaceDetails.Fields.geometry |
				PlaceDetails.Fields.photo | PlaceDetails.Fields.place_id)));
			yield return www;
			if (www.error != null)
			{
				Debug.Log(www.error);
				yield break;
			}

			data.placeDetails = JsonUtility.FromJson<PlaceDetails>(www.text);
		}

		if (data.placeDetails.status != "OK")
		{
			Debug.Log(data.placeDetails.error_message);
			yield break;
		}

		nameLabel.text = gameObject.name = data.placeDetails.result.name;
		
		yield return GetTravelTimes();

		data.pos = MapCamera.LatLongToUnity(data.placeDetails.result.geometry.location);
		// create a map tag on the world map
		mapTag = MapTagManager.Instance.ShowPlaceOnMap(data.placeDetails);
		IsLoading = false; // done with loading
	}

	/// <summary>
	/// Public coroutine to update travel times of this place item
	/// from the Distance Matrix data retrieve from Google Maps
	/// </summary>
	public IEnumerator GetTravelTimes()
	{
		yield return new WaitUntil(() => Sidebar.Instance.currentDistanceMatrix != null);

		// retrieve the duration text
		travelTimeLabel.text = Sidebar.Instance.currentDistanceMatrix
			.GetDurationToNextPlace(data.place.googleid);
	}

	IEnumerator StopLoadingImageCoroutine()
	{
		// page will change when all the Google Places are loaded
		yield return new WaitUntil(() => Sidebar.Instance.currentPage == Sidebar.Page.PlaceDetails);
		// hide the loading icon
		loading.gameObject.SetActive(false);
	}
}
