using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PhpDB
{
	[Serializable]
	public class Place
	{
		public string placeid, itineraryid, googleid, createddate;
		int labelid;
	}

	[Serializable]
	public class Itinerary
	{
		public string itineraryid, userid, name, createddate;
		public int rating, is_public, deleted;
		string colors;

		public Color[] GetColors()
		{
			string[] split = colors.Split(',');
			List<Color> converted = new List<Color>();
			foreach (string s in split)
			{
				Color color = Color.white;
				if (ColorUtility.TryParseHtmlString("#" + s, out color))
					converted.Add(color);
			}
			return converted.ToArray();
		}
	}

	[Serializable]
	public class LoginResult
	{
		public static readonly string URL = "http://webcity.online/live/db/login.php?username={0}&pwhash={1}";
		public string userid, username, fbid, email, desc, isfbonly, createddate, forgetpwtime;
		public string error;
	}

	[Serializable]
	public class GetPlacesResult
	{
		public static readonly string URL = "http://webcity.online/live/db/getplaces.php?itineraryid={0}";
		public Place[] places;
		public string error;
	}

	[Serializable]
	public class GetItinerariesResult
	{
		public static readonly string URL = "http://webcity.online/live/db/getitineraries.php?userid={0}";
	}
}
