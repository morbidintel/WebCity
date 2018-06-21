using System;

namespace Google_Maps
{
	[Serializable]
	public class PlaceDetails
	{
		[Serializable]
		public class AddressComponent
		{
			public string long_name, short_name;
			public string[] types;
		}

		[Serializable]
		public class Coords
		{
			public float lat, lng;
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
		public string status;
	}
}
