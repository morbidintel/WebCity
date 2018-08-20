using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Parent class for tooltips
/// </summary>
public class TooltipMenu : MonoBehaviour
{
	/// <summary>
	/// Root GameObject for the tooltip
	/// </summary>
	[SerializeField]
	protected GameObject tooltipRoot = null;

	//public Text label;

	Vector2 spacingFromCursor = new Vector2(10, -10);

	// Use this for initialization
	protected virtual void Start()
	{
		var graphics = tooltipRoot.GetComponentsInChildren<Graphic>();
		foreach (var g in graphics)
		{
			if ((g is Image || g is Text) && g.material != null)
			{
				g.material.renderQueue = 5000;
			}
		}

		tooltipRoot.transform.position = Input.mousePosition.To2DXY() + spacingFromCursor;
	}

	// Update is called once per frame
	protected virtual void Update()
	{
	}

	public virtual void OpenTooltip()
	{
		tooltipRoot.SetActive(true);
	}

	public virtual void CloseTooltip()
	{
		tooltipRoot.SetActive(false);
	}
}
