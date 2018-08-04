using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;
using GoogleMaps;

public class ItineraryListItem : MonoBehaviour
{
	public Itinerary itinerary { get; private set; }

	[SerializeField]
	Text nameLabel = null;
	[SerializeField]
	Button button = null;
	[SerializeField]
	GameObject loading = null;

	public List<PlaceListItemData> placesData = new List<PlaceListItemData>();

	public void Init(Itinerary data)
	{
		itinerary = data;
		nameLabel.text = itinerary.name;
		loading.gameObject.SetActive(false);
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() =>
		{
			Sidebar.Instance.OnClickItineraryItem(this);
			loading.gameObject.SetActive(true);
			StartCoroutine(StopLoadingImageCoroutine());
		});
	}

	IEnumerator StopLoadingImageCoroutine()
	{
		yield return new WaitUntil(() => Sidebar.Instance.currentPage == Sidebar.Page.Places);
		loading.gameObject.SetActive(false);
	}
}
