using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;
using GoogleMaps;

public class PlaceListItem : MonoBehaviour
{
	public Place place { get; private set; }
	public PlaceDetails placeDetails = null;

	[SerializeField]
	Text nameLabel = null;
	[SerializeField]
	Button button = null;

	public bool isLoading { get; private set; } = false;

	void OnDestroy()
	{
		Destroy(gameObject);
	}

	public void Init(Place data)
	{
		place = data;
		StartCoroutine(GetPlaceCoroutine(place.googleid));
		button.onClick.AddListener(() => Sidebar.Instance.OnClickPlaceItem(this));
		isLoading = true;
	}

	IEnumerator GetPlaceCoroutine(string place_id)
	{
		if (place_id == "") yield break;
		if (placeDetails != null)
		{
			string url = string.Format(PlaceDetails.URL, WWW.EscapeURL(place_id), "name,geometry");
			WWW www = new WWW(PHPProxy.Escape(url));
			yield return www;
			if (www.error != null)
			{
				Debug.Log(www.error);
				yield break;
			}

			placeDetails = JsonUtility.FromJson<PlaceDetails>(www.text);
		}

		if (placeDetails.status != "OK")
		{
			Debug.Log(placeDetails.error_message);
			yield break;
		}

		nameLabel.text = placeDetails.result.name;
		isLoading = false;
		MapTagManager.Instance.ShowPlaceOnMap(placeDetails);
	}
}
