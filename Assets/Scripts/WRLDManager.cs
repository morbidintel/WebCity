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
	}

	// Update is called once per frame
	void Update()
	{
		wrldMap.SetActive(mapCam.Distance < 10);
		transform.position = mapCam.TargetPosition.WithY(transform.position.y);
		//fixedCam.transform.localPosition = (mapCam.TargetPosition / transform.localScale.x).WithY(fixedCam.transform.position.y);
		//fixedCam.transform.localPosition = mapCam.TargetPosition.WithY(fixedCam.transform.position.y);

		var coord = MapCamera.UnityToLatLong(mapCam.TargetPosition);
		Api.Instance.SetOriginPoint(new Wrld.Space.LatLongAltitude(coord.lat, coord.lng, mapCam.Distance * 10.0));

		// TODO: different places in the world somehow requires different scaling of this manager object...
	}
}
