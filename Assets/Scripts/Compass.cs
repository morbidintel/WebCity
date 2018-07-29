﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Gamelogic.Extensions;

public class Compass : MonoBehaviour
{
	[SerializeField]
	Transform compassTransform = null;
	[SerializeField]
	Image compassImage = null;

	MapCamera camera;

	// Use this for initialization
	void Start()
	{
		camera = MapCamera.Instance;
	}

	// Update is called once per frame
	void Update()
	{
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

		compassTransform.rotation = Quaternion.Euler(compassTransform.transform.rotation.eulerAngles
			.SetX(90f - camera.RealElevation)
			.SetZ(camera.RealAzimuth));
	}

	public void OnClickCompass()
	{
		// move the camera a bit to stop the camera spinning in Sidebar:Update()
		camera.SetFocusTarget(camera.TargetPosition + new Vector3(camera.TargetDistance * .01f, 0, 0));
		camera.TargetAzimuth = camera.RealAzimuth < 180f ? 0 : 360;
		camera.TargetElevation = 89;
	}
}
