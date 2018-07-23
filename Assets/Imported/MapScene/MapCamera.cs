using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;
using System;

//hch: this orbital camera differs from a usual orbit camera in 2 ways
//1. panning amount is distance dependant, the closer the zoom, the less pan is applied
//2. zoom is non linear, so scrolling when near zooms less than when far.

//This is a 2-in-1 class, allows for zooming and rotation of the camera 
//around the 3d model while always pointing the camera to it
public class MapCamera : MonoBehaviour
{
	//getting instance of orbital fetching first active instance if it exists
	public static MapCamera Instance
	{
		get
		{
			if (instance != null && !instance.gameObject.activeInHierarchy)
			{
				instance = null;
			}
			if (instance == null)
			{
				instance = (MapCamera)FindObjectOfType(typeof(MapCamera));
			}
			return instance;
		}
	}
	private static MapCamera instance;


	public Vector3 TargetPosition
	{
		get { return targetPosition; }
	}
	public Vector3 CurrentLookPosition
	{
		get { return realTargetPosition; }
	}

	[SerializeField]
	RectTransform clickableArea;

	public float minDistance = 16, maxDistance = 40;
	public float minAngleAtMinZoom = 20, minAngleAtMaxZoom = 20, maxAngle = 88;
	public bool enablePan = true;
	public float rotateSensitivity = 1.0f;
	public float zoomSensitivity = 10.0f;
	public float panSensitivity = 10.0f;
	public float rotateAnimationSpeed = 3.0f;
	public float zoomAnimationSpeed = 3.0f;
	public float panAnimationSpeed = 3.0f;

	Transform transformToTrack;
	protected Vector3 targetPosition; //current static target position
	Vector3 realTargetPosition; //will be lerped to targetpos
	float dx, dy; //stores the per frame delta mouse positions when clicked
	float totalMovement = 0;
	const float totalMovementThreshold = 18.0f;
	bool hasMoved = false;

	Vector3 anchorPosition;
	//allow people to anchor the camera to prevent panning further than distance
	public float anchorDistanceLimitAtMaxZoom = 0; //if non zero, limit distance
	public float anchorDistanceLimitAtMinZoom = 0; //if non zero, limit distance
	public bool limitDistanceForEachAxis = false;

	//allow people to adjust camera properties
	public float TargetDistance { get; set; }
	public float TargetAzimuth { get; set; }
	public float TargetElevation { get; set; }

	[ReadOnly]
	[SerializeField]
	float realDistance, realAzimuth, realElevation; //used to store the current orientation and distance of the camera
	public float RealElevation { get { return realElevation; } }
	public float RealAzimuth { get { return realAzimuth; } }

	//store mouse positions for this frame and last frame
	Vector3 lastMousePos;
	Vector3 currMousePos;

	float lerpTargetPositionTime = 0.6f;
	float lerpTargetPositionTimer;

	public bool IsMoving { get { return (dx != 0 || dy != 0); } }
	public bool HasMoved { get { return hasMoved; } }//applies to both pan and rotate
	public bool HasRotated { get; private set; }

	[SerializeField]
	Transform mapAreaPlane = null;

	// Use this for initialization
	void Start()
	{
		//Debug.Assert(clickableArea, "No clickable area!");
		lastMousePos = currMousePos = Vector3.zero;
		dx = 0; dy = 0;
		realTargetPosition = targetPosition;
		TargetElevation = Mathf.Repeat(gameObject.transform.eulerAngles.x, 360.0f);
		realElevation = TargetElevation;
		TargetAzimuth = Mathf.Repeat(gameObject.transform.eulerAngles.y, 360.0f);
		realAzimuth = TargetAzimuth;
		TargetDistance = (gameObject.transform.position - targetPosition).magnitude;
		realDistance = TargetDistance * 1.5f;
		HasRotated = false;
		anchorPosition = transform.position;
	}

