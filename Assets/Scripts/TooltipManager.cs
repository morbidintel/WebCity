using System;
using UnityEngine;

public class TooltipManager : Gamelogic.Extensions.Singleton<TooltipManager>
{
	[SerializeField]
	TooltipWorld AddPlaceTooltip = null, RemovePlaceTooltip = null;

	public void OpenAddPlaceTooltip(MapTag tag)
	{
		AddPlaceTooltip.action = place => Sidebar.Instance.OnClickAddPlaceTooltip(place);
		AddPlaceTooltip.OpenTooltip(tag);
	}

	public void OpenRemovePlaceTooltip(MapTag tag)
	{
		//RemovePlaceTooltip.action = place => 
		RemovePlaceTooltip.OpenTooltip(tag);
	}
}
