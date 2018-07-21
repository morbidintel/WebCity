using System;
using UnityEngine;

namespace GoogleMaps
{
	[Serializable]
	public class Coords
	{
		public double lat, lng;
		public Coords() { }
		public Coords(double lat, double lng)
		{
			this.lat = lat;
			this.lng = lng;
		}
		public override string ToString()
		{
			return string.Format("{0},{1}", lat, lng);
		}
	}

	[Serializable]
	public class AddressComponent
	{
		public string long_name, short_name;
		public string[] types;
	}

	[Serializable]
	public class Geometry
	{
		[Serializable]
		public class Viewport
		{
			public Coords northeast, southwest;
		}
		public Coords location;
		public Viewport viewport;
	}

	[Serializable]
	public class OpeningHours
	{
		[Serializable]
		public class Period
		{
			[Serializable]
			public class DayTime
			{
				public int day;
				public string time;
			}

			public DayTime close, open;
		}

		public bool open_now;
		public Period periods;
		public string[] weekday_text;
	}

	[Serializable]
	public class Photo
	{
		public int height, width;
		public string[] html_attributions;
		public string photo_reference;
	}

	[Serializable]
	public class Review
	{
		public string author_name,
				author_url,
				language,
				profile_photo_url,
				relative_time_description,
				text;
		public float rating;
		public int time;
	}

	/*
	 * Documentation
	 * https://developers.google.com/places/web-service/details#PlaceDetailsResults
	 * 
	 * Basic
	 * The Basic category includes the following fields:
	 * address_component, adr_address, alt_id, formatted_address, geometry, icon, id, name, permanently_closed, photo, place_id, scope, type, url, utc_offset, vicinity
	 * 
	 * Contact (billed higher)
	 * The Contact category includes the following fields:
	 * formatted_phone_number, international_phone_number, opening_hours, website
	 * 
	 * Atmosphere (billed higher)
	 * The Atmosphere category includes the following fields:
	 * price_level, rating, review
	 */
	[Serializable]
	public class PlaceDetails
	{
		public static readonly string URL = "https://maps.googleapis.com/maps/api/place/details/json?key=AIzaSyBAm0l1jdDejOC9Smk9WviPNvjeAb2XBbI";

		[Serializable]
		public class Result
		{
			public AddressComponent[] address_components;
			public Geometry geometry;
			public OpeningHours opening_hours;
			public Photo[] photos;
			public bool permanently_closed;
			public float rating;
			public Review[] reviews;
			public string[] types;
			public int utc_offset, price_level;
			public string adr_address,
				formatted_address,
				formatted_phone_number,
				icon,
				id,
				international_phone_number,
				name,
				place_id,
				reference,
				scope,
				url,
				vicinity,
				website;
		}

		public Result result;
		public string status, error_message;

		[Flags]
		public enum Fields
		{
			NONE = -1,
			// Basic
			address_component = 1 << 0,
			adr_address = 1 << 1,
			alt_id = 1 << 2,
			formatted_address = 1 << 3,
			geometry = 1 << 4,
			icon = 1 << 5,
			id = 1 << 6,
			name = 1 << 7,
			permanently_closed = 1 << 8,
			photo = 1 << 9,
			place_id = 1 << 10,
			scope = 1 << 11,
			type = 1 << 12,
			url = 1 << 13,
			utc_offset = 1 << 14,
			vicinity = 1 << 15,
			// Contact
			formatted_phone_number = 1 << 16,
			international_phone_number = 1 << 17,
			opening_hours = 1 << 18,
			website = 1 << 19,
			// Atmosphere
			price_level = 1 << 20,
			rating = 1 << 21,
			review = 1 << 22,
			ALL = 1 << 23
		}

		public static string BuildURL(string placeid, Fields fields = Fields.geometry | Fields.place_id)
		{
			string query = "&placeid={0}&fields={1}";
			string fieldsString = "";
			foreach (var e in Enum.GetValues(typeof(Fields)))
				if (fields.HasFlag((Enum)e) && e.ToString() != "ALL") fieldsString += e.ToString() + ',';

			return URL + string.Format(query, WWW.EscapeURL(placeid), WWW.EscapeURL(fieldsString.TrimEnd(',')));
		}
	}
}
