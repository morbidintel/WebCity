using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;

namespace UI.Dates
{
    [ExecuteInEditMode]
    public class DatePicker : MonoBehaviour
    {
        #region Dates
        [SerializeField]
        private DateSelectionMode m_DateSelectionMode = DateSelectionMode.SingleDate;
        public DateSelectionMode DateSelectionMode
        {
            get { return m_DateSelectionMode; }
            set
            {
                SetProperty(ref m_DateSelectionMode, value);
            }
        }

        [SerializeField]
        private SerializableDate m_SelectedDate;
        public SerializableDate SelectedDate 
        { 
            get { return m_SelectedDate; } 
            set 
            {                
                SetProperty(ref m_SelectedDate, value);
                
                // This will update the VisibleDate field to match the selected date, ensuring that the currently visible month always matches the selected date
                // (when the date is selected)
                // This is only relevant when selecting dates that don't fall within the current month
                if (Config.Misc.SwitchToSelectedMonthWhenDateSelected)
                {
                    VisibleDate = value;
                }

                UpdateInputFieldText();
                if (Config.Misc.CloseWhenDateSelected) Hide();
            } 
        }

        [SerializeField]
        private List<SerializableDate> m_SelectedDates = new List<SerializableDate>();
        public List<SerializableDate> SelectedDates
        {
            get { return m_SelectedDates; }
            set
            {
                SetProperty(ref m_SelectedDates, value);
            }
        }

        [SerializeField]
        private SerializableDate m_VisibleDate;        
        public SerializableDate VisibleDate 
        { 
            get 
            {
                if (!m_VisibleDate.HasValue)
                {
                    if (SelectedDate.HasValue)
                    {                        
                        m_VisibleDate = new SerializableDate(SelectedDate.Date);
                    }
                    else
                    {                     
                        m_VisibleDate = new SerializableDate(DateTime.Today);
                    }
                }

                return m_VisibleDate; 
            } 
            set { SetProperty(ref m_VisibleDate, value); } 
        }

        [Tooltip("Defines how 'VisibleDate' is calculated, if at all. Only used if Selected Date has no value.")]
        public VisibleDateDefaultBehaviour VisibleDateDefaultBehaviour = VisibleDateDefaultBehaviour.UseTodaysDate;

        public bool IsSharedCalendar { get; set; }
        #endregion

        #region Config
        public DatePickerConfig Config;                
        #endregion

        #region References
        [Header("References")]
        public RectTransform Ref_DatePickerTransform;
        public DatePicker_Header Ref_Header;

        public TableLayout Ref_DayTable;
        public DatePicker_Animator Ref_DayTableAnimator;
        public TableCell Ref_DayTableContainer;
        
        public DatePicker_DayHeader Ref_Template_DayName;
        public DatePicker_DayButton Ref_Template_Day_CurrentMonth;
        public DatePicker_DayButton Ref_Template_Day_OtherMonths;
        public DatePicker_DayButton Ref_Template_Day_Today;
        public DatePicker_DayButton Ref_Template_Day_SelectedDay;

        public Image Ref_Border;
        public DatePicker_ContentLayout Ref_ContentLayout;

        public Image Ref_ScreenOverlay;
        public DatePicker_Animator Ref_ScreenOverlayAnimator;

        public DatePicker_Animator Ref_Animator;

        // Optional
        public InputField Ref_InputField;
        public TableLayout Ref_InputFieldContainer;
        public TableCell Ref_InputFieldToggleButtonCell;

        public DatePicker_DateRange Ref_DatePicker_DateRange;
        #endregion        

        [SerializeField]
        private DatePicker_DayButton_Pool _buttonPool;
        private DatePicker_DayButton_Pool buttonPool
        {
            get
            {
                if (_buttonPool == null) _buttonPool = GetComponent<DatePicker_DayButton_Pool>();
                if (_buttonPool == null) _buttonPool = gameObject.AddComponent<DatePicker_DayButton_Pool>();

                return _buttonPool;
            }
        }

        public bool IsVisible
        {
            get
            {
                return Ref_DatePickerTransform.gameObject.activeInHierarchy;
            }
        }

