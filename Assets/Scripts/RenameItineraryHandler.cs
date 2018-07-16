using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RenameItineraryHandler : MonoBehaviour
{
	[SerializeField]
	InputField input = null;

	void Update()
	{
		if (EventSystem.current.currentSelectedGameObject == gameObject)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				Sidebar.Instance.OnSubmitRenameItinerary();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				Sidebar.Instance.OnCancelRenameItinerary();
			}
		}
	}
}
