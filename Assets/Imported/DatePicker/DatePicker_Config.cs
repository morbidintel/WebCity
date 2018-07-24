using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UI.Tables;

namespace UI.Dates
{
    [Serializable]
    public class DatePickerEvent : UnityEvent<DateTime> { }

    [Serializable]
    public struct DatePickerConfig
    {
        public DatePickerMiscConfig Misc;
        [Space]
        public DatePickerSizeConfig Sizing;
        [Space]
        public DatePickerModalConfig Modal;
        [Space]
        public DatePickerDateRangeConfig DateRange;
        [Space]
        public DatePickerFormatConfig Format;
        [Space]
        public DatePickerBorderConfig Border;
        [Space]
        public DatePickerHeaderConfig Header;
        [Space]
        public DatePickerWeekDaysConfig WeekDays;
        [Space]
        public DatePickerDayConfig Days;
        [Space]
        public DatePickerAnimationConfig Animation;
        [Space]
        public DatePickerInputFieldConfig InputField;
        [Space]
        public DatePickerEventConfig Events;        
    }

    [Serializable]
    public class DatePickerSizeConfig
    {
        public bool OverrideTransformHeight = false;
        public float PreferredHeight = 256f;

        public bool UsePreferredWidthInsteadOfInputFieldWidth = false;
        public float PreferredWidth = 256f;
    }

    [Serializable]
    public class DatePickerDayConfig
    {
        public Color BackgroundColor;

        public Font Font;
        public int FontSize;

        public DatePickerDayButtonConfig CurrentMonth;
        public DatePickerDayButtonConfig OtherMonths;
        public DatePickerDayButtonConfig Today;
        public DatePickerDayButtonConfig SelectedDay;
    }

    [Serializable]
    public class DatePickerBorderConfig
    {
        public RectOffset Size = null;
        public Color Color;
    }

    [Serializable]
    public class DatePickerDayButtonConfig
    {        
        [Tooltip("If this value is set, then the values provided here will override the values of the template. If you wish to modify the template directly, this value should be cleared.")]
        public bool OverrideTemplate = true;

        [Header("Text")]
        public Color TextColor;

        [Header("Background")]
        public Sprite BackgroundImage;
        public ColorBlock BackgroundColors;        

        public void ApplyConfig(DatePicker_Button button)
        {
            if (!OverrideTemplate) return;

            button.Text.color = TextColor;
            //button.Button.colors = BackgroundColors;
            button.Button.colors.Apply(BackgroundColors);
            button.Button.image.sprite = BackgroundImage;
        }
    }

    [Serializable]
    public class DatePickerWeekDaysConfig
    {
        [Header("Week Numbers")]
        public bool ShowWeekNumbers = false;
        public WeekNumberMode WeekNumberMode = WeekNumberMode.WeekOfYear;
        public CalendarWeekRule CalendarWeekRule = CalendarWeekRule.FirstFullWeek;
        [Tooltip("0 == Auto")]
        public float WeekNumberColumnWidth = 0f;

        [Header("Appearance")]
        [Tooltip("If this value is set, then the values provided here will override the values of the template. If you wish to modify the template directly, this value should be cleared.")]
        public bool OverrideTemplate = true;

        [Header("Text")]
        public Color TextColor;
        public Font Font;
        public int FontSize;

        [Header("Background")]
        public Sprite BackgroundImage;
        public Color BackgroundColor;        

        public void ApplyConfig(DatePicker_DayHeader header)
        {
            if (!OverrideTemplate) return;

            header.HeaderText.color = TextColor;
            header.HeaderText.font = Font;
            header.HeaderText.fontSize = FontSize;
            header.Cell.image.sprite = BackgroundImage;
            header.Cell.image.color = BackgroundColor;            
        }        
    }

    [Serializable]
    public class DatePickerButtonConfig
    {
        public Sprite Image;
        public ColorBlock Colors;        

        public void ApplyConfig(DatePicker_Button button)
        {                        
            //button.Button.colors = Colors;
            button.Button.colors.Apply(Colors);
            button.Button.image.sprite = Image;
        }
    }

    [Serializable]
    public class DatePickerHeaderConfig
    {
        public bool ShowHeader = true;

        [Header("Background")]
        public Color BackgroundColor;

        [Header("Text")]
        public Color TextColor;
        public Font Font;        

        [Header("Buttons")]
        public bool ShowNextAndPreviousMonthButtons = true;
        public bool ShowNextAndPreviousYearButtons = true;

        [Header("Height")]
        public float Height = 48f;

        public DatePickerButtonConfig PreviousMonthButton;
        public DatePickerButtonConfig NextMonthButton;
        public DatePickerButtonConfig PreviousYearButton;
        public DatePickerButtonConfig NextYearButton;

