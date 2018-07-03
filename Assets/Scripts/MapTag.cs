using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleMaps;

public class MapTag : MonoBehaviour, IPointerClickHandler
{
	[SerializeField]
	public Text placeName = null;
	[SerializeField]
	LineRenderer line = null;

	public PlaceDetails place = null;
	public bool ignoreCameraZoom = true;
	public float tagHeight = 0.4f;

	// Use this for initialization
	void Start()
	{
		name = placeName?.text;
		line.useWorldSpace = true;
		line.loop = false;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 ptVec = transform.position - Camera.main.transform.position;
		Vector3 ptProj = Vector3.Project(ptVec, Camera.main.transform.forward);
		Vector3 newScale = Vector3.one * (ptProj.magnitude / 900);
		if (newScale.x < .9) transform.localScale = newScale;

		// Always facing the camera
		transform.LookAt(
			transform.position + Camera.main.transform.rotation * Vector3.forward,
			Camera.main.transform.rotation * Vector3.up);

		transform.position = transform.position.SetY(tagHeight);

		line.SetPositions(new Vector3[] { transform.position, transform.position.SetY(0) });
		//line.startWidth = line.endWidth = 1 / newScale.x;
	}

	void OnDestroy()
	{
		Destroy(gameObject);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (place != null && !Sidebar.Instance.ShowItineraries)
			TooltipManager.Instance.OpenAddPlaceTooltip(this);
	}
}
