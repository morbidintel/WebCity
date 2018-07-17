using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Extensions;
using Wrld;

public class WRLDManager : MonoBehaviour
{
	public MapCamera mapCam;
	public Camera fixedCam;
	public GameObject wrldMap;

	void Start()
	{
		wrldMap.transform.Find("Root/Terrain").gameObject.SetActive(false);
		wrldMap.transform.Find("Root/Roads").gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{ 
		var coord = MapCamera.UnityToLatLong(mapCam.TargetPosition);
		var latlngalt = new Wrld.Space.LatLongAltitude(coord.lat, coord.lng, 0);
		var latlng = new Wrld.Space.LatLong(coord.lat, coord.lng);

		wrldMap.SetActive(mapCam.TargetDistance < 5);
		transform.position = mapCam.TargetPosition.WithY(transform.position.y);
		if (mapCam.TargetPosition.magnitude > 0)
		{
			float dist = (float)Wrld.Space.LatLong.EstimateGreatCircleDistance(latlng, new Wrld.Space.LatLong(0, 0)) / 1000f;
			transform.localScale = Vector3.one / (dist / (2 * Mathf.PI));
		}
		else
			transform.localScale = Vector3.one;
		fixedCam.transform.position = fixedCam.transform.position.WithY(mapCam.TargetDistance);

		Api.Instance.SetOriginPoint(latlngalt);

		// TODO: different places in the world somehow requires different scaling of this manager object...

		if (Input.GetKeyDown(KeyCode.BackQuote))
			Debug.Log(Api.Instance.EnvironmentFlatteningApi.GetCurrentScale());
	}
}
