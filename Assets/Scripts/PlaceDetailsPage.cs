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

	PlaceListItem place;

	public bool IsLoading { get; private set; } = false;

	public void Init(PlaceListItem place)
	{
		IsLoading = true;
		this.place = place;

		Destroy(image.sprite);
		image.color = Color.clear;
		loading.gameObject.SetActive(true);
		image.rectTransform.sizeDelta = image.rectTransform.sizeDelta.SetY(loading.rectTransform.rect.height);

		StartCoroutine(GetPhotos(place.data.place.googleid));
	}

	IEnumerator GetPhotos(string place_id)
	{
		WWW www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(
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

		www = new WWW(PHPProxy.Escape(PlacePhotos.BuildURL(
			place.result.photos[0].photo_reference,
			(int)this.RectTransform().rect.width, 0)));
		yield return www;
		if (www.error != null)
		{
			Debug.Log(www.error + "\n" + www.url);
			yield break;
		}
		
		Rect rect = new Rect(0, 0, www.texture.width, www.texture.height);
		image.sprite = Sprite.Create(www.texture, rect, new Vector2(.5f, .5f));
		image.rectTransform.sizeDelta = image.rectTransform.sizeDelta.SetY(rect.height);
		image.color = Color.white;

		loading.gameObject.SetActive(false);
		IsLoading = false;
	}
}