	public void Reset()
	{
		transformToTrack = null;
		targetPosition = Vector3.zero;
		HasRotated = false;
		anchorPosition = targetPosition;
	}

	public void Reset(Vector3 tPos, Quaternion quat, float height)
	{
		targetPosition = tPos;
		TargetElevation = Mathf.Repeat(quat.eulerAngles.x, 360.0f);
		TargetAzimuth = Mathf.Repeat(quat.eulerAngles.y, 360.0f);
		if (realAzimuth - TargetAzimuth > 180.0f) TargetAzimuth += 360.0f;
		else if (TargetAzimuth - realAzimuth > 180.0f) TargetAzimuth -= 360.0f;
		this.TargetDistance = height;
		HasRotated = false;
	}

	bool PointInClickArea(Vector2 point)
	{
		if (clickableArea == null) //if click area unassigned, can click anywhere
		{
			return true;
		}
		RectTransform region = clickableArea;

		Vector3[] wc = new Vector3[4];
		region.GetWorldCorners(wc);
		var touchRegion = new Rect(wc[0].x, wc[0].y, wc[2].x - wc[0].x, wc[2].y - wc[0].y);


		if (touchRegion.Contains(Input.mousePosition))
		{
			return true;
		}
		return false;
	}

	public Transform GetTrackedTarget()
	{
		return transformToTrack;
	}
	public void SetFocusTarget(Transform target, bool changeAnchor = false, bool instant = false)
	{
		SetFocusTarget(target.position, changeAnchor, instant);
		transformToTrack = target;
		if (changeAnchor)
		{
			anchorPosition = target.position;
		}

	}
	public void SetFocusTarget(Vector3 target, bool changeAnchor = false, bool instant = false)
	{
		targetPosition = target;
		transformToTrack = null;
		if (changeAnchor)
		{
			anchorPosition = targetPosition;
		}
		if (instant)
		{
			realTargetPosition = targetPosition;
		}
	}

	public void SetAnchor(Vector3 target)
	{
		anchorPosition = target;
	}

	public float GetZoom()
	{
		return Mathf.InverseLerp(minDistance, maxDistance, TargetDistance);
	}

	public float GetExpoZoom()
	{
		float zoomF = 8.0f - Mathf.Log(TargetDistance, 2.0f);
		float zoomFmin = 8.0f - Mathf.Log(minDistance, 2.0f);
		float zoomFmax = 8.0f - Mathf.Log(maxDistance, 2.0f);

		//normalize
		float pct = (zoomF - zoomFmin)/(zoomFmax - zoomFmin);
		return pct;
	}

	public void SetExpoZoom(float percentage)
	{
		float zoomFmin = 8.0f - Mathf.Log(minDistance, 2.0f);
		float zoomFmax = 8.0f - Mathf.Log(maxDistance, 2.0f);

		float desiredZoomF = Mathf.Lerp(zoomFmin, zoomFmax, percentage);
		TargetDistance = Mathf.Pow(2.0f, 8.0f - desiredZoomF);
	}

	//sets distance to a percentage between min and max
	public void SetZoom(float percentage)
	{
		TargetDistance = Mathf.Lerp(minDistance, maxDistance, percentage);
	}

