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

	public bool isLoading { get; private set; } = false;

	void OnDestroy()
	{
		Destroy(gameObject);
	}

	public void Init(Place place)
	{
		this.data.place = place;
		StartCoroutine(GetPlaceCoroutine(place.googleid));
		button.onClick.AddListener(() => Sidebar.Instance.OnClickPlaceItem(this));
		isLoading = true;
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		if (place_id == "") yield break;
		if (data.placeDetails == null)
		{
			string url = string.Format(PlaceDetails.URL, WWW.EscapeURL(place_id), "name,geometry,place_id");
			WWW www = new WWW(PHPProxy.Escape(url));
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
		if (data.place.arrivaltime != null && data.place.arrivaltime != "")
		{
			DateTime arrival;
			if (DateTime.TryParse(data.place.arrivaltime, out arrival))
				arrivalTimeLabel.text = arrival.ToString("dd MMM HH:mm");
		}
		else
		{
			arrivalTimeLabel.text = "";
		}
		travelTimeLabel.text = "15 mins";
		isLoading = false;
		data.pos = MapCamera.LatLongToUnity(data.placeDetails.result.geometry.location);
		MapTagManager.Instance.ShowPlaceOnMap(data.placeDetails);
	}
}
