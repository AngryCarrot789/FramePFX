using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using FramePFX.Commands;
using FramePFX.Utils;

namespace FramePFX.WPF.Controls.Dragger {
    [TemplatePart(Name = nameof(PART_HintTextBlock), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(PART_TextBlock), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(PART_TextBox), Type = typeof(TextBox))]
    public class NumberDragger : RangeBase {
        private static readonly object ValueModStateBeginBox = ValueModState.Begin;
        private static readonly object ValueModStateFinishBox = ValueModState.Finish;
        private static readonly object ValueModStateCancelledBox = ValueModState.Cancelled;

        #region Dependency Properties

        public static readonly DependencyProperty TinyChangeProperty = DependencyProperty.Register("TinyChange", typeof(double), typeof(NumberDragger), new PropertyMetadata(0.001d));
        public static readonly DependencyProperty MassiveChangeProperty = DependencyProperty.Register("MassiveChange", typeof(double), typeof(NumberDragger), new PropertyMetadata(5d));
        protected static readonly DependencyPropertyKey IsDraggingPropertyKey =DependencyProperty.RegisterReadOnly("IsDragging",typeof(bool),typeof(NumberDragger),new PropertyMetadata(BoolBox.False, (d, e) => ((NumberDragger) d).OnIsDraggingChanged((bool) e.OldValue, (bool) e.NewValue)));
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;
        public static readonly DependencyProperty CompleteEditOnTextBoxLostFocusProperty =DependencyProperty.Register("CompleteEditOnTextBoxLostFocus",typeof(bool?),typeof(NumberDragger),new PropertyMetadata(BoolBox.True));
        public static readonly DependencyProperty OrientationProperty =DependencyProperty.Register(    "Orientation",    typeof(Orientation),    typeof(NumberDragger),    new PropertyMetadata(Orientation.Horizontal, (d, e) => ((NumberDragger) d).OnOrientationChanged((Orientation) e.OldValue, (Orientation) e.NewValue)));
        public static readonly DependencyProperty HorizontalIncrementProperty =DependencyProperty.Register(    "HorizontalIncrement",    typeof(HorizontalIncrement),    typeof(NumberDragger),    new PropertyMetadata(HorizontalIncrement.LeftDecrRightIncr));
        public static readonly DependencyProperty VerticalIncrementProperty = DependencyProperty.Register("VerticalIncrement", typeof(VerticalIncrement), typeof(NumberDragger), new PropertyMetadata(VerticalIncrement.UpDecrDownIncr));
        public static readonly DependencyPropertyKey IsEditingTextBoxPropertyKey = DependencyProperty.RegisterReadOnly("IsEditingTextBox", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.False, (d, e) => ((NumberDragger) d).OnIsEditingTextBoxChanged((bool) e.OldValue, (bool) e.NewValue), (d, v) => ((NumberDragger) d).OnCoerceIsEditingTextBox(v)));
        public static readonly DependencyProperty IsEditingTextBoxProperty = IsEditingTextBoxPropertyKey.DependencyProperty;
        public static readonly DependencyProperty RoundedPlacesProperty = DependencyProperty.Register("RoundedPlaces", typeof(int?), typeof(NumberDragger), new PropertyMetadata(null, (d, e) => ((NumberDragger) d).OnRoundedPlacesChanged((int?) e.OldValue, (int?) e.NewValue)));
        public static readonly DependencyProperty PreviewRoundedPlacesProperty = DependencyProperty.Register("PreviewRoundedPlaces", typeof(int?), typeof(NumberDragger), new PropertyMetadata((int?) 4, (d, e) => ((NumberDragger) d).OnPreviewRoundedPlacesChanged((int?) e.OldValue, (int?) e.NewValue)));
        public static readonly DependencyProperty LockCursorWhileDraggingProperty = DependencyProperty.Register("LockCursorWhileDragging", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.True));
        public static readonly DependencyProperty DisplayTextOverrideProperty = DependencyProperty.Register("DisplayTextOverride", typeof(string), typeof(NumberDragger), new PropertyMetadata(null, (o, args) => ((NumberDragger) o).UpdateText()));
        public static readonly DependencyProperty EditingHintProperty = DependencyProperty.Register("EditingHint", typeof(string), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty ForcedReadOnlyStateProperty = DependencyProperty.Register("ForcedReadOnlyState", typeof(bool?), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty RestoreValueOnCancelProperty = DependencyProperty.Register("RestoreValueOnCancel", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.True));
        public static readonly DependencyProperty ChangeMapperProperty = DependencyProperty.Register("ChangeMapper", typeof(IChangeMapper), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty ValuePreProcessorProperty = DependencyProperty.Register("ValuePreProcessor", typeof(IValuePreProcessor), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty EditStateChangedCommandProperty = DependencyProperty.Register("EditStateChangedCommand", typeof(ICommand), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty IsDoubleClickToEditProperty = DependencyProperty.Register("IsDoubleClickToEdit", typeof(bool), typeof(NumberDragger), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty ResetValueCommandProperty = DependencyProperty.Register("ResetValueCommand", typeof(ICommand), typeof(NumberDragger), new PropertyMetadata(null));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the tiny-change value. This is added or subtracted when CTRL + SHIFT is pressed
        /// </summary>
        public double TinyChange {
            get => (double) this.GetValue(TinyChangeProperty);
            set => this.SetValue(TinyChangeProperty, value);
        }

        /// <summary>
        /// Gets or sets the massive change value. This is added or subtracted when CTRL is pressed
        /// </summary>
        public double MassiveChange {
            get => (double) this.GetValue(MassiveChangeProperty);
            set => this.SetValue(MassiveChangeProperty, value);
        }

        public bool IsDragging {
            get => (bool) this.GetValue(IsDraggingProperty);
            protected set => this.SetValue(IsDraggingPropertyKey, value.Box());
        }

        public bool? CompleteEditOnTextBoxLostFocus {
            get => (bool?) this.GetValue(CompleteEditOnTextBoxLostFocusProperty);
            set => this.SetValue(CompleteEditOnTextBoxLostFocusProperty, value.BoxNullable());
        }

        public Orientation Orientation {
            get => (Orientation) this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public HorizontalIncrement HorizontalIncrement {
            get => (HorizontalIncrement) this.GetValue(HorizontalIncrementProperty);
            set => this.SetValue(HorizontalIncrementProperty, value);
        }

        public VerticalIncrement VerticalIncrement {
            get => (VerticalIncrement) this.GetValue(VerticalIncrementProperty);
            set => this.SetValue(VerticalIncrementProperty, value);
        }

        public bool IsEditingTextBox {
            get => (bool) this.GetValue(IsEditingTextBoxProperty);
            protected set => this.SetValue(IsEditingTextBoxPropertyKey, value.Box());
        }

        /// <summary>
        /// The number of digits to round the actual value to. Set to null to disable rounding.
        /// <para>
        /// When <see cref="RangeBase.ValueProperty"/> is bound to non floating point, this value should be ignored
        /// </para>
        /// <para>
        /// However when binding to floating point numbers, this value should ideally be 6 or 7. For doubles,
        /// this should be 14 or 15. This is to combat floating point rounding issues, causing the the
        /// </para>
        /// </summary>
        public int? RoundedPlaces {
            get => (int?) this.GetValue(RoundedPlacesProperty);
            set => this.SetValue(RoundedPlacesProperty, value);
        }

        /// <summary>
        /// The number of digits to round the preview value to. Set to null to disable rounding.
        /// <para>
        /// When <see cref="RangeBase.ValueProperty"/> is bound to non floating point, this value should be ignored
        /// </para>
        /// <para>
        /// However when binding to floating point numbers, this value should ideally be 6 or 7. For doubles,
        /// this should be 14 or 15. This is to combat floating point rounding issues, causing the the
        /// </para>
        /// </summary>
        public int? PreviewRoundedPlaces {
            get => (int?) this.GetValue(PreviewRoundedPlacesProperty);
            set => this.SetValue(PreviewRoundedPlacesProperty, value);
        }

        public bool LockCursorWhileDragging {
            get => (bool) this.GetValue(LockCursorWhileDraggingProperty);
            set => this.SetValue(LockCursorWhileDraggingProperty, value.Box());
        }

        /// <summary>
        /// Gets or sets a value that is displayed while the value preview is active, instead of displaying the
        /// actual value. A text box will still appear if the control is clicked
        /// <para>
        /// This is only displayed when the value is non-null and not an empty string
        /// </para>
        /// </summary>
        public string DisplayTextOverride {
            get => (string) this.GetValue(DisplayTextOverrideProperty);
            set => this.SetValue(DisplayTextOverrideProperty, value);
        }

        /// <summary>
        /// A piece of text to display overtop of the editor text box when manually editing
        /// the value, and there is no text in the text box
        /// </summary>
        public string EditingHint {
            get => (string) this.GetValue(EditingHintProperty);
            set => this.SetValue(EditingHintProperty, value);
        }

        public bool? ForcedReadOnlyState {
            get => (bool?) this.GetValue(ForcedReadOnlyStateProperty);
            set => this.SetValue(ForcedReadOnlyStateProperty, value.BoxNullable());
        }

        /// <summary>
        /// Whether or not to restore the value property when the drag is cancelled. Default is true
        /// </summary>
        public bool RestoreValueOnCancel {
            get => (bool) this.GetValue(RestoreValueOnCancelProperty);
            set => this.SetValue(RestoreValueOnCancelProperty, value.Box());
        }

        public IChangeMapper ChangeMapper {
            get => (IChangeMapper) this.GetValue(ChangeMapperProperty);
            set => this.SetValue(ChangeMapperProperty, value);
        }

        public IValuePreProcessor ValuePreProcessor {
            get => (IValuePreProcessor) this.GetValue(ValuePreProcessorProperty);
            set => this.SetValue(ValuePreProcessorProperty, value);
        }

        /// <summary>
        /// Gets or sets a command executed when the edit state changes. A <see cref="ValueModState"/> is passed as a parameter.
        /// <para>
        /// Supports async relay commands
        /// </para>
        /// </summary>
        public ICommand EditStateChangedCommand {
            get => (ICommand) this.GetValue(EditStateChangedCommandProperty);
            set => this.SetValue(EditStateChangedCommandProperty, value);
        }

        public bool IsDoubleClickToEdit {
            get => (bool) this.GetValue(IsDoubleClickToEditProperty);
            set => this.SetValue(IsDoubleClickToEditProperty, value.Box());
        }

        public ICommand ResetValueCommand {
            get => (ICommand) this.GetValue(ResetValueCommandProperty);
            set => this.SetValue(ResetValueCommandProperty, value);
        }

        public bool IsValueReadOnly {
            get {
                if (this.GetValue(ForcedReadOnlyStateProperty) is bool forced)
                    return forced;

                Binding binding;
                BindingExpression expression = this.GetBindingExpression(ValueProperty);
                if (expression == null || (binding = expression.ParentBinding) == null)
                    return false;

                switch (binding.Mode) {
                    case BindingMode.OneWay:
                    case BindingMode.OneTime:
                        return true;
                    default: return false;
                }
            }
        }

        #endregion

        public static readonly RoutedEvent EditStartedEvent = EventManager.RegisterRoutedEvent(nameof(EditStarted), RoutingStrategy.Bubble, typeof(EditStartEventHandler), typeof(NumberDragger));
        public static readonly RoutedEvent EditCompletedEvent = EventManager.RegisterRoutedEvent(nameof(EditCompleted), RoutingStrategy.Bubble, typeof(EditCompletedEventHandler), typeof(NumberDragger));

        [Category("Behavior")]
        public event EditStartEventHandler EditStarted {
            add => this.AddHandler(EditStartedEvent, value);
            remove => this.RemoveHandler(EditStartedEvent, value);
        }

        [Category("Behavior")]
        public event EditCompletedEventHandler EditCompleted {
            add => this.AddHandler(EditCompletedEvent, value);
            remove => this.RemoveHandler(EditCompletedEvent, value);
        }

        private TextBlock PART_HintTextBlock;
        private TextBlock PART_TextBlock;
        private TextBox PART_TextBox;
        private Point? lastClickPoint;
        private Point? lastMouseMove;
        private (int, int)? screenClip;
        private double? previousValue;
        private bool ignoreMouseMove;
        private bool isUpdatingExternalMouse;
        private bool ignoreLostFocus;
        private bool hasCancelled_ignoreMouseUp;

        public NumberDragger() {
            this.Loaded += (s, e) => {
                this.CoerceValue(IsEditingTextBoxProperty);
                this.UpdateText();
                this.UpdateCursor();
                this.RequeryChangeMapper(this.Value);
            };
        }

        static NumberDragger() {
            ValueProperty.OverrideMetadata(typeof(NumberDragger), new FrameworkPropertyMetadata(null, (o, value) => ((NumberDragger) o).OnCoerceValue(value)));
            Application.Current.Deactivated += OnApplicationFocusLost;
        }

        private static void OnApplicationFocusLost(object sender, EventArgs e) {
            if (Keyboard.FocusedElement is NumberDragger dragger) {
                if (dragger.IsDragging) {
                    dragger.CancelDrag();
                }
                else if (dragger.IsEditingTextBox) {
                    dragger.CancelInputEdit();
                }
            }
        }

        private object OnCoerceValue(object value) {
            double src = (double) value;
            double dst = Maths.Clamp(this.GetRoundedValue(src, false, out _), this.Minimum, this.Maximum);
            if (this.ValuePreProcessor is IValuePreProcessor processor) {
                dst = processor.Process(dst, this.Minimum, this.Maximum);
            }

            return Maths.Equals(dst, src, 0.00000000001d) ? dst : value;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_TextBlock = this.GetTemplateChild("PART_TextBlock") as TextBlock ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBlock));
            this.PART_TextBox = this.GetTemplateChild("PART_TextBox") as TextBox ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBox));
            this.PART_HintTextBlock = this.GetTemplateChild("PART_HintTextBlock") as TextBlock;
            this.PART_TextBox.Focusable = true;
            this.PART_TextBox.KeyDown += this.OnTextBoxKeyDown;
            this.PART_TextBox.LostFocus += (s, e) => {
                if (this.IsEditingTextBox && this.CompleteEditOnTextBoxLostFocus is bool complete) {
                    if (!complete || !this.TryCompleteEdit()) {
                        this.CancelInputEdit();
                    }
                }

                this.IsEditingTextBox = false;
            };

            this.PART_TextBox.TextChanged += (sender, e) => this.UpdateHintVisibility();

            this.CoerceValue(IsEditingTextBoxProperty);
        }

        public double GetRoundedValue(double value, bool isPreview, out int? places) {
            places = this.RoundedPlaces;
            if (places.HasValue) {
                value = Math.Round(value, places.Value);
            }

            if (isPreview) {
                int? preview = this.PreviewRoundedPlaces;
                if (preview.HasValue) {
                    value = Math.Round(value, preview.Value);
                    places = preview;
                }
            }

            return value;
        }

        protected virtual void OnIsDraggingChanged(bool oldValue, bool newValue) {

        }

        protected virtual void OnOrientationChanged(Orientation oldValue, Orientation newValue) {
            if (this.IsDragging) {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
        }

        protected virtual void OnIsEditingTextBoxChanged(bool oldValue, bool newValue) {
            if (newValue && this.IsDragging) {
                this.CancelDrag();
            }

            this.UpdatePreviewVisibilities();
            this.UpdateText();
            if (oldValue != newValue) {
                this.ignoreLostFocus = true;
                try {
                    this.PART_TextBox.Focus();
                    this.PART_TextBox.SelectAll();
                }
                finally {
                    this.ignoreLostFocus = false;
                }
            }

            this.UpdateCursor();
            this.UpdateHintVisibility();
        }

        private object OnCoerceIsEditingTextBox(object isEditing) {
            if (this.PART_TextBox == null || this.PART_TextBlock == null) {
                return isEditing;
            }

            this.UpdatePreviewVisibilities();
            return isEditing;
        }

        private void UpdateHintVisibility() {
            if (this.PART_HintTextBlock != null && this.PART_TextBox != null) {
                if (string.IsNullOrWhiteSpace(this.PART_TextBox.Text) && this.IsEditingTextBox && !string.IsNullOrEmpty(this.EditingHint)) {
                    this.PART_HintTextBlock.Visibility = Visibility.Visible;
                }
                else {
                    this.PART_HintTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UpdatePreviewVisibilities() {
            if (this.IsEditingTextBox) {
                this.PART_TextBox.Visibility = Visibility.Visible;
                this.PART_TextBlock.Visibility = Visibility.Hidden;
            }
            else {
                this.PART_TextBox.Visibility = Visibility.Hidden;
                this.PART_TextBlock.Visibility = Visibility.Visible;
            }

            this.PART_TextBox.IsReadOnly = this.IsValueReadOnly;
        }

        public void UpdateCursor() {
            if (this.IsValueReadOnly) {
                if (this.IsEditingTextBox) {
                    if (this.PART_TextBlock != null) {
                        this.PART_TextBlock.ClearValue(CursorProperty);
                    }
                    else {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }

                    this.ClearValue(CursorProperty);
                }
                else {
                    this.Cursor = Cursors.No;
                    if (this.PART_TextBlock != null) {
                        this.PART_TextBlock.Cursor = Cursors.No;
                    }
                    else {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }
                }
            }
            else {
                if (this.IsDragging) {
                    this.Cursor = this.LockCursorWhileDragging ? Cursors.None : this.GetCursorForOrientation();
                    if (this.PART_TextBlock != null) {
                        this.PART_TextBlock.ClearValue(CursorProperty);
                    }
                    else {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }
                }
                else {
                    if (this.IsEditingTextBox) {
                        if (this.PART_TextBlock != null) {
                            this.PART_TextBlock.ClearValue(CursorProperty);
                        }
                        else {
                            Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                        }

                        this.ClearValue(CursorProperty);
                    }
                    else {
                        Cursor cursor = this.GetCursorForOrientation();
                        this.Cursor = cursor;
                        if (this.PART_TextBlock != null) {
                            this.PART_TextBlock.Cursor = cursor;
                        }
                        else {
                            Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                        }
                    }
                }
            }
        }

        protected virtual void OnRoundedPlacesChanged(int? oldValue, int? newValue) {
            if (newValue != null)
                this.UpdateText();
        }

        protected virtual void OnPreviewRoundedPlacesChanged(int? oldValue, int? newValue) {
            if (newValue != null)
                this.UpdateText();
        }

        protected override void OnValueChanged(double oldValue, double newValue) {
            base.OnValueChanged(oldValue, newValue);
            this.UpdateText();
            this.RequeryChangeMapper(newValue);
        }

        private void RequeryChangeMapper(double value) {
            if (this.ChangeMapper is IChangeMapper mapper) {
                mapper.OnValueChanged(value, out double t, out double s, out double l, out double m);
                if (!this.TinyChange.Equals(t))
                    this.TinyChange = t;
                if (!this.SmallChange.Equals(s))
                    this.SmallChange = s;
                if (!this.LargeChange.Equals(l))
                    this.LargeChange = l;
                if (!this.MassiveChange.Equals(m))
                    this.MassiveChange = m;
            }
        }

        protected void UpdateText() {
            if (this.PART_TextBox == null && this.PART_TextBlock == null) {
                return;
            }

            if (this.IsEditingTextBox) {
                if (this.PART_TextBlock != null)
                    this.PART_TextBlock.Text = "";

                if (this.PART_TextBox == null)
                    return;
                // don't use preview for text box; only round to RoundedPlaces, if possible
                double value = this.GetRoundedValue(this.Value, false, out int? places);
                this.PART_TextBox.Text = (places.HasValue ? Math.Round(value, places.Value) : value).ToString();
            }
            else {
                // prevents problems where the text box could be very large due
                // to an un-rounded value, affecting the entire control size
                // 0.300000011920929 for example when it should be 0.3
                if (this.PART_TextBox != null)
                    this.PART_TextBox.Text = "";

                if (this.PART_TextBlock == null)
                    return;
                string text = this.DisplayTextOverride;
                if (string.IsNullOrEmpty(text)) {
                    double value = this.GetRoundedValue(this.Value, true, out int? places);
                    text = places.HasValue ? value.ToString("F" + places.Value.ToString()) : value.ToString();
                }

                this.PART_TextBlock.Text = text;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            if (e.LeftButton != MouseButtonState.Pressed && this.IsDragging) {
                this.CompleteDrag();
            }
        }

        private bool canActivateInputEdit;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            if (!this.IsDragging && !this.IsValueReadOnly) {
                e.Handled = true;
                this.Focus();

                this.lastMouseMove = this.lastClickPoint = e.GetPosition(this);
                if (!this.IsDoubleClickToEdit || e.ClickCount > 1) {
                    this.canActivateInputEdit = this.IsDoubleClickToEdit;
                    this.ignoreMouseMove = true;
                    try {
                        this.CaptureMouse();
                    }
                    finally {
                        this.ignoreMouseMove = false;
                    }

                    this.UpdateCursor();
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            e.Handled = true;
            if (this.IsDragging) {
                this.CompleteDrag();
            }
            else if (this.hasCancelled_ignoreMouseUp) {
                this.hasCancelled_ignoreMouseUp = false;
            }
            else if (this.canActivateInputEdit && this.IsMouseOver && !this.IsValueReadOnly) {
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                this.OnBeginInputEdit();
            }

            this.canActivateInputEdit = false;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove || this.isUpdatingExternalMouse) {
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed) {
                if (this.IsDragging) {
                    this.CompleteDrag();
                }

                return;
            }
            else if (!this.IsEnabled || this.IsValueReadOnly) { // saves a bit of performance by processing these here
                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape) && this.IsDragging) {
                this.CancelDrag();
                return;
            }

            if (!(this.lastMouseMove is Point lastPos)) {
                return;
            }

            bool wrap = false;
            Point mpos = e.GetPosition(this), wrapPoint = mpos;
            if (this.lastClickPoint is Point lastClick && !this.IsDragging) {
                if (Math.Abs(mpos.X - lastClick.X) < 5d && Math.Abs(mpos.Y - lastClick.Y) < 5d) {
                    return;
                }

                this.BeginMouseDrag();
            }

            if (!this.IsDragging) {
                return;
            }

            if (this.IsEditingTextBox) {
                Debug.WriteLine("IsEditingTextBox and IsDragging were both true");
                this.IsEditingTextBox = false;
            }

            if (this.LockCursorWhileDragging) {
                double x = mpos.X, y = mpos.Y;
                if (this.Orientation == Orientation.Horizontal) {
                    if (mpos.X < 0) {
                        x = this.ActualWidth;
                        wrap = true;
                    }
                    else if (mpos.X > this.ActualWidth) {
                        x = 0;
                        wrap = true;
                    }
                }
                else {
                    if (mpos.Y < 0) {
                        y = this.ActualHeight;
                        wrap = true;
                    }
                    else if (mpos.X > this.ActualHeight) {
                        y = 0;
                        wrap = true;
                    }
                }

                if (wrap) {
                    wrapPoint = new Point(x, y);
                }
            }

            double change;
            Orientation orientation = this.Orientation;
            switch (orientation) {
                case Orientation.Horizontal: {
                    change = mpos.X - lastPos.X;
                    break;
                }
                case Orientation.Vertical: {
                    change = mpos.Y - lastPos.Y;
                    break;
                }
                default: {
                    throw new Exception("Invalid orientation: " + orientation);
                }
            }

            bool isShiftDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool isCtrlDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (isShiftDown) {
                if (isCtrlDown) {
                    change *= this.TinyChange;
                }
                else {
                    change *= this.SmallChange;
                }
            }
            else if (isCtrlDown) {
                change *= this.MassiveChange;
            }
            else {
                change *= this.LargeChange;
            }

            double newValue;
            if ((orientation == Orientation.Horizontal && this.HorizontalIncrement == HorizontalIncrement.LeftDecrRightIncr) ||
                (orientation == Orientation.Vertical && this.VerticalIncrement == VerticalIncrement.UpDecrDownIncr)) {
                newValue = this.Value + change;
            }
            else {
                newValue = this.Value - change;
            }

            double roundedValue = Maths.Clamp(this.GetRoundedValue(newValue, false, out _), this.Minimum, this.Maximum);
            if (Maths.Equals(this.GetRoundedValue(this.Value, false, out _), roundedValue)) {
                return;
            }

            this.Value = roundedValue;
            this.lastMouseMove = mpos;

            if (wrap) {
                this.isUpdatingExternalMouse = true;
                try {
                    this.lastMouseMove = wrapPoint;
                    Point sp = this.PointToScreen(wrapPoint);
                    CursorUtils.SetCursorPos((int) sp.X, (int) sp.Y);
                }
                finally {
                    this.isUpdatingExternalMouse = false;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled) {
                if (this.IsDragging) {
                    if (e.Key == Key.Escape) {
                        this.hasCancelled_ignoreMouseUp = true;
                        e.Handled = true;
                        this.CancelInputEdit();
                        if (this.IsDragging) {
                            this.CancelDrag();
                        }

                        this.IsEditingTextBox = false;
                    }
                }
                // If the user previously edited another NumberDragger, then once they complete/cancel an edit, WPF
                // auto-focused that number dragger. Then they can press tab to navigate nearby draggers, and they can
                // edit them by just clicking a key. Massive convenience feature, saves having to use the mouse as much
                else if (this.CanEnableAutoEdit(e.Key) && !this.IsValueReadOnly && (this.HasEffectiveKeyboardFocus || this.IsFocused)) {
                    if (this.IsMouseCaptured) {
                        Debug.WriteLine("Unexpected mouse capture for KeyDown event");
                        this.ReleaseMouseCapture();
                    }

                    this.OnBeginInputEdit();
                }
            }
        }

        private bool CanEnableAutoEdit(Key k) {
            return k >= Key.D0 && k <= Key.D9 || k == Key.Enter;
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e) {
            if (!e.Handled && !this.IsDragging && this.IsEditingTextBox) {
                if ((e.Key == Key.Enter || e.Key == Key.Escape)) {
                    if (e.Key != Key.Enter || !this.TryCompleteEdit()) {
                        this.CancelInputEdit();
                    }

                    e.Handled = true;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e) {
            base.OnLostFocus(e);
            if (!this.ignoreLostFocus) {
                if (this.IsDragging) {
                    this.CancelDrag();
                }

                this.IsEditingTextBox = false;
            }
        }

        public void OnBeginInputEdit() {
            this.IsEditingTextBox = true;
            this.UpdateCursor();
            this.ExecuteCommand(ValueModStateBeginBox);
        }

        public bool TryCompleteEdit() {
            if (!this.IsValueReadOnly && double.TryParse(this.PART_TextBox.Text, out double value)) {
                this.CompleteInputEdit(value);
                return true;
            }
            else {
                return false;
            }
        }

        public void CompleteInputEdit(double value) {
            this.IsEditingTextBox = false;
            // TODO: figure out "trimmed" out part (due to rounding) and use that to figure out if the value is actually different
            this.Value = value;
            this.ExecuteCommand(ValueModStateFinishBox);
        }

        public void CancelInputEdit() {
            this.IsEditingTextBox = false;
            this.ExecuteCommand(ValueModStateCancelledBox);
        }

        public void BeginMouseDrag() {
            this.IsEditingTextBox = false;
            this.previousValue = this.Value;
            this.Focus();
            this.CaptureMouse();
            this.IsDragging = true;
            this.UpdateCursor();

            bool fail = true;
            try {
                this.RaiseEvent(new EditStartEventArgs());
                fail = false;
            }
            finally {
                if (fail) {
                    this.CancelDrag();
                }
            }

            this.ExecuteCommand(ValueModStateBeginBox);
        }

        public void CompleteDrag() {
            if (this.IsDragging) {
                this.ProcessDragCompletion(false);
                this.previousValue = null;
            }
        }

        public void CancelDrag() {
            if (this.IsDragging) {
                this.ProcessDragCompletion(true);
                if (this.previousValue is double oldVal) {
                    this.previousValue = null;
                    if (this.RestoreValueOnCancel) {
                        this.Value = oldVal;
                    }
                }
            }
        }

        private void ProcessDragCompletion(bool cancelled) {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.ClearValue(IsDraggingPropertyKey);

            this.lastMouseMove = null;
            if (this.lastClickPoint is Point point) {
                this.isUpdatingExternalMouse = true;
                try {
                    Point p = this.PointToScreen(point);
                    CursorUtils.SetCursorPos((int) p.X, (int) p.Y);
                }
                finally {
                    this.isUpdatingExternalMouse = false;
                }
            }

            this.lastClickPoint = null;
            this.UpdateCursor();

            this.RaiseEvent(new EditCompletedEventArgs(cancelled));
            this.ExecuteCommand(cancelled ? ValueModStateCancelledBox : ValueModStateFinishBox);
        }

        private void ExecuteCommand(object param) {
            ICommand cmd = this.EditStateChangedCommand;
            if (cmd != null && cmd.CanExecute(param)) {
                if (cmd is BaseAsyncRelayCommand asyncCommand) {
                    this.IsEnabled = false;
                    Task task = asyncCommand.ExecuteAsync(param);
                    if (task.IsCompleted) {
                        this.IsEnabled = true;
                    }
                    else {
                        task.ContinueWith(x => this.IsEnabled = true, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
                else {
                    cmd.Execute(param);
                }
            }
        }

        private Cursor GetCursorForOrientation() {
            Cursor cursor;
            switch (this.Orientation) {
                case Orientation.Horizontal:
                    cursor = Cursors.SizeWE;
                    break;
                case Orientation.Vertical:
                    cursor = Cursors.SizeNS;
                    break;
                default:
                    cursor = Cursors.Arrow;
                    break;
            }

            return cursor;
        }
    }
}