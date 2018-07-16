using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SpaceManagement
{
    public class ColorToggle : MonoBehaviour
    {
        Graphic graphic;

        public Color valueFalseColor, valueTrueColor;

        // Use this for initialization
        void Start()
        {
            graphic = GetComponent<Graphic>();
        }

        public void ToggleColor(bool value)
        {
            graphic.color = value ? valueTrueColor : valueFalseColor;
        }
    }
}