using System;
using UnityEngine;

namespace PhpDB
{
	/// <summary>
	/// JSON serializable class for a row in the <code>places</code> table.
	/// </summary>
	[Serializable]
	public class Place
	{
		public string placeid, itineraryid, googleid, arrivaltime, createddate;
		public int labelid = -1, itineraryindex = -1;

		[System.Runtime.Serialization.IgnoreDataMember]
		private DateTime? arrival = null; // don't serialize this

		/// <summary>
		/// DateTime format used to parse strings from database.
		/// </summary>
		public static string timeDBFormat = "yyyy-MM-dd HH:mm:ss";
		/// <summary>
		/// DateTime format used for displaying within the application.
		/// </summary>
		public static string timeDisplayFormat = "dd MMM HH:mm";

		/// <summary>
		/// Retrieve the parsed arrival time.
		/// <para></para>Note: Does not retrieve the value from database.
		/// </summary>
		/// <returns>The arrival time in DateTime, or <code>DateTime.MinValue</code>
		/// if unable to parse or <code>arrivaltime</code> is empty.</returns>
		public DateTime ArrivalDateTime()
		{
			if (arrival == null)
			{
				DateTime temp;
				if (!string.IsNullOrEmpty(arrivaltime) && 
					DateTime.TryParseExact(arrivaltime, timeDBFormat, null, 0, out temp))
					arrival = temp;
				else
					return DateTime.MinValue;
			}

			return arrival.Value;
		}

		/// <summary>
		/// Sets the arrivaltime string.
		/// <para></para>Note: Does not sends the value to database.
		/// </summary>
		/// <param name="dateTime">The arrival time as a DateTime</param>
		public void SetArrivalTime(DateTime dateTime)
		{
			arrivaltime = dateTime.ToString(timeDBFormat);
		}
	}

	/// <summary>
	/// JSON serializable class for a row in the <code>itineraries</code> table.
	/// </summary>
	[Serializable]
	public class Itinerary
	{
		public string itineraryid, userid, name, createddate;
		public int rating, is_public, deleted;
		public string colors;

		/// <summary>
		/// Get the names for every label for this itinerary.
		/// </summary>
		/// <returns>An array of 10 strings containing the name of each label.</returns>
		public string[] GetLabels()
		{
			return colors.Split(',');
		}

		/// <summary>
		/// Set the name for a specific label.
		/// <para></para>Note: Does not sends the value to database.
		/// </summary>
		/// <param name="index">Index, from 0 to 9, of the specific label to be set.</param>
		/// <param name="name">The name to be set for the label.</param>
		public void SetLabel(int index, string name)
		{
			string[] labels = GetLabels();
			if (index > 0 && index < labels.Length) return;
			labels[index] = name;
			colors = string.Join(",", labels);
		}

		/// <summary>
		/// Sets the names for all the labels.
		/// <para></para>Note: Does not sends the values to database.
		/// </summary>
		/// <param name="labels">Array of 10 strings representing the name of each label.</param>
		public void SetLabels(string[] labels)
		{
			if (labels == null || labels.Length != 10) return;
			colors = string.Join(",", labels);
		}
	}

	/// <summary>
	/// JSON serializable class for a row in the <code>users</code> table.
	/// </summary>
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
		public static string DefaultColors = "%2C%2C%2C%2C%2C%2C%2C%2C%2C"; // nine commas
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
