using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class InputFieldTab : MonoBehaviour
{
	Selectable selectable;

	void Start()
	{
		selectable = GetComponent<Selectable>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject == gameObject)
		{
			var system = EventSystem.current;
			bool hasShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			Selectable next = hasShift ? selectable.FindSelectableOnUp() : selectable.FindSelectableOnDown();

			if (next != null)
			{
				InputField inputfield = next.GetComponent<InputField>();
				if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret
				system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
			}
		}
	}
}