        void Awake()
        {
            if (!SelectedDate.HasValue)
            {
                if(VisibleDateDefaultBehaviour == Dates.VisibleDateDefaultBehaviour.UseTodaysDate)
                {                
                    VisibleDate = DateTime.Today;                
                }
            }
        }
        
        void Start()
        {            
            //UpdateDisplay();
            DatePickerTimer.DelayedCall(0, UpdateDisplay, this);            
        }

        public void ShowPreviousMonth()
        {
            VisibleDate = VisibleDate.Date.AddMonths(-1);
            MonthChangedUpdateDisplay();  
        }

        public void ShowNextMonth()
        {            
            VisibleDate = VisibleDate.Date.AddMonths(1);
            MonthChangedUpdateDisplay();                       
        }

        public void ShowPreviousYear()
        {
            VisibleDate = VisibleDate.Date.AddYears(-1);
            MonthChangedUpdateDisplay();
        }

        public void ShowNextYear()
        {
            VisibleDate = VisibleDate.Date.AddYears(1);
            MonthChangedUpdateDisplay();
        }

        void MonthChangedUpdateDisplay()
        {
            if (Config.Animation.MonthChangedAnimation == Animation.None)
            {
                UpdateDisplay();
                return;
            }

            Ref_DayTableAnimator.PlayAnimation(Config.Animation.MonthChangedAnimation,
                                               AnimationType.Hide,
                                               () =>
                                               {
                                                   UpdateDisplay();

                                                   Ref_DayTableAnimator.PlayAnimation(Config.Animation.MonthChangedAnimation, AnimationType.Show);
                                               }); 
        }

        public void UpdateDisplay()
        {            
            // don't do anything if we aren't actually active in the hierarchy
            // (basically, we're either inactive or a prefab)
            if (!this.gameObject.activeInHierarchy) return;

            if (Config.Sizing.OverrideTransformHeight)
            {
                Ref_DatePickerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Config.Sizing.PreferredHeight);
            }

            UpdateBorder();
            UpdateHeader();
            UpdateWeekDayHeaders();
            UpdateDaySection();

            // Free all buttons in the pool
            buttonPool.FreeAll();

            // Clear existing data
            Ref_DayTable.ClearRows();

            // Day Names
            var dayNameRow = Ref_DayTable.AddRow(0);
            dayNameRow.dontUseTableRowBackground = true;

            // Empty cell if we're showing the Week Numbers column
            if (Config.WeekDays.ShowWeekNumbers)
            {
                var header = Instantiate(Ref_Template_DayName);
                dayNameRow.AddCell(header.Cell);

                Ref_DayTable.ColumnWidths[0] = Config.WeekDays.WeekNumberColumnWidth;
            }
            else
            {
                Ref_DayTable.ColumnWidths[0] = 0;
            }

            var dayNames = DatePickerUtilities.GetAbbreviatedDayNames();
            foreach (var dayName in dayNames)
            {
                var header = Instantiate(Ref_Template_DayName);
                dayNameRow.AddCell(header.Cell);

                header.HeaderText.text = dayName;
            }

            // Validate our Date Range (if necessary. This method will output an error message if we fail)
            bool dateRangeValid = Config.DateRange.Validate();

            // Dates
            var days = DatePickerUtilities.GetDateRangeForDisplay(VisibleDate.Date);
            TableRow row = null;
            int weekNumber = 1;

            DateTimeFormatInfo currentDateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;            

