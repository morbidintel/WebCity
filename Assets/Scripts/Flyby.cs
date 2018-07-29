using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Extensions;
using GoogleMaps;

public class Flyby : Singleton<Flyby>
{
	List<GoogleMaps.Geometry> geometries;
	public bool isDoingFlyby { get; private set; } = false;

	void Update()
	{
		if (isDoingFlyby)
		{
			MapCamera cam = MapCamera.Instance;
			cam.TargetElevation = 45;
			cam.TargetAzimuth -= Time.deltaTime * cam.rotateAnimationSpeed;
		}
	}

	public void StartFlyby(ItineraryListItem itinerary)
	{
		geometries = itinerary.placesData
			.Select(d => d.placeDetails.result.geometry)
			.ToList();

		StartCoroutine(FlybyCoroutine());
		isDoingFlyby = true;
	}

	public void StopFlyby()
	{
		StopAllCoroutines();
		isDoingFlyby = false;
	}

	IEnumerator FlybyCoroutine()
	{
		foreach (var g in geometries)
		{
			MapCamera.Instance.SetCameraViewport(g, nospin: true);
			yield return new WaitForSecondsRealtime(5.0f);
		}
		StartCoroutine(FlybyCoroutine());
	}
}
