using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InputField))]
public class RenameHandler : MonoBehaviour
{
	InputField input = null;

	[SerializeField]
	InputField.SubmitEvent OnEnter = null;
	[SerializeField]
	UnityEvent OnCancel = null;

	void Awake()
	{
		input = GetComponent<InputField>();
	}

	void Update()
	{
		if (EventSystem.current.currentSelectedGameObject == gameObject)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				OnEnter.Invoke(input.text);
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				OnCancel.Invoke();
			}
		}
	}
}
