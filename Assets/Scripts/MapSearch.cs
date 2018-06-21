using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Google_Maps;

public class MapSearch : MonoBehaviour
{
	[SerializeField]
	InputField input = null;
	[SerializeField]
	Dropdown dropdown = null;

	Coroutine coroutine;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (input.isFocused &&
			input.text != "" &&
			!dropdown.template.gameObject.activeInHierarchy &&
			coroutine == null)
			Autocomplete(input.text);
	}

	IEnumerator AutocompleteCoroutine(string value)
	{
		yield return new WaitForSecondsRealtime(.5f);

		string url = string.Format(QueryAutocomplete.URL, WWW.EscapeURL(value));
		WWW www = new WWW(url);
		yield return www;
		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		QueryAutocomplete result = JsonUtility.FromJson<QueryAutocomplete>(www.text);

		if (result.status != "OK")
		{
			Debug.Log(result.error_message);
			yield break;
		}

		dropdown.ClearOptions();
		dropdown.AddOptions(result.predictions?.Select(p => p.description).ToList());
		dropdown.Show();
		coroutine = null;
	}

	// called by the InputField.OnValueChanged(string)
	public void Autocomplete(string value)
	{
		if (coroutine != null) StopCoroutine(coroutine);
		coroutine = StartCoroutine(AutocompleteCoroutine(value));
	}

	// called by the Dropdown.OnValueChanged(int)
	public void SelectAutocomplete(int value)
	{

	}
}
