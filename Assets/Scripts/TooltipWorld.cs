using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipWorld : TooltipMenu, IPointerClickHandler, IDragHandler, IPointerUpHandler, IScrollHandler
{
	GraphicRaycaster gRayCaster = null;

	MapTag tag = null;

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
		this.tag = tag;
		OpenTooltip();
		tooltipRoot.transform.position = tag.transform.position;
	}

	public void OnClickAddToItinerary()
	{
		// TODO: tag.place seems empty...
		Sidebar.Instance.OnClickAddPlaceTooltip(tag.place);
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
