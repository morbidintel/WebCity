using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{    
    public class DatePicker_Header : MonoBehaviour
    {
        public Text HeaderText;
        public DatePicker_Button PreviousMonthButton;
        public DatePicker_Button NextMonthButton;
        public DatePicker_Button PreviousYearButton;
        public DatePicker_Button NextYearButton;
        public Image Background;
        public TableRow Row;
    }    
}
