using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GoogleMaps
{
	[Serializable]
	public static class PlacePhotos
	{
		public static string URL = "https://maps.googleapis.com/maps/api/place/photo?key=AIzaSyBAm0l1jdDejOC9Smk9WviPNvjeAb2XBbI";

		public static string BuildURL(string photoreference, int maxWidth, int maxHeight)
		{
			Debug.Assert(maxWidth > 0 || maxHeight > 0); // must have either parameter

			string query = "&photoreference={0}";
			if (maxWidth > 0) query += "&maxwidth={1}";
			if (maxHeight > 0) query += "&maxheight={2}";
			return string.Format(URL + query, photoreference, maxWidth, maxHeight);
		}
	}
}
