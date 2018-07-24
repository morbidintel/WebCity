using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UI.Dates
{
    [CustomEditor(typeof(DatePicker)), CanEditMultipleObjects]
    public class DatePickerEditor : Editor
    {
        public void OnEnable()
        {            
        }

        public override void OnInspectorGUI()
        {
            var datePicker = ((DatePicker)target);
            if (GUILayout.Button("Force Update"))
            {
                datePicker.UpdateDisplay();
            }

            if (GUILayout.Button("Invalidate Button Templates"))
            {
                datePicker.InvalidateAllDayButtonTemplates();
            }

            if (DrawDefaultInspector())
            {
                datePicker.UpdateDisplay();
                DatePickerTimer.DelayedCall(0.005f, datePicker.UpdateDisplay, datePicker);
            }
        }        
    }
}
