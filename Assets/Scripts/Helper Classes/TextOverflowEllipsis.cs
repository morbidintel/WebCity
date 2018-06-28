using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextOverflowEllipsis : Text
{
	static readonly int _THRESHOLD = 3;
	string updatedText = "";

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		Vector2 extents = rectTransform.rect.size;
		var settings = GetGenerationSettings(extents);
		cachedTextGenerator.Populate(base.text, settings);

		float scale = extents.x / preferredWidth;
		//text is going to be truncated, 
		//cant update the text directly as we are in the graphics update loop
		if (scale < 1 && cachedTextGenerator.characterCount >= _THRESHOLD)
		{
			updatedText = base.text.Substring(0, cachedTextGenerator.characterCount - _THRESHOLD);
			updatedText += "...";
		}

		base.OnPopulateMesh(toFill);
	}

	void Update()
	{
		if (updatedText != "")
		{
			base.text = updatedText;
			updatedText = "";
		}
	}
}