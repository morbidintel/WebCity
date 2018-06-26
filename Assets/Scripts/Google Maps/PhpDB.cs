using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PhpDB
{
	[Serializable]
	public class LoginUser
	{
		public static readonly string URL = "http://webcity.online/live/db/login.php?username={0}&pwhash={1}";
		public string userid, username, fbid, email, desc, isfbonly, createddate, forgetpwtime;
		public string error;
	}

	[Serializable]
	public class GetPlaces
	{
		public static readonly string URL = "http://webcity.online/live/db/getplaces.php?itineraryid={0}";

		[Serializable]
		public class Place
		{
			public string placeid, itineraryid, googleid, createddate;
		}

		public Place[] places;
		public string error;
	}
}