            foreach (var day in days)
            {
                if (day.DayOfWeek == DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                {
                    row = Ref_DayTable.AddRow(0);

                    if (Config.WeekDays.ShowWeekNumbers)
                    {
                        if (Config.WeekDays.WeekNumberMode == WeekNumberMode.WeekOfYear)
                        {                            
                            weekNumber = currentDateTimeFormatInfo.Calendar.GetWeekOfYear(day, Config.WeekDays.CalendarWeekRule, currentDateTimeFormatInfo.FirstDayOfWeek);
                        }

                        var weekNumberCell = Instantiate(Ref_Template_DayName);
                        row.AddCell(weekNumberCell.Cell);

                        weekNumberCell.HeaderText.text = weekNumber.ToString();

                        weekNumber++;
                    }
                }

                if (!Config.Misc.ShowDatesInOtherMonths && !DatePickerUtilities.DateFallsWithinMonth(day, VisibleDate))
                {
                    // add an empty cell
                    row.AddCell();
                }
                else
                {
                    var dayType = GetDayTypeForDate(day);
                    var dayItem = buttonPool.GetButton(dayType);
                    
                    //var dayItem = Instantiate(GetDayTemplateForDate(day));
                    row.AddCell(dayItem.Cell);

                    dayItem.Text.text = day.Day.ToString();
                    dayItem.DatePicker = this;
                    dayItem.Date = day;
                    dayItem.name = day.ToDateString();
                    dayItem.IsTemplate = false;
                    dayItem.Button.interactable = true;

                    if (dateRangeValid) // if the date range is not valid, then don't attempt to use it
                    {
                        if ((Config.DateRange.RestrictFromDate && day.CompareTo(Config.DateRange.FromDate) < 0)
                         || (Config.DateRange.RestrictToDate && day.CompareTo(Config.DateRange.ToDate) > 0))
                        {
                            dayItem.Button.interactable = false;
                        }                        
                    }
                }
            }
            
            if (Ref_InputField != null && Ref_InputFieldContainer != null && Ref_InputFieldToggleButtonCell != null)
            {
                Ref_InputField.text = SelectedDate.HasValue ? SelectedDate.Date.ToString(Config.Format.DateFormat) : "";
                if(Ref_ScreenOverlay != null) Ref_ScreenOverlay.color = Config.Modal.ScreenOverlayColor;

                var valueBefore = Ref_InputFieldContainer.ColumnWidths.ToList();

                if (Config.InputField.ShowToggleButton)
                {
                    Ref_InputFieldContainer.ColumnWidths = new List<float> { 0, Config.InputField.ToggleButtonWidth };
                    Ref_InputFieldToggleButtonCell.gameObject.SetActive(true);
                }
                else
                {
                    Ref_InputFieldContainer.ColumnWidths = new List<float> { 0 };
                    Ref_InputFieldToggleButtonCell.gameObject.SetActive(false);
                }

                if(!valueBefore.SequenceEqual(Ref_InputFieldContainer.ColumnWidths)) Ref_InputFieldContainer.UpdateLayout();
            }            
        }

        void UpdateBorder()
        {
            // Border size / color
            Ref_ContentLayout.SetBorderSize(Config.Border.Size);
            Ref_Border.color = Config.Border.Color;
        }

        void UpdateHeader()
        {
            // Update month name
            Ref_Header.HeaderText.text = VisibleDate.Date.ToString("MMM yyyy");

            Config.Header.Apply(Ref_Header);

            var dateRangeValid = Config.DateRange.Validate(true);

            if (dateRangeValid && Config.DateRange.RestrictFromDate)
            {
                var lastDayOfPreviousMonth = VisibleDate.Date.AddMonths(-1);
                lastDayOfPreviousMonth = new DateTime(lastDayOfPreviousMonth.Year, lastDayOfPreviousMonth.Month, DateTime.DaysInMonth(lastDayOfPreviousMonth.Year, lastDayOfPreviousMonth.Month)).AddDays(1).AddTicks(-1);

                Ref_Header.PreviousMonthButton.Button.interactable = (lastDayOfPreviousMonth.CompareTo(Config.DateRange.FromDate) >= 0);

                var lastDayOfPreviousYear = VisibleDate.Date.AddYears(-1);
                lastDayOfPreviousYear = new DateTime(lastDayOfPreviousYear.Year, 12, 31);

                Ref_Header.PreviousYearButton.Button.interactable = (lastDayOfPreviousYear.CompareTo(Config.DateRange.FromDate) >= 0);
            }
            else
            {
                Ref_Header.PreviousMonthButton.Button.interactable = true;
            }

            if (dateRangeValid && Config.DateRange.RestrictToDate)
            {
                var firstDayOfNextMonth = VisibleDate.Date.AddMonths(1);
                firstDayOfNextMonth = new DateTime(firstDayOfNextMonth.Year, firstDayOfNextMonth.Month, 1);                                

                Ref_Header.NextMonthButton.Button.interactable = (firstDayOfNextMonth.CompareTo(Config.DateRange.ToDate) <= 0);

                var firstDayOfNextYear = VisibleDate.Date.AddYears(1);
                firstDayOfNextYear = new DateTime(firstDayOfNextYear.Year, 1, 1);

                Ref_Header.NextYearButton.Button.interactable = (firstDayOfNextYear.CompareTo(Config.DateRange.ToDate) <= 0);
            }
            else
            {
                Ref_Header.NextMonthButton.Button.interactable = true;
            }
        }

