using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapRaycaster : Gamelogic.Extensions.Singleton<MapRaycaster>
{
	GameObject lastClicked = null;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyUp(KeyCode.Mouse0) && !MapCamera.Instance.IsMoving && !MapCamera.Instance.HasMoved)
		{
			if (IsPointerOverUIObject()) return;

			Ray touchRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			//do a simple raycast 
			RaycastHit hitinfo = new RaycastHit();
			//LayerMask mask = LayerMask.NameToLayer("Clickable");
			//int maskval = 1 << mask.value;
			int maskval = ~0;
			if (Physics.Raycast(touchRay, out hitinfo, 10000.0f, maskval))
			{
				//if (lastClicked != hitinfo.transform.gameObject)
				{
					//do click actions
					lastClicked = hitinfo.transform.gameObject;
				}
			}
			else
			{
				lastClicked = null;
			}
		}
	}

	public static bool IsPointerOverUIObject()
	{
		if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
		{
			return true;
		}

		PointerEventData ped = new PointerEventData(EventSystem.current);
		ped.position = Input.mousePosition;
		if (Input.touchCount > 0)
		{
			ped.position = Input.GetTouch(0).position;
		}
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(ped, results);

		return results.Count > 0;
	}
}
