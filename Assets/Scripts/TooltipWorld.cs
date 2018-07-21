using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleMaps;

public class TooltipWorld : TooltipMenu, IPointerClickHandler, IDragHandler, IPointerUpHandler, IScrollHandler
{
	GraphicRaycaster gRayCaster = null;

	public PlaceDetails place = null;

	public System.Action<PlaceDetails> action = null;

	// Use this for initialization
	protected override void Start()
	{
		var graphics = tooltipRoot.GetComponentsInChildren<Graphic>();
		foreach (var g in graphics)
		{
			if ((g is Image || g is Text) && g.material != null)
			{
				g.material.renderQueue = 5000;
			}
		}

		gRayCaster = GetComponent<GraphicRaycaster>();
	}

	// Update is called once per frame
	protected override void Update()
	{
		Vector3 ptVec = transform.position - Camera.main.transform.position;
		Vector3 ptProj = Vector3.Project(ptVec, Camera.main.transform.forward);
		Vector3 newScale = Vector3.one * (ptProj.magnitude / 900);
		if (newScale.x < .9) transform.localScale = newScale;

		// Always facing the camera
		transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
			Camera.main.transform.rotation * Vector3.up);
	}

	public void OpenTooltip(MapTag tag)
	{
		place = tag.place;
		OpenTooltip();
		tooltipRoot.transform.position = tag.transform.position;
		Update();
	}

	public void OnClick()
	{
		action.Invoke(place);
		CloseTooltip();
	}

	public void OnClickAddToItinerary()
	{
		Sidebar.Instance.OnClickAddPlaceTooltip(place);
		CloseTooltip();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		CloseTooltip();
	}

	public void OnDrag(PointerEventData eventData)
	{
		gRayCaster.enabled = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		gRayCaster.enabled = true;
	}

	Coroutine coroutine = null;
	public void OnScroll(PointerEventData eventData)
	{
		gRayCaster.enabled = !eventData.IsScrolling();
		if (coroutine != null) StopCoroutine(coroutine);
		coroutine = StartCoroutine(EnableRaycasterCoroutine(0.2f));
	}

	IEnumerator EnableRaycasterCoroutine(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		gRayCaster.enabled = true;
		coroutine = null;
	}
}
