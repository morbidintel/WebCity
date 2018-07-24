using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMaps;
using Gamelogic.Extensions;

public class PlaceDetailsPage : Singleton<PlaceDetailsPage>
{
	[SerializeField]
	Text title = null;
	[SerializeField]
	Image image = null, loading = null;
	[SerializeField]
	UI.Dates.DatePicker datePicker = null;
	[SerializeField]
	Dropdown hoursDropdown = null, minutesDropdown = null;

	PlaceListItem currentPlace;

	public bool IsLoading { get; private set; } = false;

	public void Init(PlaceListItem place)
	{
		if (this.currentPlace == place) return;

		IsLoading = true;
		currentPlace = place;
		title.text = place.data.placeDetails.result.name;
		Destroy(image.sprite);
		image.color = Color.clear;
		loading.gameObject.SetActive(true);

		// TODO: get the time, set the time
		if (place.data.place.arrivaltime != null)
		{
		}

		StartCoroutine(GetPhotos(place.data.place.googleid));
	}

	IEnumerator GetPhotos(string place_id)
	{
		WWW www;
		string photo_reference = "";
		if (currentPlace.data.placeDetails.result.photos == null)
		{
			www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(
				place_id,
				PlaceDetails.Fields.photo |
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

			if (place.result?.photos?.Length == 0) yield break;

			photo_reference = place.result.photos[0].photo_reference;
		}
		else
		{
			photo_reference = currentPlace.data.placeDetails.result.photos[0].photo_reference;
		}


		www = new WWW(PHPProxy.Escape(PlacePhotos.BuildURL(
			photo_reference,
			(int)this.RectTransform().rect.width, 0)));
		yield return www;
		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		Rect rect = new Rect(0, 0, www.texture.width, www.texture.height);
		image.sprite = Sprite.Create(www.texture, rect, new Vector2(.5f, .5f));
		image.rectTransform.sizeDelta = rect.size;
		image.color = Color.white;

		loading.gameObject.SetActive(false);
		IsLoading = false;
	}
}
