using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;

[RequireComponent(typeof(Image))]
public class SpriteToggle : MonoBehaviour
{
	Image image;

	[SerializeField]
	Sprite valueFalseSprite, valueTrueSprite;

	public float fadeSpeed = 10.0f;

	// Use this for initialization
	void Start()
	{
		image = GetComponent<Image>();
	}

	public void ToggleSprite(bool value)
	{
		image.sprite = value ? valueTrueSprite : valueFalseSprite;
	}

	public void ToggleSpriteFade(bool value)
	{
		StopAllCoroutines();
		StartCoroutine(FadeCoroutine(value));
	}

	IEnumerator FadeCoroutine(bool value)
	{
		yield return new WaitUntil(() =>
		{
			if (image.color.a > 0)
				image.color = image.color.WithAlpha(
					image.color.a - (Time.deltaTime * fadeSpeed));
			return image.color.a <= 0;
		});

		image.sprite = value ? valueTrueSprite : valueFalseSprite;

		yield return new WaitUntil(() =>
		{
			if (image.color.a < 1)
				image.color = image.color.WithAlpha(
					image.color.a + (Time.deltaTime * fadeSpeed));
			return image.color.a >= 1;
		});
	}
}
