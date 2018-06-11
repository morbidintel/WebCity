using System.Text;
using UnityEngine;


public class GoogleMapsTexture : OnlineTexture
{
    //HCH: now technically we're supposed to register our own API key with google maps 
    //and request the image url from them. 
    //but requesting the images directly seems to be working fine, so I guess we can use it for our demos for now.

    //obviously the url format is not documented, but managed to figure out some stuff
    public static string hardcodedUrl = "https://mt2.google.com/vt/lyrs=y&x={x}&y={y}&z={z}";
    // public static string hardcodedUrl = "https://mt2.google.com/vt/x={x}&y={y}&z={z}"; //no satellite

    //so for 'free' we can use openstreetmap but i'm not sure if 'free' means we can use in a commercial application
    //anyway here is their tile usage policy https://operations.osmfoundation.org/policies/tiles/
    //public static string hardcodedUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

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

    /// <summary>
    /// Converts a QuadKey into tile XY coordinates.
    /// </summary>
    /// <param name="quadKey">QuadKey of the tile.</param>
    /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
    /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
    /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
    public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
    {
        tileX = tileY = 0;
        levelOfDetail = quadKey.Length;
        for (int i = levelOfDetail; i > 0; i--)
        {
            int mask = 1 << (i - 1);
            switch (quadKey[levelOfDetail - i])
            {
                case '0':
                    break;

                case '1':
                    tileX |= mask;
                    break;

                case '2':
                    tileY |= mask;
                    break;

                case '3':
                    tileX |= mask;
                    tileY |= mask;
                    break;

                default:
                    throw new System.ArgumentException("Invalid QuadKey digit sequence.");
            }
        }
    }



    public static bool ValidateServerURL(string serverURL, out string errorMessage)
	{
        errorMessage = "";
		return true;
	}


	protected override string GenerateRequestURL( string nodeID )
	{
		// Children node numbering differs between QuadtreeLODNoDe and Bing maps, so we
		// correct it here.
		nodeID = nodeID.Substring(1).Replace('1','9').Replace('2','1').Replace('9','2');
        string quadkey = initialSector + nodeID;

        string url = CurrentFixedUrl ();
        int tilex, tiley, zoom;
        QuadKeyToTileXY(quadkey, out tilex, out tiley, out zoom);
        url = url.Replace("{x}", tilex.ToString());
        url = url.Replace("{y}", tiley.ToString());
        url = url.Replace("{z}", zoom.ToString());
		return url;
	}


	public string CurrentFixedUrl ()
	{
		return this.serverUrl;
	}


	protected override void InnerCopyTo(OnlineTexture copy)
	{
		GoogleMapsTexture target = (GoogleMapsTexture)copy;
		target.serverUrl = serverUrl;
		target.initialSector = initialSector;
		target.latitude = latitude;
		target.longitude = longitude;
		target.initialZoom = initialZoom;
	}
}