        void UpdateWeekDayHeaders()
        {
            Config.WeekDays.ApplyConfig(Ref_Template_DayName);
        }

        void UpdateDaySection()
        {
            var templateList = new List<DatePicker_Button>() 
            { 
                Ref_Template_Day_Today,
                Ref_Template_Day_SelectedDay,
                Ref_Template_Day_CurrentMonth,
                Ref_Template_Day_OtherMonths
            };

            foreach (var template in templateList)
            {
                template.IsTemplate = true; // just in case
                template.Text.font = Config.Days.Font;
                template.Text.fontSize = Config.Days.FontSize;                
            }            

            Config.Days.Today.ApplyConfig(Ref_Template_Day_Today);
            Config.Days.SelectedDay.ApplyConfig(Ref_Template_Day_SelectedDay);
            Config.Days.OtherMonths.ApplyConfig(Ref_Template_Day_OtherMonths);
            Config.Days.CurrentMonth.ApplyConfig(Ref_Template_Day_CurrentMonth);

            Ref_DayTable.RowBackgroundColor = Config.Days.BackgroundColor;            
            Ref_DayTableContainer.image.color = Config.Days.BackgroundColor;
        }

        public void InvalidateAllDayButtonTemplates()
        {
            buttonPool.InvalidateAll();
            UpdateDisplay();
        }

        public void InvalidateDayButtonTemplate(DatePickerDayButtonType type)
        {
            buttonPool.InvalidateType(type);
            UpdateDisplay();
        }

        /*private DatePicker_DayButton GetDayTemplateForDate(DateTime date)
        {
            DatePicker_DayButton dayTemplate = null;

            if ((DateSelectionMode == Dates.DateSelectionMode.SingleDate && SelectedDate.HasValue && date.Equals(SelectedDate.Date))
             || (DateSelectionMode == Dates.DateSelectionMode.MultipleDates && SelectedDates.Contains(date)))
            {
                dayTemplate = Ref_Template_Day_SelectedDay;
            }
            else if (date.Equals(DateTime.Today))
            {
                dayTemplate = Ref_Template_Day_Today;
            }
            else if (date.Month == VisibleDate.Date.Month)
            {
                dayTemplate = Ref_Template_Day_CurrentMonth;
            }
            else
            {
                dayTemplate = Ref_Template_Day_OtherMonths;
            }

            return dayTemplate;
        }*/

        private DatePickerDayButtonType GetDayTypeForDate(DateTime date)
        {
            DatePickerDayButtonType type;

            if ((DateSelectionMode == Dates.DateSelectionMode.SingleDate && SelectedDate.HasValue && date.Equals(SelectedDate.Date))
             || (DateSelectionMode == Dates.DateSelectionMode.MultipleDates && SelectedDates.Contains(date)))
            {
                type = DatePickerDayButtonType.SelectedDay;
            }
            else if (date.Equals(DateTime.Today))
            {
                type = DatePickerDayButtonType.Today;
            }
            else if (date.Month == VisibleDate.Date.Month)
            {
                type = DatePickerDayButtonType.CurrentMonth;
            }
            else
            {
                type = DatePickerDayButtonType.OtherMonths;
            }

            return type;
        }

