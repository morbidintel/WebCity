using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;
using PhpDB;

/// <summary>
/// Handles the colored Labels on the right of the Sidebar.
/// </summary>
public class ItineraryLabels : Singleton<ItineraryLabels>
{
	/// <summary>
	/// Element class to hold each of the inner components of each Label.
	/// </summary>
	class SidebarLabel : MonoBehaviour
	{
		public Image image;
		public Toggle toggle;
		public InputField input;
		public int index; // from 0 to 9, for easy referencing of ItineraryLabels.Colors
	}

	/// <summary>
	/// Parent Transform that holds all the Labels.
	/// </summary>
	[SerializeField]
	RectTransform labelsHolder = null;

	/// <summary>
	/// Prefab GameObject of the Label for instantiation.
	/// </summary>
	[SerializeField]
	GameObject labelPrefab = null;

	/// <summary>
	/// Image that serves as the Graphic to the Mask that constrains our Labels.
	/// </summary>
	Image maskImage;

	/// <summary>
	/// The instantiated Labels.
	/// </summary>
	List<SidebarLabel> labels = new List<SidebarLabel>();

	/// <summary>
	/// The standard 10 label colors.
	/// </summary>
	public static readonly Color[] Colors = new Color[10]
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
		maskImage.color = Color.white.WithAlpha(0); // hide our Labels

		// prepare our prefab to be instantiated
		SidebarLabel prefabScript = labelPrefab.AddComponent<SidebarLabel>();
		prefabScript.image = labelPrefab.GetComponent<Image>();
		prefabScript.toggle = labelPrefab.GetComponentInChildren<Toggle>();
		prefabScript.input = labelPrefab.GetComponentInChildren<InputField>();

		// create the 10 labels
		for (int i = 0; i < 10; ++i)
		{
			SidebarLabel label = Instantiate(labelPrefab, labelsHolder).GetComponent<SidebarLabel>();
			label.image.color = Colors[i];
			label.input.text = "";
			label.index = i;
			label.name = "Label " + (i + 1);
			labels.Add(label);
		}

		// prefab is destroyed so it doesn't show up
		Destroy(labelPrefab);
	}

	void Update()
	{
		// hide our Labels when we are not in Places page
		maskImage.color = maskImage.color.WithAlpha(
			Sidebar.Instance.currentPage == Sidebar.Page.Places ? 1 : 0);
	}

	/// <summary>
	/// Initialize our Labels with the label names from an Itinerary.
	/// </summary>
	/// <param name="itinerary"></param>
	public void Init(Itinerary itinerary)
	{
		string[] labelNames = itinerary.GetLabels();
		for (int i = 0; i < labels.Count; ++i)
		{
			labels[i].input.text =
				i < labelNames.Length ? labelNames[i] : "";
		}
	}

	/// <summary>
	/// Sends the new Label name to be updated in the database.
	/// </summary>
	/// <param name="labelObject">The Label GameObject that has a SidebarLabel component.</param>
	/// <remarks>
	/// Called by Label -> InputField -> RenameHandler.OnEnter() event.
	/// Parameter is GameObject because SidebarLabel class is added in runtime.
	/// </remarks>
	public void OnSubmitRenameLabel(GameObject labelObject)
	{
		var itinerary = Sidebar.Instance.currentItinerary.itinerary;
		var label = labelObject.GetComponent<SidebarLabel>();
		if (itinerary == null || !label) return;

		label.toggle.isOn = false;
		itinerary.SetLabel(label.index, label.input.text);
		Sidebar.Instance.UpdateCurrentItinerary(itinerary);
	}

	/// <summary>
	/// Resets the text on the Label GameObject.
	/// </summary>
	/// <param name="labelObject">The Label GameObject that has a SidebarLabel component.</param>
	/// <remarks>
	/// Called by Label -> InputField -> RenameHandler.OnCancel() event.
	/// Parameter is GameObject because SidebarLabel class is added in runtime.
	/// </remarks>
	public void OnCancelRenameLabel(GameObject labelObject)
	{
		var itinerary = Sidebar.Instance.currentItinerary.itinerary;
		var label = labelObject.GetComponent<SidebarLabel>();
		if (itinerary == null || !label) return;

		label.toggle.isOn = false;
		label.input.text = itinerary.GetLabels()[label.index];
	}

	/// <summary>
	/// Resets the text on the Label GameObject when the edit button is toggled off.
	/// </summary>
	/// <param name="labelObject">The Label GameObject that has a SidebarLabel component.</param>
	/// <remarks>
	/// Called by Label -> Toggle.OnValueChanged() event.
	/// Parameter is GameObject because SidebarLabel class is added in runtime.
	/// </remarks>
	public void OnLabelToggleValueChanged(GameObject labelObject)
	{
		var itinerary = Sidebar.Instance.currentItinerary.itinerary;
		var label = labelObject.GetComponent<SidebarLabel>();
		if (itinerary == null || !label || label.toggle.isOn) return;
		label.input.text = itinerary.GetLabels()[label.index];
	}
}
