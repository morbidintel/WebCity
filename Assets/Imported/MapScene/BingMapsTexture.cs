using System.Text;
using UnityEngine;


public class BingMapsTexture : OnlineTexture
{
    //HCH: now technically we're supposed to register our own API key with bing maps 
    //and request the image url from them. 
    //but requesting the images directly seems to be working fine, so I guess we can use it for our demos for now.
	public static string hardcodedUrl = "https://ecn.t2.tiles.virtualearth.net/tiles/r{quadkey}.jpeg?g=4800&shading=hill";
    public string serverUrl = hardcodedUrl;
	public string initialSector = "0";
	public float latitude = 28.127222f;
	public float longitude = -15.431389f;
	public int initialZoom = 0;

    protected override void OnDestroy()
    {
        base.OnDestroy();

    }

    public void ComputeInitialSector()
	{
		float sinLatitude = Mathf.Sin (latitude * Mathf.PI / 180.0f);

		int pixelX = (int)( ((longitude + 180) / 360) * 256 * Mathf.Pow (2, initialZoom + 1) );
		int pixelY = (int)( (0.5f - Mathf.Log ((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Mathf.PI)) * 256 * Mathf.Pow (2, initialZoom + 1) );

		int tileX = Mathf.FloorToInt (pixelX / 256);
		int tileY = Mathf.FloorToInt (pixelY / 256);

		initialSector = TileXYToQuadKey (tileX, tileY, initialZoom + 1);
	}


	// Function taken from "Bing Maps Tile System": https://msdn.microsoft.com/en-us/library/bb259689.aspx
	public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
	{
		StringBuilder quadKey = new StringBuilder();
		for (int i = levelOfDetail; i > 0; i--)
		{
			char digit = '0';
			int mask = 1 << (i - 1);
			if ((tileX & mask) != 0)
			{
				digit++;
			}
			if ((tileY & mask) != 0)
			{
				digit++;
				digit++;
			}
			quadKey.Append(digit);
		}
		return quadKey.ToString();
	}


	public static bool ValidateServerURL(string serverURL, out string errorMessage)
	{
		errorMessage = "";
		if( serverURL.IndexOf("{quadkey}" ) < 0 ){
			errorMessage = "BingMaps inspector - missing {quadkey} in server URL";
			return false;
		}
		
		return true;
	}


	protected override string GenerateRequestURL( string nodeID )
	{
		// Children node numbering differs between QuadtreeLODNoDe and Bing maps, so we
		// correct it here.
		nodeID = nodeID.Substring(1).Replace('1','9').Replace('2','1').Replace('9','2');

		string url = CurrentFixedUrl ();
		url = url.Replace ("{quadkey}", initialSector + nodeID);
		return url;
	}


	public string CurrentFixedUrl ()
	{
		return this.serverUrl;
	}


	protected override void InnerCopyTo(OnlineTexture copy)
	{
		BingMapsTexture target = (BingMapsTexture)copy;
		target.serverUrl = serverUrl;
		target.initialSector = initialSector;
		target.latitude = latitude;
		target.longitude = longitude;
		target.initialZoom = initialZoom;
	}
}
