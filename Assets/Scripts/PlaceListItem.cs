using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PhpDB;
using GoogleMaps;
using UnityEngine.UI.Extensions;

public class PlaceListItemData
{
	public Place place = null;
	public PlaceDetails placeDetails = null;
	public Vector3 pos = Vector3.zero;
	public PlaceListItemData() { }
	public PlaceListItemData(Place place, PlaceDetails placeDetails, Vector3 pos)
	{
		this.place = place;
		this.placeDetails = placeDetails;
		this.pos = pos;
	}
	public PlaceListItemData(PlaceListItemData data)
	{
		place = data.place;
		placeDetails = data.placeDetails;
		pos = data.pos;
	}
}

public class PlaceListItem : MonoBehaviour//, IPointerDownHandler, IPointerUpHandler
{
	public PlaceListItemData data { get; private set; } = new PlaceListItemData();

	[SerializeField]
	Text nameLabel = null, arrivalTimeLabel = null, travelTimeLabel = null;
	[SerializeField]
	Button button = null;
	[SerializeField]
	GameObject loading = null;
	[SerializeField]
	Image labelColor = null;
	[SerializeField]
	ReorderableListElement reorderableListElement = null;

	MapTag mapTag = null;

	public bool isLoading { get; private set; } = false;
	public bool isReordering { get; private set; } = false;

	void OnDestroy()
	{
		if (mapTag) MapTagManager.Instance?.ClearMapTag(mapTag);
		Destroy(gameObject);
	}

	void Update()
	{
		button.interactable = !reorderableListElement._isDragging;
	}

	public void Init(Place place)
	{
		data.place = place;
		StartCoroutine(GetPlaceCoroutine(place.googleid));
		loading.gameObject.SetActive(false);
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() =>
		{
			loading.gameObject.SetActive(true);
			Sidebar.Instance.OnClickPlaceItem(this);
			StartCoroutine(StopLoadingImageCoroutine());
		});
		isLoading = true;

		if (place?.arrivaltime != "")
		{
			arrivalTimeLabel.text = place.ArrivalDateTime().ToString(Place.timeDisplayFormat);
		}

		SetLabelId(place.labelid);
	}

	public void Init(PlaceListItemData data)
	{
		Init(data.place);
	}

	public void UpdatePlace(Place place)
	{
		data.place = place;
		if (place?.arrivaltime != "")
		{
			arrivalTimeLabel.text = place.ArrivalDateTime().ToString(Place.timeDisplayFormat);
		}
	}

	public void SetArrivalValid(bool valid)
	{
		// label color not directly change to preserve original color
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

	public void SetLabelId(int labelId)
	{
		labelColor.color = labelId != -1 && labelId < ItineraryLabels.colors.Length ?
			ItineraryLabels.colors[labelId] :
			Color.white;
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		if (place_id == "") yield break;
		if (data.placeDetails == null || place_id != data.placeDetails.result.place_id)
		{
			WWW www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(place_id,
			PlaceDetails.Fields.name |
			PlaceDetails.Fields.geometry |
			PlaceDetails.Fields.photo |
			PlaceDetails.Fields.place_id)));
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

		isLoading = false;
		data.pos = MapCamera.LatLongToUnity(data.placeDetails.result.geometry.location);
		mapTag = MapTagManager.Instance.ShowPlaceOnMap(data.placeDetails);
	}

	public IEnumerator GetTravelTimes()
	{
		yield return new WaitUntil(() => Sidebar.Instance.currentDistanceMatrix != null);
		try
		{
			DistanceMatrix dm = Sidebar.Instance.currentDistanceMatrix;
			int index = Array.IndexOf(dm.origin_addresses, data.place.googleid);
			travelTimeLabel.text = dm.rows[index].elements[index + 1].duration.text;
		}
		catch
		{
			travelTimeLabel.text = "";
		}
	}

	IEnumerator StopLoadingImageCoroutine()
	{
		yield return new WaitUntil(() => Sidebar.Instance.currentPage == Sidebar.Page.PlaceDetails);
		loading.gameObject.SetActive(false);
	}
}
