using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{
    public class DatePicker_DayButton : DatePicker_Button
    {
        public DateTime Date;
        public TableCell Cell;

        [HideInInspector]
        public DatePicker DatePicker;
                
        public void Clicked()
        {
            if (!Button.interactable) return;

            DatePicker.DayButtonClicked(Date);
        }

        public void MouseOver()
        {
            if (!Button.interactable) return;

            DatePicker.DayButtonMouseOver(Date);
        }        
    }
}
