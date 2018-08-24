using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapRaycaster : Gamelogic.Extensions.Singleton<MapRaycaster>
{
	public static bool IsPointerOverUIObject()
	{
		if (EventSystem.current.IsPointerOverGameObject())
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
