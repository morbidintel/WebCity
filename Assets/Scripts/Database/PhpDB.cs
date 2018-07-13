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
		public string userid, username, fbid, email, desc;
		public int isfbonly = 0;
		public DateTime createddate, forgetpwtime;
		public string error;

		public LoginResult(string userid, string username)
		{
			this.userid = userid;
			this.username = username;
		}
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
		public Itinerary[] itineraries;
		public string error;
	}

	[Serializable]
	public class AddItineraryResult : GetItinerariesResult
	{
		public new static string URL =
			"http://webcity.online/live/db/additinerary.php?userid={0}&name={1}&colors={2}";
		public static string DefaultColors = "61BD4F%2CF2D600%2CFF9F1A%2CEB5A46%2CC377E0%2C0079BF%2C00C2E0%2C51E898%2CFF78CB%2C4D4D4D";
	}

	[Serializable]
	public class AddPlaceResult : GetPlacesResult
	{
		public new static string URL =
			"http://webcity.online/live/db/addplace.php?itineraryid={0}&googleid={1}";
	}
	
	[Serializable]
	public class RemovePlaceResult : GetPlacesResult
	{
		public new static string URL =
			"http://webcity.online/live/db/removeplace.php?itineraryid={0}&googleid={1}";
	}
}
