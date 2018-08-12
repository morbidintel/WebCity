using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Extensions;
using GoogleMaps;

public class Flyby : Singleton<Flyby>
{
	public static float flybyInterval = 5.0f;

	[SerializeField]
	List<GoogleMaps.Geometry> geometries;
	public bool isDoingFlyby { get; private set; } = false;

	void Update()
	{
		if (isDoingFlyby)
		{
			if (Input.GetMouseButton(0) ||
				Input.GetMouseButton(1) ||
				Input.GetMouseButton(2))
			{
				StopFlyby();
				return;
			}

			MapCamera cam = MapCamera.Instance;
			cam.TargetElevation = 30;
			cam.TargetAzimuth -= Time.deltaTime * cam.rotateAnimationSpeed * 2;
		}
	}

	public void StartFlyby(ItineraryListItem itinerary, float delay = 0)
	{
		geometries = itinerary.placesData
			.Select(d => d.placeDetails.result.geometry)
			.ToList();
		StopFlyby();
		StartCoroutine(FlybyCoroutine(delay));
	}

	public void StartFlyby(PlaceDetails place, float delay = 0)
	{
		geometries = new List<GoogleMaps.Geometry>();
		geometries.Add(place.result.geometry);
		StopFlyby();
		StartCoroutine(FlybyCoroutine(delay));
	}

	public void StopFlyby()
	{
		StopAllCoroutines();
		isDoingFlyby = false;
	}

	IEnumerator FlybyCoroutine(float delay)
	{
		if (delay > 0) yield return new WaitForSecondsRealtime(delay);
		isDoingFlyby = true;

		foreach (var g in geometries)
		{
			var viewport = g.viewport;
			float diag = (MapCamera.LatLongToUnity(viewport.northeast) - MapCamera.LatLongToUnity(viewport.southwest)).magnitude;
			float dist = Mathf.Clamp(diag, 0.5f, MapCamera.Instance.maxDistance) / 2f;
			MapCamera.Instance.SetCameraViewport(g, dist, true);
			yield return new WaitForSecondsRealtime(flybyInterval);
		}

		StartCoroutine(FlybyCoroutine(0f));
	}
}
