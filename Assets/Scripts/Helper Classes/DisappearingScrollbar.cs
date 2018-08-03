using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gamelogic.Extensions;
namespace SpaceManagement
{
    // Add to a GameObject with an Image
    // Will make the image disappear when it is not moving
    [RequireComponent(typeof(Image))]
    public class DisappearingScrollbar : MonoBehaviour
    {

        Image image;
        Vector3 lastPos;
        Color origColor;

        [Range(0, 1)]
        public float fadeSpeed = .25f;
        public float delay = .5f;
        public bool lockFade = false;

        float deltaTime;

        // Use this for initialization
        void Start()
        {
            image = GetComponent<Image>();
            origColor = image.color;
            image.color = origColor.WithAlpha(0); // make it transparent at the start
            lastPos = image.rectTransform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (!lockFade && image.rectTransform.position.Approximately(lastPos))
            {
                if (deltaTime > delay) image.color = Color.Lerp(image.color, origColor.WithAlpha(0), fadeSpeed);
                else deltaTime += Time.deltaTime;
            }
            else
            {
                image.color = origColor;
                deltaTime = 0;
            }

            lastPos = image.rectTransform.position;
        }
    }
}