        /// <summary>
        /// Called by DayButton
        /// </summary>
        /// <param name="date"></param>
        public void DayButtonClicked(DateTime date)
        {            
            if (DateSelectionMode == Dates.DateSelectionMode.SingleDate)
            {
                SelectedDate = date;
            }
            else
            {
                if (SelectedDates.Any(d => d == date))
                {                    
                    SelectedDates.Remove(date);
                }
                else
                {                 
                    SelectedDates.Add(date);
                }
            }

            if (Ref_DatePicker_DateRange != null)
            {
                Ref_DatePicker_DateRange.DateSelected(date);
            }

            if (Config.Events.OnDaySelected != null)
            {
                Config.Events.OnDaySelected.Invoke(date);
            }

            UpdateDisplay();

            // I would have preferred to have this react automatically to changes,
            // but that would mean setting up an observable list, which is an added
            // complication we don't need right now
            UpdateInputFieldText();
        }

        public void UpdateInputFieldText()
        {
            if (Ref_InputField != null)
            {
                switch (DateSelectionMode)
                {
                    case Dates.DateSelectionMode.SingleDate:
                        Ref_InputField.text = (SelectedDate.HasValue) ? SelectedDate.Date.ToString(Config.Format.DateFormat) : "";
                        break;
                    case Dates.DateSelectionMode.MultipleDates:
                        var valueCount = SelectedDates.Count(s => s.HasValue);
                        Ref_InputField.text = ((valueCount == 1) ? SelectedDates.First(s => s.HasValue).Date.ToString(Config.Format.DateFormat)
                                                         : (valueCount > 1 ? "Multiple Dates" : ""));
                        break;
                } 
            }
        }

        /// <summary>
        /// Called by DayButton
        /// </summary>
        /// <param name="date"></param>
        public void DayButtonMouseOver(DateTime date)
        {
            if (Config.Events.OnDayMouseOver != null)
            {
                Config.Events.OnDayMouseOver.Invoke(date);
            }
        }

        /// <summary>
        /// Called by the screen overlay when it is clicked
        /// </summary>
        public void ModalOverlayClicked()
        {
            if (Ref_DatePicker_DateRange != null)
            {
                Ref_DatePicker_DateRange.ModalOverlayClicked();
            }
            else
            {
                if (Config.Modal.CloseWhenModalOverlayClicked) Hide();
            }
        }

        public void InputFieldClicked()
        {
            if (Config.InputField.ToggleDisplayWhenInputFieldClicked) ToggleDisplay();
        }

        public void ToggleDisplay()
        {                        
            if (Ref_DatePickerTransform.gameObject.activeInHierarchy)
            {
                Hide();
            }
            else
            {                
                Show();
            }
        }

        public void Show(bool setPositionIfNecessary = true)
        {
            var canvas = FindParentOfType<Canvas>(this.gameObject);

            if (setPositionIfNecessary && Ref_InputField != null)
            {
                // Position tablelayout relative to InputField
                SetPositionAdjacentToInputFieldContainer();
                // Wait till the end of the frame, then complete Show() (this ensures that the DatePicker only becomes visible after being resized)
                DatePickerTimer.DelayedCall(0, () => Show(false), this);
                return;
            }

            Ref_DatePickerTransform.gameObject.SetActive(true);            

            if (Config.Modal.IsModal && Ref_ScreenOverlay != null)
            {
                if (canvas != null)
                {
                    Ref_ScreenOverlay.transform.SetParent(canvas.transform);
                    Ref_ScreenOverlay.transform.SetAsLastSibling();
                }

                Ref_ScreenOverlay.gameObject.SetActive(true);

                Ref_ScreenOverlayAnimator.PlayAnimation(Animation.Fade, AnimationType.Show);
            }
            
            if (canvas != null)
            {
                Ref_DatePickerTransform.SetParent(canvas.transform);
                Ref_DatePickerTransform.SetAsLastSibling();
            }            

            if (Config.Animation.ShowAnimation != Animation.None)
            {
                PlayAnimation(Config.Animation.ShowAnimation, AnimationType.Show);
            }
        }

        private void PlayAnimation(Animation animation, AnimationType animationType, Action onComplete = null)
        {
            Ref_Animator.PlayAnimation(animation, animationType, onComplete);            
        }        

