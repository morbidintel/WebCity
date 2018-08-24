using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;
using DG.Tweening;
using PhpDB;
using GoogleMaps;

public class PlaceDetailsPage : Singleton<PlaceDetailsPage>
{
	[SerializeField]
	Text title = null;
	[SerializeField]
	RectTransform content = null;
	[SerializeField]
	GameObject loading = null;
	[SerializeField]
	Image labelColor = null;

	[Header("Photos")]
	[SerializeField]
	Image currentPhoto = null;

	[Header("Place Information")]
	[SerializeField]
	GameObject addressGroup = null;
	[SerializeField]
	Text addressText = null;
	[SerializeField]
	GameObject websiteGroup = null;
	[SerializeField]
	Text websiteText = null;
	[SerializeField]
	GameObject phoneGroup = null;
	[SerializeField]
	Text phoneText = null;
	[SerializeField]
	GameObject hoursGroup = null;
	[SerializeField]
	Text hoursText = null;

	[Header("Labels")]
	[SerializeField]
	Text labelTitle = null;
	[SerializeField]
	ToggleGroup labelsToggleGroup = null;
	[SerializeField]
	DOTweenAnimation[] labelsAnims = null;

	[Header("Arrival Time")]
	[SerializeField]
	Text arrivalTitle = null;
	[SerializeField]
	DOTweenAnimation[] arrivalAnims = null;
	[SerializeField]
	UI.Dates.DatePicker datePicker = null;
	[SerializeField]
	Dropdown hoursDropdown = null, minutesDropdown = null;

	public PlaceListItem currentPlace { get; private set; } = null;

	public bool IsLoading { get; private set; } = false;

	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void OpenLink(string url);

	public void Load(PlaceListItem place)
	{
		if (currentPlace != place)
		{
			IsLoading = true;
			currentPlace = place;
			title.text = place.data.placeDetails.result.name;
			Destroy(currentPhoto.sprite);
			currentPhoto.color = Color.clear;
			loading.gameObject.SetActive(true);
			UpdateLabels(place.data.place);
			UpdateArrivalTime(place.data.place);
			OnToggleArrivalContent(false);
			OnToggleLabelsContent(false);

			StartCoroutine(GetPhotos(place.data.place.googleid));
			StartCoroutine(GetDetails(place.data.place.googleid));
		}
		else
		{
			IsLoading = false;
			Sidebar.Instance.GoToPage(Sidebar.Page.PlaceDetails);
		}

		title.rectTransform.sizeDelta = title.rectTransform.sizeDelta.SetY(title.preferredHeight);
		GetComponentInChildren<ScrollRect>().verticalScrollbar.value = 1;
	}

	public void ForceUpdateLayout()
	{
		LayoutRebuilder.MarkLayoutForRebuild(content);
	}

