using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTag : MonoBehaviour
{
	[SerializeField]
	public Text placeName = null;

	public bool ignoreCameraZoom = true;

	// Use this for initialization
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 ptVec = transform.position - Camera.main.transform.position;
		Vector3 ptProj = Vector3.Project(ptVec, Camera.main.transform.forward);
		Vector3 newScale = Vector3.one * (ptProj.magnitude / 500.0f);
		if (newScale.x < .5f) transform.localScale = newScale;
	}
}
