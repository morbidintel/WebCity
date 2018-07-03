using System;

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

	/*
	 * Documentation
	 * https://developers.google.com/places/web-service/details#PlaceDetailsResults
	 * 
	 * Basic
		The Basic category includes the following fields:
		address_component, adr_address, alt_id, formatted_address, geometry, icon, id, name, permanently_closed, photo, place_id, scope, type, url, utc_offset, vicinity

		Contact
		The Contact category includes the following fields:
		formatted_phone_number, international_phone_number, opening_hours, website

		Atmosphere
		The Atmosphere category includes the following fields:
		price_level, rating, review
	 */
	[Serializable]
	public class PlaceDetails
	{
		public static readonly string URL = "https://maps.googleapis.com/maps/api/place/details/json?key=AIzaSyBAm0l1jdDejOC9Smk9WviPNvjeAb2XBbI&placeid={0}&fields={1}";

		[Serializable]
		public class AddressComponent
		{
			public string long_name, short_name;
			public string[] types;
		}

		[Serializable]
		public class Viewport
		{
			public Coords northeast, southwest;
		}

		[Serializable]
		public class Geometry
		{
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
			public int utc_offset;
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
	}
}
