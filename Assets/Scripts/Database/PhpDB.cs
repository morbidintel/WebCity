using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Extensions;

namespace PhpDB
{
	[Serializable]
	public class Place
	{
		public string placeid, itineraryid, googleid, arrivaltime, createddate;
		public int labelid = -1, itineraryindex = -1;

		[System.Runtime.Serialization.IgnoreDataMember]
		DateTime? arrival = null;

		public static string timeDBFormat = "yyyy-MM-dd HH:mm:ss";
		public static string timeDisplayFormat = "dd MMM HH:mm";

		/// <summary>
		/// Retrieve the parsed arrival time
		/// </summary>
		/// <returns>the arrival time in DateTime, or DateTime.MinValue if unable to parse or arrivaltime is empty</returns>
		public DateTime ArrivalDateTime()
		{
			if (arrival == null)
			{
				DateTime temp;
				if (!string.IsNullOrEmpty(arrivaltime) &&
					DateTime.TryParseExact(arrivaltime, timeDBFormat, null, System.Globalization.DateTimeStyles.None, out temp))
					arrival = temp;
				else
					return DateTime.MinValue;
			}

			return arrival.Value;
		}

		public void SetArrivalTime(DateTime dateTime)
		{
			arrivaltime = dateTime.ToString(timeDBFormat);
		}
	}

	[Serializable]
	public class Itinerary
	{
		public string itineraryid, userid, name, createddate;
		public int rating, is_public, deleted;
		public string colors;

		public string[] GetLabels()
		{
			return colors.Split(',');
		}

		public void SetLabel(int index, string name)
		{
			string[] labels = GetLabels();
			Debug.Assert(index > 0 && index < labels.Length);
			labels[index] = name;
			colors = string.Join(",", labels);
		}

		public void SetLabels(string[] labels)
		{
			Debug.Assert(labels != null &&
				labels.Length == 10);
			colors = string.Join(",", labels);
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
	public class EditPlaceResult
	{
		public new static string URL =
			"http://webcity.online/live/db/editplace.php";
		public Place place;
		public string error;

		public static string BuildURL(Place place)
		{
			string query = URL + "?placeid={0}&googleid={1}&labelid={2}&itineraryindex={3}&arrivaltime={4}";
			query = string.Format(query,
				WWW.EscapeURL(place.placeid),
				WWW.EscapeURL(place.googleid),
				place.labelid,
				place.itineraryindex,
				WWW.EscapeURL(place.arrivaltime));
			return query;
		}
	}

	[Serializable]
	public class RemovePlaceResult : GetPlacesResult
	{
		public new static string URL =
			"http://webcity.online/live/db/removeplace.php";

		public static string BuildURL(Place place)
		{
			string query = URL + "?itineraryid={0}&placeid={1}";
			query = string.Format(query,
				WWW.EscapeURL(place.itineraryid),
				WWW.EscapeURL(place.placeid));
			return query;
		}
	}

	[Serializable]
	public class EditItineraryResult : GetItinerariesResult
	{
		public new static string URL =
			"http://webcity.online/live/db/edititinerary.php";

		public static string BuildURL(Itinerary itinerary)
		{
			string query = URL + "?itineraryid={0}&name={1}&rating={2}&is_public={3}&deleted={4}&colors={5}";
			query = string.Format(query,
				WWW.EscapeURL(itinerary.itineraryid),
				WWW.EscapeURL(itinerary.name),
				WWW.EscapeURL(itinerary.rating.ToString()),
				WWW.EscapeURL(itinerary.is_public.ToString()),
				WWW.EscapeURL(itinerary.deleted.ToString()),
				WWW.EscapeURL(itinerary.colors));
			return query;
		}
	}

	[Serializable]
	public class RemoveItineraryResult : GetItinerariesResult
	{
		public new static string URL =
			"http://webcity.online/live/db/removeitinerary.php";

		public static string BuildURL(Itinerary itinerary)
		{
			string query = URL + "?itineraryid={0}";
			query = string.Format(query,
				WWW.EscapeURL(itinerary.itineraryid));
			return query;
		}
	}
}