	public void OnClickWebsite()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		OpenLink("http://" + websiteText.text);
		Debug.Log("Opening website: " + websiteText.text);
#else
		Application.OpenURL(websiteText.text);
#endif
	}

	public void OnClickSave()
	{
		if (!datePicker.SelectedDate.HasValue)
			return;

		Place place = currentPlace.data.place;
		DateTime arrival = datePicker.SelectedDate
			.Date
			.AddHours(hoursDropdown.value)
			.AddMinutes(minutesDropdown.value * 5);
		place.SetArrivalTime(arrival);

		StartCoroutine(EditPlaceCoroutine(place));
	}

	void UpdateArrivalTime(Place place)
	{
		arrivalTitle.text = "Arrival Time";
		if (place?.arrivaltime != "")
		{
			DateTime arrival = place.ArrivalDateTime();
			arrivalTitle.text += ": " + arrival.ToString(Place.timeDisplayFormat);
			datePicker.SelectedDate = new UI.Dates.SerializableDate(arrival);
			hoursDropdown.value = arrival.Hour;
			minutesDropdown.value = Mathf.FloorToInt(arrival.Minute / 5f);
		}
		else
		{
			datePicker.SelectedDate = new UI.Dates.SerializableDate();
		}
	}

	public void OnToggleArrivalContent(bool on)
	{
		foreach (var anim in arrivalAnims)
		{
			if (on) anim.DOPlayBackwards();
			else anim.DORestart();
		}
	}

	public void OnClickRemove()
	{
		Sidebar.Instance.OnClickRemovePlaceTooltip(currentPlace.data.placeDetails);
		loading.SetActive(true);
		StartCoroutine(RemovePlaceCoroutine());
	}

	public void UpdateLabels(Place place, bool updateToggles = true)
	{
		bool validId =
			place != null &&
			place.labelid != -1 &&
			place.labelid < ItineraryLabels.Colors.Length;
		labelColor.color = validId ?
			ItineraryLabels.Colors[place.labelid] : Color.white;
		labelTitle.text = validId ?
			Sidebar.Instance.currentItinerary.itinerary.GetLabels()[place.labelid] :
			"No label";
		currentPlace.SetLabel(place.labelid);
		if (updateToggles)
			foreach (var toggle in labelsToggleGroup.GetAllToggles())
				toggle.isOn = toggle.transform.GetSiblingIndex() == place.labelid;
	}

	public void OnToggleLabelsContent(bool on)
	{
		foreach (var anim in labelsAnims)
		{
			if (on) anim.DOPlayBackwards();
			else anim.DORestart();
		}
	}

	public void OnToggleLabelColor(Toggle toggle)
	{
		int index = toggle.transform.GetSiblingIndex(),
			placelabelid = currentPlace.data.place.labelid,
			newlabelid = -1;
		Debug.Log(index + " " + toggle.isOn + " " + Time.frameCount);

		newlabelid = toggle.isOn ? index : index == placelabelid ? -1 : placelabelid;

		if (newlabelid != currentPlace.data.place.labelid)
		{
			currentPlace.data.place.labelid = newlabelid;
			StopAllCoroutines();
			StartCoroutine(EditPlaceCoroutine(currentPlace.data.place));
		}

		UpdateLabels(currentPlace.data.place, false);
	}

	IEnumerator GetPhotos(string place_id)
	{
		WWW www;
		string photo_reference = "";
		if (currentPlace?.data.placeDetails.result.photos == null)
		{
			www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(
				place_id,
				PlaceDetails.Fields.photo |
				PlaceDetails.Fields.place_id)));
			yield return www;
			Debug.Log(www.url);
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
		else if (currentPlace.data.placeDetails.result.photos.Length == 0)
		{
			yield break;
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
		currentPhoto.sprite = Sprite.Create(www.texture, rect, new Vector2(.5f, .5f));
		currentPhoto.rectTransform.sizeDelta = rect.size;
		currentPhoto.color = Color.white;
		currentPhoto.name = photo_reference;

		loading.gameObject.SetActive(false);
	}

	IEnumerator GetDetails(string place_id)
	{
		WWW www;
		PlaceDetails.Result result = currentPlace?.data.placeDetails.result;
		if (result == null ||
			result.formatted_address == null || result.formatted_address == "" ||
			result.website == null || result.website == "" ||
			result.international_phone_number == null || result.international_phone_number == "" ||
			result.opening_hours.weekday_text == null)
		{
			www = new WWW(PHPProxy.Escape(PlaceDetails.BuildURL(
				place_id,
				PlaceDetails.Fields.formatted_address |
				PlaceDetails.Fields.website |
				PlaceDetails.Fields.international_phone_number |
				PlaceDetails.Fields.opening_hours)));
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

			result = place.result;
		}

		addressGroup.SetActive(result.formatted_address != null && result.formatted_address != "");
		if (addressGroup.activeInHierarchy)
		{
			addressText.text = result.formatted_address;
		}

		websiteGroup.SetActive(result.website != null && result.website != "");
		if (websiteGroup.activeInHierarchy)
		{
			websiteText.text = new Regex(@"^(?:https?:\/\/)?(?:www\.)?")
				.Replace(result.website, "")
				.TrimEnd('/');
		}

		phoneGroup.SetActive(result.international_phone_number != null && result.international_phone_number != "");
		if (phoneGroup.activeInHierarchy)
		{
			phoneText.text = result.international_phone_number;
		}

		hoursGroup.SetActive(result.opening_hours.weekday_text != null);
		if (hoursGroup.activeInHierarchy)
		{
			string hourstext =
				result.opening_hours.weekday_text[((int)DateTime.Now.DayOfWeek + 6) % 7];
			hourstext = new Regex(@"^.{6,9}: ").Replace(hourstext, "");
			hoursText.text = result.opening_hours.open_now ?
				"Open now: " + hourstext : "<color=red>Closed</color>";
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInChildren<ScrollRect>().content);
		Sidebar.Instance.GoToPage(Sidebar.Page.PlaceDetails);
		IsLoading = false;
	}

	IEnumerator EditPlaceCoroutine(Place place)
	{
		yield return new WaitForEndOfFrame();
		string url = EditPlaceResult.BuildURL(place);
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

		currentPlace.UpdatePlace(result.place);
		UpdateArrivalTime(result.place);
		UpdateLabels(result.place);
	}

	IEnumerator RemovePlaceCoroutine()
	{
		yield return new WaitUntil(() => currentPlace == null);
		Sidebar.Instance.GoToPage(Sidebar.Page.Places);
		loading.SetActive(false);
	}
}
