using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class InputFieldEnter : MonoBehaviour
{
	[SerializeField]
	Button button = null;

	// Update is called once per frame
	void Update()
	{
		if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) &&
			EventSystem.current.currentSelectedGameObject == gameObject &&
			button != null)
		{
			button.onClick.Invoke();
		}
	}
}
