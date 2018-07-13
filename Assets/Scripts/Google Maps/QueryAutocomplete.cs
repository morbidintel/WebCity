using System;

namespace GoogleMaps
{
	[Serializable]
	public class QueryAutocomplete
	{
		public static readonly string URL = "https://maps.googleapis.com/maps/api/place/queryautocomplete/json?key=AIzaSyBAm0l1jdDejOC9Smk9WviPNvjeAb2XBbI&input={0}";

		[Serializable]
		public class Substring
		{
			public int length, offset;
		}

		[Serializable]
		public class Formatting
		{
			public string main_text, secondary_text;
			public Substring[] main_text_matched_substrings;
		}

		[Serializable]
		public class Term
		{
			public int offset;
			public string value;
		}

		[Serializable]
		public class Prediction
		{
			public string description, id, place_id, reference;
			public string[] types;
			public Substring[] matched_substrings;
			public Formatting structured_formatting;
			public Term[] terms;
		}

		public Prediction[] predictions;
		public string status, error_message;
	}
}
