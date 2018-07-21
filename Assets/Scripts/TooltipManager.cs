using System;
using UnityEngine;

public class TooltipManager : Gamelogic.Extensions.Singleton<TooltipManager>
{
	[SerializeField]
	TooltipWorld AddPlaceTooltip = null, RemovePlaceTooltip = null;

	TooltipWorld currentTooltip = null;

	public void ToggleAddPlaceTooltip(MapTag tag)
	{
		ToggleTooltip(AddPlaceTooltip, place => Sidebar.Instance.OnClickAddPlaceTooltip(place), tag);
	}

	public void ToggleRemovePlaceTooltip(MapTag tag)
	{
		ToggleTooltip(RemovePlaceTooltip, place => Sidebar.Instance.OnClickRemovePlaceTooltip(place), tag);
	}

	void ToggleTooltip(TooltipWorld tooltip, Action<GoogleMaps.PlaceDetails> action, MapTag tag)
	{
		currentTooltip?.CloseTooltip();

		if (currentTooltip != tooltip || currentTooltip.place != tag.place)
		{
			currentTooltip = tooltip;
			tooltip.action = action;
			tooltip.OpenTooltip(tag);
		}
		else
		{
			currentTooltip = null;
		}
	}
}
