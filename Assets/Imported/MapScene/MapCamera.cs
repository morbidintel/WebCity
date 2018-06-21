using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	float distance, azimuth, elevation;
	public float Distance { get { return distance; } set { distance = value; } }
	public float Azimuth { get { return azimuth; } set { azimuth = value; } }
	public float Elevation { get { return elevation; } set { elevation = value; } }

	[ReadOnly]
	[SerializeField]
	float realDistance, realAzimuth, realElevation; //used to store the current orientation and distance of the camera

	//store mouse positions for this frame and last frame
	Vector3 lastMousePos;
	Vector3 currMousePos;

	float lerpTargetPositionTime = 0.6f;
	float lerpTargetPositionTimer;

	public bool IsMoving { get { return (dx != 0 || dy != 0); } }
	public bool HasMoved { get { return hasMoved; } }//applies to both pan and rotate
	public bool HasRotated { get; private set; }

	// Use this for initialization
	void Start()
	{
		//Debug.Assert(clickableArea, "No clickable area!");
		lastMousePos = currMousePos = Vector3.zero;
		dx = 0; dy = 0;
		realTargetPosition = targetPosition;
		elevation = Mathf.Repeat(gameObject.transform.eulerAngles.x, 360.0f);
		realElevation = elevation;
		azimuth = Mathf.Repeat(gameObject.transform.eulerAngles.y, 360.0f);
		realAzimuth = azimuth;
		distance = (gameObject.transform.position - targetPosition).magnitude;
		realDistance = distance * 1.5f;
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

	public void Reset(Transform trans, Vector3 tPos)
	{
		targetPosition = tPos;
		elevation = Mathf.Repeat(trans.eulerAngles.x, 360.0f);
		azimuth = Mathf.Repeat(trans.eulerAngles.y, 360.0f);
		if (realAzimuth - azimuth > 180.0f) azimuth += 360.0f;
		else if (azimuth - realAzimuth > 180.0f) azimuth -= 360.0f;
		distance = (trans.transform.position - targetPosition).magnitude;
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
		return Mathf.InverseLerp(minDistance, maxDistance, distance);
	}

	public float GetExpoZoom()
	{
		float zoomF = 8.0f - Mathf.Log(distance, 2.0f);
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
		distance = Mathf.Pow(2.0f, 8.0f - desiredZoomF);
	}

	//sets distance to a percentage between min and max
	public void SetZoom(float percentage)
	{
		distance = Mathf.Lerp(minDistance, maxDistance, percentage);
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
		float zoomF = 8.0f - Mathf.Log(distance, 2.0f);
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
		if (!((distance <= minDistance && distChanged < 0) || (distance >= maxDistance && distChanged > 0)))
		{
			distance += distChanged;
			realDistance += distChanged * 0.9f;
		}

		if (enablePan)
		{
			//get pan amount for mobile
			Vector2 pan = GetTwoFingerPanAmount();

			//get panning if applied
			if (Input.GetKey(KeyCode.Mouse2) || pan != Vector2.zero)
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
		if ((Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse0)) && PointInClickArea(Input.mousePosition))
		{
			if (!MapRaycaster.IsPointerOverUIObject())
			{
				float elevationChange = -dy * rotateSensitivity;
				if (!((elevation <= minAngleAtCurrentZoom && elevationChange < 0) || (elevation >= maxAngle && elevationChange > 0)))
				{
					elevation += elevationChange;
					realElevation += elevationChange * 0.7f;
				}

				float azimuthChange = dx * rotateSensitivity;
				azimuth += azimuthChange;
				realAzimuth += azimuthChange * 0.7f;

				if (dx != 0 || dy != 0)
				{
					HasRotated = true;
				}
			}
		}

		//clamp elevation and zoom distance to sensible values

		elevation = Mathf.Clamp(elevation, minAngleAtCurrentZoom, maxAngle);
		if ((realAzimuth < 0.0f && azimuth < 0.0f) || (realAzimuth > 360.0f && azimuth > 360.0f))
		{
			azimuth = Mathf.Repeat(azimuth, 360.0f);
			realAzimuth = Mathf.Repeat(realAzimuth, 360.0f);
		}

		realElevation = Mathf.Lerp(realElevation, elevation, Mathf.Clamp(Time.deltaTime * rotateAnimationSpeed, 0.0f, 0.15f));
		realAzimuth = Mathf.Lerp(realAzimuth, azimuth, Mathf.Clamp(Time.deltaTime * rotateAnimationSpeed, 0.0f, 0.15f));


		//update the camera with the new values
		gameObject.transform.rotation = Quaternion.Euler(realElevation, realAzimuth, 0);
		distance = Mathf.Clamp(distance, minDistance, maxDistance);
		realDistance = Mathf.Lerp(realDistance, distance, Mathf.Clamp(Time.deltaTime * zoomAnimationSpeed, 0.0f, 0.15f));

		float snapLimit = 0.3f;
		float snapLimitD = 0.01f;
		if (Mathf.Abs(realElevation - elevation) < snapLimit) { realElevation = elevation; }
		if (Mathf.Abs(realAzimuth - azimuth) < snapLimit) { realAzimuth = azimuth; }
		if (Mathf.Abs(realDistance - distance) < snapLimitD) { realDistance = distance; }

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
	}
}
