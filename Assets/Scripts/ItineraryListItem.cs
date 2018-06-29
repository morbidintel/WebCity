using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;

public class ItineraryListItem : MonoBehaviour
{
	public Itinerary itinerary { get; private set; }

	[SerializeField]
	Text nameLabel = null;
	[SerializeField]
	Button button = null;
	[SerializeField]
	Image loading = null;

	public List<PlaceListItem> places = new List<PlaceListItem>();

	public void Init(Itinerary data)
	{
		itinerary = data;
		nameLabel.text = itinerary.name;
		loading.gameObject.SetActive(false);
		button.onClick.AddListener(() =>
		{
			Sidebar.Instance.OnClickItineraryItem(this);
			loading.gameObject.SetActive(true);
			StartCoroutine(StopLoadingImageCoroutine());
		});
	}

	IEnumerator StopLoadingImageCoroutine()
	{
		yield return new WaitUntil(() => !Sidebar.Instance.ShowItineraries);
		loading.gameObject.SetActive(false);
	}
}
