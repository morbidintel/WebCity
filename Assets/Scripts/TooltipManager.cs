using System.Collections;
using System.Collections.Generic;
using GoogleMaps;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TooltipManager : Gamelogic.Extensions.Singleton<TooltipManager>
{
	[SerializeField]
	TooltipWorld AddPlaceTooltip;

	public void OpenAddPlaceTooltip(MapTag tag)
	{
		AddPlaceTooltip.OpenTooltip(tag);
	}
}
