using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMaps
{
	[Serializable]
	public class DistanceMatrix
	{
		public static string URL = "https://maps.googleapis.com/maps/api/distancematrix/json?key=AIzaSyBAm0l1jdDejOC9Smk9WviPNvjeAb2XBbI";

		[Serializable]
		public class ValueText
		{
			public float value;
			public string text;
		}

		[Serializable]
		public class Duration : ValueText { }
		[Serializable]
		public class Distance : ValueText { }
		[Serializable]
		public class DurationInTraffic : ValueText { }
		[Serializable]
		public class Fare : ValueText
		{
			public string currency;
		}

		[Serializable]
		public class Element
		{
			public string status;
			public Duration duration;
			public Distance distance;
			public DurationInTraffic duration_in_traffic;
			public Fare fare;
		}

		[Serializable]
		public class Row
		{
			public Element[] elements;
		}

		// Each row corresponds to an origin
		// Each element within that row corresponds to a pairing of the origin with a destination value.
		public Row[] rows;
		public string[] origin_addresses, destination_addresses;
		public string status, error_message;

		public static string BuildURL(string[] originsPlaceIDs, string[] destinationsPlaceIDs)
		{
			string query = "&origins={0}&destinations={1}", origins = "", destinations = "";
			foreach (string origin in originsPlaceIDs)
				origins += "place_id:" + origin + "|";
			origins.TrimEnd('|');
			foreach (string destination in destinationsPlaceIDs)
				destinations += "place_id:" + destination + "|";
			destinations.TrimEnd('|');
			return URL + string.Format(query, origins, destinations);
		}

		public string GetDurationToNextPlace(string place_id)
		{
			try // lazy checks
			{
				int index = Array.IndexOf(origin_addresses, place_id);
				return rows[index].elements[index + 1].duration.text;
			}
			catch
			{
				return "";
			}
		}
	}
}
