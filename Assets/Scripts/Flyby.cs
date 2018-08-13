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

	float expectedAzimuth, expectedElevation;
	Vector3 expectedPosition;

	void Update()
	{
		if (isDoingFlyby)
		{
			MapCamera cam = MapCamera.Instance;

			if (cam.HasMoved)
			{
				StopFlyby();
			}
			else
			{
				cam.TargetAzimuth -= Time.deltaTime * cam.rotateAnimationSpeed * 2;
				expectedAzimuth = Mathf.Repeat(cam.TargetAzimuth, 360f);
			}
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
			float dist = Mathf.Clamp(diag / 2f, 0.5f, MapCamera.Instance.maxDistance);
			MapCamera cam = MapCamera.Instance;
			cam.SetCameraViewport(g, dist, true);

			expectedPosition = cam.TargetPosition;
			expectedAzimuth = cam.RealAzimuth;
			expectedElevation = cam.TargetElevation = 30f;
			yield return new WaitForSecondsRealtime(flybyInterval);
		}

		StartCoroutine(FlybyCoroutine(0f));
	}
}