        public void Apply(DatePicker_Header header)
        {
            if (header == null) return;

            if (ShowHeader)
            {
                header.gameObject.SetActive(true);
            }
            else
            {
                header.gameObject.SetActive(false);                
                return;
            }

            header.Background.color = BackgroundColor;

            header.HeaderText.color = TextColor;
            header.HeaderText.font = Font;            

            if (ShowNextAndPreviousMonthButtons)
            {
                header.NextMonthButton.gameObject.SetActive(true);
                header.PreviousMonthButton.gameObject.SetActive(true);

                NextMonthButton.ApplyConfig(header.NextMonthButton);
                PreviousMonthButton.ApplyConfig(header.PreviousMonthButton);
            }
            else
            {
                header.NextMonthButton.gameObject.SetActive(false);
                header.PreviousMonthButton.gameObject.SetActive(false);
            }

            if (ShowNextAndPreviousYearButtons)
            {
                header.NextYearButton.gameObject.SetActive(true);
                header.PreviousYearButton.gameObject.SetActive(true);

                NextYearButton.ApplyConfig(header.NextYearButton);
                PreviousYearButton.ApplyConfig(header.PreviousYearButton);
            }
            else
            {
                header.NextYearButton.gameObject.SetActive(false);
                header.PreviousYearButton.gameObject.SetActive(false);
            }

            header.Row.preferredHeight = Height;
        }
    }

    [Serializable]
    public struct DatePickerEventConfig
    {
        [SerializeField]
        public DatePickerEvent OnDaySelected;
        [SerializeField]
        public DatePickerEvent OnDayMouseOver;
    }

    [Serializable]
    public class DatePickerMiscConfig
    {
        [Tooltip("If this is set, the DatePicker will always switch to the selected month when a new date is selected.")]
        public bool SwitchToSelectedMonthWhenDateSelected = true;

        public bool ShowDatesInOtherMonths = true;

        [Tooltip("If this is set, then the DatePicker will be closed when a date is selected. Useful for popup datepicker dialogs.")]
        public bool CloseWhenDateSelected = false;

        public DatePickerMiscConfig Clone()
        {
            return new DatePickerMiscConfig
            {
                SwitchToSelectedMonthWhenDateSelected = SwitchToSelectedMonthWhenDateSelected,
                ShowDatesInOtherMonths = ShowDatesInOtherMonths,
                CloseWhenDateSelected = CloseWhenDateSelected
            };
        }
    }

    [Serializable]
    public class DatePickerFormatConfig
    {
        public string DateFormat = DatePickerUtilities.DateFormat;
    }

    [Serializable]
    public class DatePickerModalConfig
    {
        [Tooltip("If this is set to true, then the DatePicker will be the only thing on the screen which can be interacted with when it is active.")]
        public bool IsModal = false;

        [Tooltip("If this is set to true, then the DatePicker will be closed when the screen overlay is clicked. Relevant to Modal DatePickers only.")]
        public bool CloseWhenModalOverlayClicked = true;

        public Color ScreenOverlayColor;
    }

    [Serializable]
    public class DatePickerDateRangeConfig
    {
        public bool RestrictFromDate = false;
        public SerializableDate FromDate;
        public bool RestrictToDate = false;
        public SerializableDate ToDate;

        public bool Validate(bool silent = false)
        {
            if (RestrictFromDate && !FromDate.HasValue)
            {
                if(!silent) Debug.Log("[DatePicker] Restrict From Date requires a 'From Date' to be specified.");
                return false;
            }

            if (RestrictToDate && !ToDate.HasValue)
            {
                if(!silent) Debug.Log("[DatePicker] Restrict To Date requires a 'To Date' to be specified.");
                return false;
            }

            if (RestrictFromDate && RestrictToDate)
            {
                if (ToDate.Date.CompareTo(FromDate.Date) < 0)
                {
                    if(!silent) Debug.Log("[DatePicker] Invalid Date range specified.");
                    return false;
                }
            }

            return true;            
        }

        public DatePickerDateRangeConfig Clone()
        {
            return new DatePickerDateRangeConfig
            {
                RestrictFromDate = RestrictFromDate,
                RestrictToDate = RestrictToDate,
                FromDate = new SerializableDate(FromDate),
                ToDate = new SerializableDate(ToDate)
            };
        }
    }

    [Serializable]
    public class DatePickerInputFieldConfig
    {
        public bool ToggleDisplayWhenInputFieldClicked = true;
        public bool ShowToggleButton = true;
        public float ToggleButtonWidth = 64f;

        

        public Dates.Alignment DatePickerAlignmentRelativeToInputField = Dates.Alignment.Left;
    }

    [Serializable]
    public class DatePickerAnimationConfig
    {
        public Animation ShowAnimation = Animation.None;
        public Animation HideAnimation = Animation.None;

        public Animation MonthChangedAnimation = Animation.None;
    }
}
