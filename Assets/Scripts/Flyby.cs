using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Extensions;
using GoogleMaps;

/// <summary>
/// Controls is Flyby feature when in Pages or Place Details pages.
/// </summary>
public class Flyby : Singleton<Flyby>
{
	/// <summary>
	/// Seconds to wait before flying to the next location.
	/// </summary>
	public static float flybyInterval = 5.0f;

	/// <summary>
	/// List of locations to fly to.
	/// </summary>
	/// <remarks>
	/// GoogleMaps.Geometry is used so as to retiain viewport information.
	/// </remarks>
	List<GoogleMaps.Geometry> locations;

	/// <summary>
	/// Flag to check if a flyby is in progress.
	/// </summary>
	public static bool IsDoingFlyby { get; private set; } = false;

	// reference of the MapCamera
	new MapCamera camera;

	void Start()
	{
		camera = MapCamera.Instance;
	}

	void Update()
	{
		if (IsDoingFlyby)
		{
			if (camera.HasMoved)
			{
				// stop flyby if there's any camera movement
				StopFlyby();
			}
			else
			{
				// rotate the camera
				camera.TargetAzimuth -= Time.deltaTime * camera.rotateAnimationSpeed * 2;
			}
		}
	}

	/// <summary>
	/// Start a flyby sequence, with an optional delay.
	/// </summary>
	/// <param name="itinerary">Instantiated itinerary item.</param>
	/// <param name="delay">An optional delay, in seconds.</param>
	public void StartFlyby(ItineraryListItem itinerary, float delay = 0)
	{
		if (itinerary.placesData.Count == 0) return;
		locations = itinerary.placesData
			.Select(d => d.placeDetails.result.geometry)
			.ToList();
		StopFlyby();
		StartCoroutine(FlybyCoroutine(delay));
	}

	/// <summary>
	/// Start a flyby of a specific location, with an optional delay.
	/// </summary>
	/// <param name="place">A Google Maps Place Details location.</param>
	/// <param name="delay">An optional delay, in seconds.</param>
	public void StartFlyby(PlaceDetails place, float delay = 0)
	{
		locations = new List<GoogleMaps.Geometry>();
		locations.Add(place.result.geometry);
		StopFlyby();
		StartCoroutine(FlybyCoroutine(delay));
	}

	/// <summary>
	/// Stop any ongoing flyby.
	/// </summary>
	public void StopFlyby()
	{
		StopAllCoroutines();
		IsDoingFlyby = false;
	}

	/// <summary>
	/// Coroutine that does the actual flyby logic.
	/// </summary>
	/// <param name="delay">delay between each location, in seconds.</param>
	/// <remarks>
	/// This logic is in a coroutine so that we don't have to put it in Update;
	/// Coroutines provide an easy way to keep track of time past
	/// </remarks>
	IEnumerator FlybyCoroutine(float delay)
	{
		// wait for delay, if there's any
		if (delay > 0) yield return new WaitForSecondsRealtime(delay);
		
		IsDoingFlyby = true;
		
		while (true)
		{
			foreach (var l in locations)
			{
				// calculate distance manually because we want to fly nearer
				Vector3 northeast = MapCamera.LatLongToUnity(l.viewport.northeast),
					southwest = MapCamera.LatLongToUnity(l.viewport.southwest);
				float diag = (northeast - southwest).magnitude; // viewport diagonal distance
				float dist = Mathf.Clamp(diag / 2f, 0.5f, camera.maxDistance);

				camera.SetCameraViewport(l, dist, true);
				camera.TargetElevation = 30f; // nice angle

				// wait for delay to fly to next location
				yield return new WaitForSecondsRealtime(flybyInterval);
			}
		}
	}
}