	float GetPinchZoomAmount()
	{
		if (Input.touchCount == 2)
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
			return deltaMagnitudeDiff;
		}
		return 0;
	}

	Vector2 GetTwoFingerPanAmount()
	{
		if (Input.touchCount == 2)
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			Vector2 touchAvgPrevPos = (touchOnePrevPos + touchZeroPrevPos) * 0.5f;
			Vector2 touchAvgPos = (touchZero.position + touchOne.position) * 0.5f;

			return touchAvgPos - touchAvgPrevPos;
		}
		return Vector2.zero;
	}

	public void UnclickCamera()
	{
		enablePan = true;
		transformToTrack = null;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.touchCount < 2)
		{
			currMousePos = Input.mousePosition;
		}
		HasRotated = false;

		//if mouse is pressed, allow camera movement      
		if (!Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
		{
			totalMovement = 0;
			hasMoved = false;
		}

		//need at least 2 frames of valid data before we have valid deltas
		if (lastMousePos != Vector3.zero && currMousePos != Vector3.zero)
		{
			dx = currMousePos.x - lastMousePos.x;
			dy = currMousePos.y - lastMousePos.y;
			totalMovement += Mathf.Abs(dx) + Mathf.Abs(dy);
			if (totalMovement > totalMovementThreshold)
			{
				hasMoved = true;
			}
		}
		else
		{
			dx = dy = 0;
		}


		//special support for touch input ON WINDOWS
#if !UNITY_ANDROID
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			// Get movement of the finger since last frame
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

			dx = touchDeltaPosition.x;
			dy = touchDeltaPosition.y;
			totalMovement += Mathf.Abs(dx) + Mathf.Abs(dy);
			if (totalMovement > totalMovementThreshold)
			{
				hasMoved = true;
			}
		}
