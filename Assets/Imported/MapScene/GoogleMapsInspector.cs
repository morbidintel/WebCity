﻿#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GoogleMapsTexture))]
[CanEditMultipleObjects]
public class GoogleMapsInspector : Editor
{
	const int MIN_ZOOM = 0;
	const int MAX_ZOOM = 20;

	static string lattitudeLabel = "Lattitude (decimal): ";
	static string longitudeLabel = "Longitude (decimal): ";
	static string zoomLabel = "Zoom: ";

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (Application.isPlaying)
		{
			EditorGUILayout.LabelField("Currently play mode editting is not allowed");
			return;
		}

		GoogleMapsTexture mapsTexture = (GoogleMapsTexture)target;

		EditorGUILayout.HelpBox("HCH: The Tile Image URL I'm using for our 3d demo purposes is not a proper Bing authorized URL, as we have not procured a proper API key", MessageType.Warning);

		string errorMessage = "";
		if (GoogleMapsTexture.ValidateServerURL(mapsTexture.CurrentFixedUrl(), out errorMessage) == false)
		{
			EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
		}

		mapsTexture.latitude = EditorGUILayout.FloatField(lattitudeLabel, mapsTexture.latitude);
		mapsTexture.longitude = EditorGUILayout.FloatField(longitudeLabel, mapsTexture.longitude);
		int zoom = EditorGUILayout.IntSlider(zoomLabel, mapsTexture.initialZoom, MIN_ZOOM, MAX_ZOOM);
		foreach (GoogleMapsTexture t in targets)
		{
			t.initialZoom = zoom;
			t.ComputeInitialSector();
		}

		if (GUILayout.Button("Update preview (may take a while)"))
		{
			foreach (GoogleMapsTexture t in targets)
				t.RequestTexturePreview();
		}

		if (mapsTexture.IsDownloading())
		{
			EditorGUILayout.HelpBox("Downloading texture from server...", MessageType.Info);
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(mapsTexture);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
	}
}
#endif