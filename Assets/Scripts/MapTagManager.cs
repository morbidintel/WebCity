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

	public MapTag ShowPlaceOnMap(PlaceDetails place, Color? color = null)
	{
		if (place?.result?.geometry == null || place?.result?.name == null) return null;

		Vector3 pos = MapCamera.LatLongToUnity(place.result.geometry.location);
		MapTag tag = Instantiate(tagPrefab, tagsHolder).GetComponent<MapTag>();
		tag.transform.position = pos;
		tag.placeName.text = place.result.name;
		tag.placeName.color = color.HasValue ? color.Value : tag.placeName.color;

		tags.Add(tag);
		return tag;
	}

	public MapTag ShowPlaceOnMap(Coords location, string name, Color? color = null)
	{
		if (location == null || name == null) return null;

		Vector3 pos = MapCamera.LatLongToUnity(location);
		MapTag tag = Instantiate(tagPrefab, tagsHolder).GetComponent<MapTag>();
		tag.transform.position = pos;
		tag.placeName.text = name;
		tag.placeName.color = color.HasValue ? color.Value : tag.placeName.color;

		tags.Add(tag);
		return tag;
	}

	public void ClearMapTag(MapTag tag)
	{
		if (tags.Remove(tag)) Destroy(tag);
	}

	public void ClearMapTags()
	{
		foreach (var t in tags) Destroy(t);
		tags.Clear();
	}
}
