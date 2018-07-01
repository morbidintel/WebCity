using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleMaps;

public class MapSearch : MonoBehaviour
{
	protected internal class DropdownItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
	{
		public Text text;
		public Button button;
		public int index;

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}

		public virtual void OnCancel(BaseEventData eventData)
		{
			MapSearch dropdown = GetComponentInParent<MapSearch>();
			if (dropdown) dropdown.HideDropdown();
		}
	}

	[SerializeField]
	InputField input = null;
	[SerializeField]
	Transform dropdownHolder = null;
	[SerializeField]
	GameObject itemTemplate = null;

	List<DropdownItem> items = new List<DropdownItem>();
	QueryAutocomplete result = null;
	Coroutine coroutine;
	GameObject blocker = null;
	MapTag currentTag = null;

	bool isSearchSelected
	{
		get
		{
			return EventSystem.current.currentSelectedGameObject == input.gameObject;
		}
	}

	// Use this for initialization
	void Start()
	{
		Debug.Assert(input != null);
		Debug.Assert(dropdownHolder != null);
		Debug.Assert(itemTemplate != null);
		itemTemplate.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		if (isSearchSelected &&
			input.text != "" &&
			!dropdownHolder.gameObject.activeInHierarchy &&
			coroutine == null)
		{
			ShowDropdown();
		}
	}

	void AddItems(IEnumerable<string> texts)
	{
		foreach (string t in texts)
		{
			DropdownItem item = Instantiate(itemTemplate, dropdownHolder).AddComponent<DropdownItem>();
			item.button = item.GetComponentInChildren<Button>();
			item.button.onClick.AddListener(() => SelectAutocomplete(item.index));
			item.text = item.GetComponentInChildren<Text>();
			item.text.text = t;
			item.index = items.Count;
			item.gameObject.SetActive(true);
			item.name = "Item " + items.Count;
			items.Add(item);
		}
	}

	public void ShowDropdown()
	{
		dropdownHolder.gameObject.SetActive(true);
		if (!blocker) blocker = CreateBlocker(GetComponent<Canvas>());
	}

	public void HideDropdown()
	{
		dropdownHolder.gameObject.SetActive(false);
		if (blocker)
		{
			Destroy(blocker);
			blocker = null;
		}
	}

	void ClearDropdown()
	{
		foreach (var item in items)
		{
			Destroy(item.gameObject);
		}
		items.Clear();
	}

	// called by the InputField.OnValueChanged(string)
	public void Autocomplete(string value)
	{
		if (coroutine != null) StopCoroutine(coroutine);
		coroutine = StartCoroutine(AutocompleteCoroutine(value));
	}

	// called by the Button.OnClick()
	public void SelectAutocomplete(int index)
	{
		HideDropdown();
		if (coroutine != null) StopCoroutine(coroutine);
		coroutine = StartCoroutine(GoToPlaceCoroutine(index));
	}

	GameObject CreateBlocker(Canvas rootCanvas)
	{
		// Create blocker GameObject.
		GameObject blocker = new GameObject("Blocker");

		// Setup blocker RectTransform to cover entire root canvas area.
		RectTransform blockerRect = blocker.AddComponent<RectTransform>();
		blockerRect.SetParent(rootCanvas.transform, false);
		blockerRect.anchorMin = Vector3.zero;
		blockerRect.anchorMax = Vector3.one;
		blockerRect.sizeDelta = Vector2.zero;

		// Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
		Canvas blockerCanvas = blocker.AddComponent<Canvas>();
		blockerCanvas.overrideSorting = true;
		Canvas dropdownCanvas = dropdownHolder.GetComponent<Canvas>();
		blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
		blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

		// Add raycaster since it's needed to block.
		blocker.AddComponent<GraphicRaycaster>();

		// Add image since it's needed to block, but make it clear.
		Image blockerImage = blocker.AddComponent<Image>();
		blockerImage.color = Color.clear;

		// Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
		Button blockerButton = blocker.AddComponent<Button>();
		blockerButton.onClick.AddListener(HideDropdown);

		return blocker;
	}

	IEnumerator AutocompleteCoroutine(string value)
	{
		yield return new WaitForSecondsRealtime(.5f);
		if (value != "")
		{
			string url = string.Format(QueryAutocomplete.URL + "&location={1}&radius={2}",
				WWW.EscapeURL(value),
				MapCamera.Instance.GetCameraCoords().ToString(),
				MapCamera.Instance.GetRadius());
			WWW www = new WWW(PHPProxy.Escape(url));
			yield return www;
			if (www.error != null)
			{
				Debug.Log(www.error);
				yield break;
			}

			result = JsonUtility.FromJson<QueryAutocomplete>(www.text);

			if (result.status != "OK")
			{
				Debug.Log(result.error_message);
				yield break;
			}
		}
		else
		{
			result = null;
		}

		ClearDropdown();
		if (result != null) AddItems(result.predictions?.Select(p => p.description));
		if (isSearchSelected) ShowDropdown();

		input.MoveTextEnd(false);

		coroutine = null;
	}

	IEnumerator GoToPlaceCoroutine(int index)
	{
		input.text = result.predictions[index].description;
		string place_id = result != null ? result.predictions[index].place_id : "";
		if (place_id == "") yield break;
		string url = string.Format(PlaceDetails.URL, WWW.EscapeURL(place_id), "name,geometry,place_id");
		WWW www = new WWW(PHPProxy.Escape(url));
		yield return www;
		if (www.error != null)
		{
			Debug.Log(www.error);
			yield break;
		}

		PlaceDetails place = JsonUtility.FromJson<PlaceDetails>(www.text);

		if (place.status != "OK")
		{
			Debug.Log(place.error_message);
			yield break;
		}

		if (place?.result?.geometry != null)
			MapCamera.Instance.SetCameraViewport(place.result.geometry);
		EventSystem.current.SetSelectedGameObject(null);
		if (currentTag) MapTagManager.Instance.ClearMapTag(currentTag);
		currentTag = MapTagManager.Instance.ShowPlaceOnMap(place);
		currentTag.place = place;

		coroutine = null;
	}
}
