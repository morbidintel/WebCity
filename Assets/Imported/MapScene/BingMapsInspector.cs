using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;
using System.IO;

#if UNITY_EDITOR
[CustomEditor(typeof(BingMapsTexture))]
[CanEditMultipleObjects]
public class BingMapsInspector : Editor
{
	const int MIN_ZOOM = 0;
	const int MAX_ZOOM = 20;

	static string lattitudeLabel = "Lattitude (decimal): ";
	static string longitudeLabel = "Longitude (decimal): ";
	static string zoomLabel = "Zoom: ";


	public override void OnInspectorGUI()
	{
        base.OnInspectorGUI();
        //DrawDefaultInspector();
        if (Application.isPlaying) {
			EditorGUILayout.LabelField ("Currently play mode editting is not allowed");
			return;
		}

		BingMapsTexture bingMapsTexture = (BingMapsTexture)target;        
		
		EditorGUILayout.HelpBox("HCH: The Tile Image URL I'm using for our 3d demo purposes is not a proper Bing authorized URL, as we have not procured a proper API key", MessageType.Warning);
		

		string errorMessage = "";
		if( BingMapsTexture.ValidateServerURL(bingMapsTexture.CurrentFixedUrl(), out errorMessage) == false ){
			EditorGUILayout.HelpBox (errorMessage, MessageType.Error);
		}

		bingMapsTexture.latitude = EditorGUILayout.FloatField(lattitudeLabel, bingMapsTexture.latitude);
		bingMapsTexture.longitude = EditorGUILayout.FloatField(longitudeLabel, bingMapsTexture.longitude);
		bingMapsTexture.initialZoom = EditorGUILayout.IntSlider(zoomLabel, bingMapsTexture.initialZoom, MIN_ZOOM, MAX_ZOOM);
		bingMapsTexture.ComputeInitialSector ();

		if (GUILayout.Button ("Update preview (may take a while)")) {
			bingMapsTexture.RequestTexturePreview ();
		}

		if (bingMapsTexture.IsDownloading ()) {
			EditorGUILayout.HelpBox("Downloading texture from server...", MessageType.Info);
		}

		if (GUI.changed) {
			EditorUtility.SetDirty (bingMapsTexture);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}
}
#endif