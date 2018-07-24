using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;
using GoogleMaps;

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

public class PlaceListItem : MonoBehaviour
{
	public PlaceListItemData data { get; private set; } = new PlaceListItemData();

	[SerializeField]
	Text nameLabel = null, arrivalTimeLabel = null, travelTimeLabel = null;
	[SerializeField]
	Button button = null;

	MapTag mapTag = null;

	public bool isLoading { get; private set; } = false;

	void OnDestroy()
	{
		if (mapTag) MapTagManager.Instance?.ClearMapTag(mapTag);
		Destroy(gameObject);
	}

	public void Init(Place place)
	{
		data.place = place;
		StartCoroutine(GetPlaceCoroutine(place.googleid));
		button.onClick.AddListener(() => Sidebar.Instance.OnClickPlaceItem(this));
		isLoading = true;
	}

	public void Init(PlaceListItemData data)
	{
		this.data = data;
		StartCoroutine(GetPlaceCoroutine(data.place.googleid));
		button.onClick.AddListener(() => Sidebar.Instance.OnClickPlaceItem(this));
		isLoading = true;
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		if (place_id == "") yield break;
		if (data.placeDetails == null)
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
		
		nameLabel.text = data.placeDetails.result.name;
		if (data.place?.arrivaltime != "")
		{
			DateTime arrival;
			if (DateTime.TryParseExact(data.place.arrivaltime, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out arrival))
				arrivalTimeLabel.text = arrival.ToString("dd MMM HH:mm");
		}

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

		isLoading = false;
		data.pos = MapCamera.LatLongToUnity(data.placeDetails.result.geometry.location);
		mapTag = MapTagManager.Instance.ShowPlaceOnMap(data.placeDetails);
	}
}
