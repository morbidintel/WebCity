using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;
using GoogleMaps;
using DG.Tweening;

/// <summary>
/// Instantiated class that handles each entry in the Itineraries list.
/// </summary>
public class ItineraryListItem : MonoBehaviour
{
	/// <summary>
	/// The Itinerary represented by this item.
	/// </summary>
	public Itinerary itinerary { get; private set; }

	/// <summary>
	/// Text field that shows the name of the Itinerary.
	/// </summary>
	[SerializeField] Text nameLabel = null;
	/// <summary>
	/// Loading icon which is shown when loading the Itinerary.
	/// </summary>
	[SerializeField] GameObject loading = null;
	/// <summary>
	/// Tween that is played when Itineraries edit mode is toggled.
	/// </summary>
	[SerializeField] DOTweenAnimation removeTween = null;

	/// <summary>
	/// The data of all the places that belong to this Itinerary.
	/// </summary>
	public List<PlaceData> placesData = new List<PlaceData>();

	/// <summary>
	/// Initialize the item with Itinerary data.
	/// </summary>
	/// <param name="data">Itinerary to be represented by this item.</param>
	public void Init(Itinerary data)
	{
		itinerary = data;
		nameLabel.text = itinerary.name;
		loading.gameObject.SetActive(false);
	}

	/// <summary>
	/// Start the process of retrieving data for all the places in the Itinerary.
	/// </summary>
	/// <remarks>
	/// Called by Item => Itinerary Name -> Button.OnClick event.
	/// </remarks>
	public void OnClickItineraryItem()
	{
		Sidebar.Instance.OnClickItineraryItem(this);
		loading.gameObject.SetActive(true);
		StartCoroutine(StopLoadingImageCoroutine());
	}

	/// <summary>
	/// Calls to request the database to (soft) delete this Itinerary.
	/// </summary>
	/// <remarks>
	/// Called by Item => Remove Button -> Button.OnClick event.
	/// </remarks>
	public void OnClickRemove()
	{
		Sidebar.Instance.OnClickRemoveItinerary(this);
		loading.gameObject.SetActive(true);
	}

	/// <summary>
	/// Plays the tween to show the Remove button.
	/// </summary>
	/// <param name="isOn">Is the button being toggled on?</param>
	public void ToggleRemoveButton(bool isOn)
	{
		if (isOn)
			removeTween.DORestart();
		else
			removeTween.DOPlayBackwards();
	}

	IEnumerator StopLoadingImageCoroutine()
	{
		// page will change when all the Google Places are loaded
		yield return new WaitUntil(() => Sidebar.Instance.currentPage == Sidebar.Page.Places);
		// hide the loading icon
		loading.gameObject.SetActive(false);
	}
}
