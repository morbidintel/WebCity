using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;
using GoogleMaps;
using DG.Tweening;

public class ItineraryListItem : MonoBehaviour
{
	public Itinerary itinerary { get; private set; }

	[SerializeField]
	Text nameLabel = null;
	[SerializeField]
	Button button = null;
	[SerializeField]
	GameObject loading = null;
	[SerializeField]
	Button remove = null;
	[SerializeField]
	DOTweenAnimation removeAnimation = null;

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
		remove.onClick.RemoveAllListeners();
		remove.onClick.AddListener(() =>
		{
			Sidebar.Instance.OnClickRemoveItinerary(this);
			loading.gameObject.SetActive(true);
		});
	}

	public void ToggleRemoveButton(bool isOn)
	{
		if (isOn)
			removeAnimation.DORestart();
		else
			removeAnimation.DOPlayBackwards();
	}

	IEnumerator StopLoadingImageCoroutine()
	{
		yield return new WaitUntil(() => Sidebar.Instance.currentPage == Sidebar.Page.Places);
		loading.gameObject.SetActive(false);
	}
}