        public void Hide()
        {
            if (Config.Animation.HideAnimation != Animation.None)
            {
                PlayAnimation(Config.Animation.HideAnimation, AnimationType.Hide, _Hide);
            }
            else
            {
                _Hide();
            }            
        }

        private void _Hide()
        {
            if (Config.Modal.IsModal)
            {
                if(Ref_ScreenOverlay != null) Ref_ScreenOverlayAnimator.PlayAnimation(Animation.Fade, AnimationType.Hide, HideScreenOverlay_Complete);
            }

            if (this.transform != Ref_DatePickerTransform)
            {
                Ref_DatePickerTransform.SetParent(this.transform);                
            }

            Ref_DatePickerTransform.gameObject.SetActive(false);
        }

        private void HideScreenOverlay_Complete()
        {
            Ref_ScreenOverlay.transform.SetParent(this.transform);            
            Ref_ScreenOverlay.gameObject.SetActive(false);
        }

        private void SetPositionAdjacentToInputFieldContainer()
        {            
            if (Ref_InputFieldContainer == null) return;            

            var rectTransform = Ref_DatePickerTransform;
            var inputFieldRectTransform = Ref_InputFieldContainer.transform as RectTransform;
            var inputFieldWidth = inputFieldRectTransform.rect.width;

            if (IsSharedCalendar)
            {
                rectTransform.SetParent(inputFieldRectTransform.parent);
            }

            // Fix anchors:
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);

            var widthBefore = rectTransform.rect.width;
            if (Config.Sizing.UsePreferredWidthInsteadOfInputFieldWidth)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Config.Sizing.PreferredWidth);
            }
            else
            {                
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inputFieldWidth);
            }

            if (widthBefore != rectTransform.rect.width)
            {
                ((RectTransform)Ref_DayTable.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
            }                      
            
            var pivotX = 0.5f;            
            switch (Config.InputField.DatePickerAlignmentRelativeToInputField)
            {
                case Alignment.Left:
                    {
                        pivotX = 0f;                        
                    }
                    break;
                case Alignment.Right:
                    {
                        pivotX = 1f;                        
                    }
                    break;
            }

            var canvas = GetComponentInParent<Canvas>();
            var canvasTransform = ((RectTransform)canvas.transform);
                       
            rectTransform.pivot = new Vector2(pivotX, 0.5f);
            rectTransform.anchoredPosition = inputFieldRectTransform.anchoredPosition;
            rectTransform.SetParent(canvasTransform);

            rectTransform.pivot = new Vector2(pivotX, 1);
            rectTransform.anchoredPosition -= new Vector2(0, inputFieldRectTransform.rect.height);
                                
            var spaceBelow = canvasTransform.rect.height + rectTransform.anchoredPosition.y;

            if (spaceBelow < rectTransform.rect.height)
            {
                rectTransform.pivot = new Vector2(pivotX, 0);
                rectTransform.anchoredPosition += new Vector2(0, inputFieldRectTransform.rect.height);

                var spaceAbove = -(rectTransform.anchoredPosition.y + rectTransform.rect.height);
                if (spaceAbove < 0)
                {
                    rectTransform.anchoredPosition += new Vector2(0, spaceAbove);
                }
            }                            

            DatePickerTimer.DelayedCall(0.05f, () => { Ref_DayTableContainer.GetRow().NotifyTableRowPropertiesChanged(); }, this);            
        }

        private static T FindParentOfType<T>(GameObject childObject)
        where T : UnityEngine.Object
        {
            Transform t = childObject.transform;
            while (t.parent != null)
            {
                var component = t.parent.GetComponent<T>();

                if (component != null) return component;

                t = t.parent.transform;
            }

            // We didn't find anything
            return null;
        }

        #region SetProperty
        protected void SetProperty<T>(ref T currentValue, T newValue)
        {   
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;

            currentValue = newValue;

            UpdateDisplay();
        }

        void OnRectTransformDimensionsChange()
        {                        
            //UpdateDisplay();
            DatePickerTimer.DelayedCall(0f, UpdateDisplay, this);
        }
        #endregion        
    }        
}