#endif

		//get zoom, if applied 
		float distChanged = 0;
		//zoom levels, ideally twice as slow when twice as near;
		//we use 256 distance as a 1.0f baseline, so at 128 distance pan is 0.5f and at 512 distance pan is 2.0f
		float zoomF = 8.0f - Mathf.Log(TargetDistance, 2.0f);
		float scaleFactor = Mathf.Pow(0.5f, zoomF);
		if (Input.mouseScrollDelta != Vector2.zero && PointInClickArea(Input.mousePosition))
		{

			if (!MapRaycaster.IsPointerOverUIObject())
			{
				//the y component affects zoom
				distChanged -= Input.mouseScrollDelta.y * zoomSensitivity * scaleFactor;
			}
		}
		distChanged += GetPinchZoomAmount() * 0.5f * scaleFactor;
		totalMovement += Mathf.Abs(distChanged * 2);
		if (totalMovement > totalMovementThreshold)
		{
			hasMoved = true;
		}
		if (!((TargetDistance <= minDistance && distChanged < 0) || (TargetDistance >= maxDistance && distChanged > 0)))
		{
			TargetDistance += distChanged;
			realDistance += distChanged * 0.9f;
		}

		if (enablePan)
		{
			//get pan amount for mobile
			Vector2 pan = GetTwoFingerPanAmount();

			//get panning if applied
			if (Input.GetKey(KeyCode.Mouse2) || Input.GetKey(KeyCode.Mouse0) || pan != Vector2.zero)
			{
				if (!MapRaycaster.IsPointerOverUIObject())
				{
					Vector3 yplanevec = -transform.forward;
					Vector3 xplanevec = -transform.right;
					yplanevec.y = 0;
					yplanevec = yplanevec.normalized * panSensitivity;
					xplanevec.y = 0;
					xplanevec = xplanevec.normalized * panSensitivity;

					//pan is reduced at close zoom levels, ideally twice as slow when twice as near;
					//we use 256 distance as a 1.0f baseline, so at 128 distance pan is 0.5f and at 512 distance pan is 2.0f

					if (pan != Vector2.zero)
					{
						Vector3 panChange = yplanevec * pan.y + xplanevec * pan.x;
						targetPosition += panChange * scaleFactor;
					}
					else
					{
						Vector3 panChange = yplanevec * dy + xplanevec * dx;
						targetPosition += panChange * scaleFactor;
					}

				}
			}
		}

		float anchorDistanceLimit = Mathf.Lerp(anchorDistanceLimitAtMinZoom, anchorDistanceLimitAtMaxZoom, 1.0f - GetZoom());
		if (anchorDistanceLimit > 0)
		{
			if (limitDistanceForEachAxis)
			{
				Vector3 offset = targetPosition - new Vector3(anchorPosition.x, targetPosition.y, anchorPosition.z);
				if (Mathf.Abs(offset.x) > anchorDistanceLimit)
				{
					offset = new Vector3(Mathf.Sign(offset.x) * anchorDistanceLimit, 0, offset.z);
				}
				if (Mathf.Abs(offset.z) > anchorDistanceLimit)
				{
					offset = new Vector3(offset.x, 0, Mathf.Sign(offset.z) * anchorDistanceLimit);
				}
				targetPosition = (anchorPosition + offset).SetY(targetPosition.y);
			}
			else
			{
				if (Vector3.Distance(targetPosition, new Vector3(anchorPosition.x, targetPosition.y, anchorPosition.z)) > anchorDistanceLimit)
				{
					Vector3 offset = targetPosition - anchorPosition;
					offset = offset.SetY(0);
					offset.Normalize();
					offset *= anchorDistanceLimit;
					targetPosition = (anchorPosition + offset).SetY(targetPosition.y);
				}
			}
		}

		realTargetPosition.x = Mathf.Lerp(realTargetPosition.x, targetPosition.x, Mathf.Clamp(Time.deltaTime * panAnimationSpeed, 0.0f, 0.2f));
		realTargetPosition.y = Mathf.Lerp(realTargetPosition.y, targetPosition.y, Mathf.Clamp(Time.deltaTime * panAnimationSpeed, 0.0f, 0.2f));
		realTargetPosition.z = Mathf.Lerp(realTargetPosition.z, targetPosition.z, Mathf.Clamp(Time.deltaTime * panAnimationSpeed, 0.0f, 0.2f));

		float minAngleAtCurrentZoom = Mathf.Lerp(minAngleAtMinZoom, minAngleAtMaxZoom, GetZoom());

		//apply mouse movement to camera
		if (Input.GetKey(KeyCode.Mouse1) && PointInClickArea(Input.mousePosition))
		{
			if (!MapRaycaster.IsPointerOverUIObject())
			{
				float elevationChange = -dy * rotateSensitivity;
				if (!((TargetElevation <= minAngleAtCurrentZoom && elevationChange < 0) || (TargetElevation >= maxAngle && elevationChange > 0)))
				{
					TargetElevation += elevationChange;
					realElevation += elevationChange * 0.7f;
				}

				float azimuthChange = dx * rotateSensitivity;
				TargetAzimuth += azimuthChange;
				realAzimuth += azimuthChange * 0.7f;

				if (dx != 0 || dy != 0)
				{
					HasRotated = true;
				}
			}
		}

		//clamp elevation and zoom distance to sensible values

		TargetElevation = Mathf.Clamp(TargetElevation, minAngleAtCurrentZoom, maxAngle);
		if ((realAzimuth < 0.0f && TargetAzimuth < 0.0f) || (realAzimuth >= 360.0f && TargetAzimuth >= 360.0f))
		{
			TargetAzimuth = Mathf.Repeat(TargetAzimuth, 360.0f);
			realAzimuth = Mathf.Repeat(realAzimuth, 360.0f);
		}

		realElevation = Mathf.Lerp(realElevation, TargetElevation, Mathf.Clamp(Time.deltaTime * rotateAnimationSpeed, 0.0f, 0.15f));
		realAzimuth = Mathf.Lerp(realAzimuth, TargetAzimuth, Mathf.Clamp(Time.deltaTime * rotateAnimationSpeed, 0.0f, 0.15f));


		//update the camera with the new values
		gameObject.transform.rotation = Quaternion.Euler(realElevation, realAzimuth, 0);
		TargetDistance = Mathf.Clamp(TargetDistance, minDistance, maxDistance);
		realDistance = Mathf.Lerp(realDistance, TargetDistance, Mathf.Clamp(Time.deltaTime * zoomAnimationSpeed, 0.0f, 0.15f));

		float snapLimit = 0.3f;
		float snapLimitD = 0.01f;
		if (Mathf.Abs(realElevation - TargetElevation) < snapLimit) { realElevation = TargetElevation; }
		if (Mathf.Abs(realAzimuth - TargetAzimuth) < snapLimit) { realAzimuth = TargetAzimuth; }
		if (Mathf.Abs(realDistance - TargetDistance) < snapLimitD) { realDistance = TargetDistance; }

		Vector3 trackedPos;
		if (transformToTrack != null)
			trackedPos = transformToTrack.position;
		else
			trackedPos = realTargetPosition;


		gameObject.transform.position = trackedPos - gameObject.transform.forward * realDistance;

		if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2))
		{
			lastMousePos = currMousePos;
		}
		else
		{
			currMousePos = Vector3.zero;
			lastMousePos = Vector3.zero;
		}

		// move viewport rect with sidebar movement
		float sidebarx = Sidebar.Instance.RectTransform().anchoredPosition.x / Sidebar.Instance.TweenMaxX;
		Camera.main.rect = new Rect(Mathf.Lerp(0, 0.245f, sidebarx), 0, Mathf.Lerp(0.751f, 1, 1 - sidebarx), 1);
	}

	public static Vector3 LatLongToUnity(GoogleMaps.Coords coords)
	{
		return LatLongToUnity(coords.lat, coords.lng);
	}

	public static Vector3 LatLongToUnity(double latitude, double longitude)
	{
		double sinLatitude = Math.Sin(latitude * Math.PI / 180.0);

		double pixelX = (longitude + 180.0) / 360.0; //from 0.0 to 1.0
		double pixelY = 1.0 - ((0.5 - Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * Math.PI)) ); //from 0.0 to 1.0

		var bounds = Instance.mapAreaPlane.GetComponent<BoxCollider>().bounds;
		Vector3 minPos = bounds.min;
		Vector3 maxPos = bounds.max;
		float mapX = Mathf.Lerp(minPos.x, maxPos.x, (float)pixelX);
		float mapZ = Mathf.Lerp(minPos.z, maxPos.z, (float)pixelY);

		return new Vector3(mapX, 0, mapZ);
	}

	static double Clamp01(double value) { if (value < 0.0) return 0.0; else if (value > 1.0) return 1.0; else return value; }

	public static GoogleMaps.Coords UnityToLatLong(Vector3 pos)
	{
		var bounds = Instance.mapAreaPlane.GetComponent<BoxCollider>().bounds;
		Vector3 minPos = bounds.min;
		Vector3 maxPos = bounds.max;

		double pixelX = Clamp01((pos.x - minPos.x) / (maxPos.x - minPos.x));
		double pixelY = Clamp01((pos.z - minPos.x) / (maxPos.x - minPos.x));

		double lng = (pixelX * 360.0) - 180.0;
		double lat = Math.Exp((0.5 - (1.0 - pixelY)) * (4.0 * Math.PI));
		lat = Math.Asin((lat - 1.0) / (lat + 1.0)) / (Math.PI / 180.0);

		return new GoogleMaps.Coords(lat, lng);
	}

	public GoogleMaps.Coords GetCameraCoords()
	{
		return UnityToLatLong(targetPosition);
	}

	public void SetCameraViewport(GoogleMaps.Geometry geometry, float dist = -1)
	{
		var location = geometry.location;
		var viewport = geometry.viewport;
		if (dist == -1)
		{
			float diag = (LatLongToUnity(viewport.northeast) - LatLongToUnity(viewport.southwest)).magnitude;
			TargetDistance = Mathf.Clamp(diag, 0.5f, maxDistance);
		}
		else
		{
			TargetDistance = dist;
		}
		SetFocusTarget(LatLongToUnity(location.lat, location.lng));
		TargetAzimuth = 0;
	}

	public float GetRadius()
	{
		return TargetDistance * 2500f;
	}
}
