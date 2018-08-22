using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controls the on screen compass and events triggered by clicks on that compass.
/// </summary>
public class Compass : MonoBehaviour
{
	/// <summary>
	/// The transform of the on screen compass; used to contorl the rotations of the compass.
	/// </summary>
	[SerializeField] Transform compassTransform = null;
	/// <summary>
	/// UI Image of the on screen compaas; used to control the DOTween fading animation.
	/// </summary>
	[SerializeField] Image compassImage = null;

	// reference of the MapCamera
	new MapCamera camera;
	
	void Start()
	{
		camera = MapCamera.Instance;
	}
	
	void Update()
	{
		// Fade out the compass when the camera orientation is at 'origin'
		// Note: MapCamera.TargetElevation is 89 degrees max
		if ((Mathf.Approximately(camera.TargetAzimuth, 0f) || Mathf.Approximately(camera.TargetAzimuth, 360f)) &&
			Mathf.Approximately(camera.TargetElevation, 89f))
		{
			if (compassImage.color.a == 1)
				DOTween.PlayBackwards(compassImage.gameObject);
		}
		else if (compassImage.color.a == 0)
		{
			DOTween.Restart(compassImage.gameObject);
		}

		// Orientate the compass to match MapCamera orientation
		compassTransform.rotation = Quaternion.Euler(compassTransform.transform.rotation.eulerAngles
			.SetX(90f - camera.RealElevation)
			.SetZ(camera.RealAzimuth));
	}

	/// <summary>
	/// Resets the MapCamera.
	/// </summary>
	/// <remarks>
	/// Called by Compass -> Button.OnClick event.
	/// </remarks>
	public void OnClickCompass()
	{
		// move the camera a bit to stop the camera spinning in Flyby.Update()
		camera.SetFocusTarget(camera.TargetPosition + new Vector3(camera.TargetDistance * .01f, 0, 0));
		camera.TargetAzimuth = camera.RealAzimuth < 180f ? 0 : 360;
		camera.TargetElevation = 89;
	}
}
