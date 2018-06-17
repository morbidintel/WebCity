using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class InputFieldEnter : MonoBehaviour
{
	// This GameObject's Selectable component (can only have 1)
	Selectable selectable;

	// Use this for initialization
	void Start()
	{
		selectable = GetComponent<Selectable>();
	}

	// Update is called once per frame
	void Update()
	{
		if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && 
			EventSystem.current.currentSelectedGameObject == gameObject)
		{
			var system = EventSystem.current;
			Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

			if (next != null)
			{
				InputField inputfield = next.GetComponent<InputField>();
				if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret
				system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
			}
		}
	}
}
