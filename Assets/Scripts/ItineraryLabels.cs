using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;
using PhpDB;

public class SidebarLabel : MonoBehaviour
{
	public Image image;
	public Toggle toggle;
	public InputField input;
	public int index;
}

public class ItineraryLabels : Singleton<ItineraryLabels>
{
	[SerializeField]
	RectTransform labelsHolder = null;
	[SerializeField]
	GameObject labelPrefab = null;
	[SerializeField]
	RectTransform PlacesPageTransform = null;
	[SerializeField]
	VerticalLayoutGroup vLayoutGroup = null;

	Image maskImage;

	List<SidebarLabel> labels = new List<SidebarLabel>();

	public static Color[] colors = new Color[10]
	{
		new Color(97/255f, 189/255f, 79/255f),
		new Color(242/255f, 214/255f, 0),
		new Color(1, 159/255f, 26/255f),
		new Color(235/255f, 90/255f, 70/255f),
		new Color(195/255f, 119/255f, 224/255f),
		new Color(0, 121/255f, 191/255f),
		new Color(0, 194/255f, 224/255f),
		new Color(81/255f, 232/255f, 152/255f),
		new Color(1, 120/255f, 203/255f),
		new Color(122/255f, 1, 1)
	};

	void Start()
	{
		maskImage = GetComponent<Image>();
		maskImage.color = Color.white;

		SidebarLabel prefabScript = labelPrefab.AddComponent<SidebarLabel>();
		prefabScript.image = labelPrefab.GetComponent<Image>();
		prefabScript.toggle = labelPrefab.GetComponentInChildren<Toggle>();
		prefabScript.input = labelPrefab.GetComponentInChildren<InputField>();

		for (int i = 0; i < 10; ++i)
		{
			SidebarLabel label = Instantiate(labelPrefab, labelsHolder).GetComponent<SidebarLabel>();
			label.image.color = colors[i];
			label.input.text = "";
			label.index = i;
			label.name = "Label " + (i + 1);
			labels.Add(label);
		}

		Destroy(labelPrefab);

		StartCoroutine(LateStart());
	}

	IEnumerator LateStart()
	{
		yield return new WaitForEndOfFrame();
		vLayoutGroup.enabled = false;
	}

	void Update()
	{
		maskImage.color = maskImage.color.WithAlpha(
			Sidebar.Instance.currentPage == Sidebar.Page.Places ? 1 : 0);
	}

	public void Init(Itinerary itinerary)
	{
		string[] labelNames = itinerary.GetLabels();
		for (int i = 0; i < labels.Count; ++i)
		{
			labels[i].input.text =
				i < labelNames.Length ? labelNames[i] : "";
		}
	}

	public void OnSubmitRenameLabel(GameObject labelObject)
	{
		var itinerary = Sidebar.Instance.currentItinerary.itinerary;
		var label = labelObject.GetComponent<SidebarLabel>();
		if (itinerary == null || !label) return;

		label.toggle.isOn = false;
		itinerary.SetLabel(label.index, label.input.text);
		Sidebar.Instance.UpdateCurrentItinerary(itinerary);
	}

	public void OnCancelRenameLabel(GameObject labelObject)
	{
		var itinerary = Sidebar.Instance.currentItinerary.itinerary;
		var label = labelObject.GetComponent<SidebarLabel>();
		if (itinerary == null || !label) return;

		label.toggle.isOn = false;
		label.input.text = itinerary.GetLabels()[label.index];
	}
}
