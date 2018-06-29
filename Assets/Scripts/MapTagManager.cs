using System.Collections;
using System.Collections.Generic;
using GoogleMaps;
using UnityEngine;

public class MapTagManager : Gamelogic.Extensions.Singleton<MapTagManager>
{
	[SerializeField]
	GameObject tagPrefab = null;
	[SerializeField]
	Transform tagsHolder = null;

	List<MapTag> tags = new List<MapTag>();

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void ShowPlaceOnMap(PlaceDetails place, Color? color = null)
	{
		if (place?.result?.geometry == null || place?.result?.name == null) return;

		Vector3 pos = MapCamera.LatLongToUnity(place.result.geometry.location);
		MapTag tag = Instantiate(tagPrefab, tagsHolder).GetComponent<MapTag>();
		tag.transform.position = pos;
		tag.placeName.text = place.result.name;
		tag.placeName.color = color.HasValue ? color.Value : tag.placeName.color;

		tags.Add(tag);
	}

	public void ClearMapTags()
	{
		foreach (var t in tags) Destroy(t);
		tags.Clear();
	}
}
