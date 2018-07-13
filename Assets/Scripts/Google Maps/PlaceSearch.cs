using System;
using UnityEngine;

namespace GoogleMaps
{
	// https://developers.google.com/places/web-service/search
	[Serializable]
	public class NearbySearch
	{
		public static readonly string URL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?" +
			"key=AIzaSyBAm0l1jdDejOC9Smk9WviPNvjeAb2XBbI" +
			"&location={0}" +
			"&radius={1}" +
			"&keyword={2}";

		[Serializable]
		public class Result
		{
			public string icon, id, name, place_id, scope, reference, vicnity;
			public string[] types;
			public Geometry geometry;
			public OpeningHours opening_hours;
			public Photo[] photos;
		}

		public Result[] results;
		public string status, error_message;

		public static string BuildURL(Coords coords, float radius, string keyword)
		{
			return string.Format(URL,
				coords.lat.ToString() + "," + coords.lng.ToString(),
				radius,
				WWW.EscapeURL(keyword));
		}
	}
}
