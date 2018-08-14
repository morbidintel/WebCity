using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;

public class SidebarLabel : MonoBehaviour
{
	public Image image;
	public InputField input;
}

public class ItineraryLabels : Singleton<ItineraryLabels>
{
	[SerializeField]
	RectTransform PlacesPageTransform = null;
	[SerializeField]
	VerticalLayoutGroup vLayoutGroup = null;
	[SerializeField]
	RectTransform labelsHolder = null;
	[SerializeField]
	GameObject labelPrefab = null;

	List<SidebarLabel> labels;

	Color[] colors = new Color[10]
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
		SidebarLabel prefabScript = labelPrefab.AddComponent<SidebarLabel>();
		prefabScript.image = labelPrefab.GetComponent<Image>();
		prefabScript.input = labelPrefab.GetComponentInChildren<InputField>();

		for (int i = 0; i < 10; ++i)
		{
			SidebarLabel label = Instantiate(labelPrefab, labelsHolder).GetComponent<SidebarLabel>();
			label.image.color = colors[i];
			label.input.text = "";
			label.name = "Label " + (i + 1);
		}

		Destroy(labelPrefab);
	}

	IEnumerator LateStart()
	{
		yield return new WaitForEndOfFrame();
		vLayoutGroup.enabled = false;
	}

	void Update()
	{
		var sidebarTransform = transform.parent as RectTransform;
		labelsHolder.position = PlacesPageTransform.position.WithIncX(
			sidebarTransform.rect.width / 2);
	}
}
