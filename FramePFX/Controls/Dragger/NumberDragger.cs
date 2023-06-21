using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using FramePFX.Core.Utils;

namespace FramePFX.Controls.Dragger {
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class NumberDragger : RangeBase {
        #region Dependency Properties

        public static readonly DependencyProperty TinyChangeProperty =
            DependencyProperty.Register(
                "TinyChange",
                typeof(double),
                typeof(NumberDragger),
                new PropertyMetadata(0.001d));

        public static readonly DependencyProperty MassiveChangeProperty =
            DependencyProperty.Register(
                "MassiveChange",
                typeof(double),
                typeof(NumberDragger),
                new PropertyMetadata(5d));

        protected static readonly DependencyPropertyKey IsDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsDragging",
                typeof(bool),
                typeof(NumberDragger),
                new PropertyMetadata(BoolBox.False,
                    (d, e) => ((NumberDragger) d).OnIsDraggingChanged((bool) e.OldValue, (bool) e.NewValue)));

        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        public static readonly DependencyProperty CompleteEditOnTextBoxLostFocusProperty =
            DependencyProperty.Register(
                "CompleteEditOnTextBoxLostFocus",
                typeof(bool?),
                typeof(NumberDragger),
                new PropertyMetadata(BoolBox.True));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(NumberDragger),
                new PropertyMetadata(Orientation.Horizontal,
                    (d, e) => ((NumberDragger) d).OnOrientationChanged((Orientation) e.OldValue, (Orientation) e.NewValue)));

        public static readonly DependencyProperty HorizontalIncrementProperty =
            DependencyProperty.Register(
                "HorizontalIncrement",
                typeof(HorizontalIncrement),
                typeof(NumberDragger),
                new PropertyMetadata(HorizontalIncrement.LeftDecrRightIncr));

        public static readonly DependencyProperty VerticalIncrementProperty =
            DependencyProperty.Register(
                "VerticalIncrement",
                typeof(VerticalIncrement),
                typeof(NumberDragger),
                new PropertyMetadata(VerticalIncrement.UpDecrDownIncr));

        public static readonly DependencyPropertyKey IsEditingTextBoxPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsEditingTextBox",
                typeof(bool),
                typeof(NumberDragger),
                new PropertyMetadata(BoolBox.False,
                    (d, e) => ((NumberDragger) d).OnIsEditingTextBoxChanged((bool) e.OldValue, (bool) e.NewValue),
                    (d, v) => ((NumberDragger) d).OnCoerceIsEditingTextBox(v)));

        public static readonly DependencyProperty IsEditingTextBoxProperty = IsEditingTextBoxPropertyKey.DependencyProperty;

        public static readonly DependencyProperty RoundedPlacesProperty =
            DependencyProperty.Register(
                "RoundedPlaces",
                typeof(int?),
                typeof(NumberDragger),
                new PropertyMetadata(null, (d, e) => ((NumberDragger) d).OnRoundedPlacesChanged((int?) e.OldValue, (int?) e.NewValue)));

        public static readonly DependencyProperty PreviewRoundedPlacesProperty =
            DependencyProperty.Register(
                "PreviewRoundedPlaces",
                typeof(int?),
                typeof(NumberDragger),
                new PropertyMetadata(2, (d, e) => ((NumberDragger) d).OnPreviewRoundedPlacesChanged((int?) e.OldValue, (int?) e.NewValue)));

        public static readonly DependencyProperty LockCursorWhileDraggingProperty =
            DependencyProperty.Register(
                "LockCursorWhileDragging",
                typeof(bool),
                typeof(NumberDragger),
                new PropertyMetadata(BoolBox.False, (d, e) => throw new NotImplementedException("Locking the mouse cursor is currently unsupported")));

        public static readonly DependencyProperty DisplayTextOverrideProperty =
            DependencyProperty.Register(
                "DisplayTextOverride",
                typeof(string),
                typeof(NumberDragger),
                new PropertyMetadata(null, (o, args) => ((NumberDragger) o).UpdateText()));

        public static readonly DependencyProperty ForcedReadOnlyStateProperty =
            DependencyProperty.Register(
                "ForcedReadOnlyState",
                typeof(bool?),
                typeof(NumberDragger),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RestoreValueOnCancelProperty =
            DependencyProperty.Register(
                "RestoreValueOnCancel",
                typeof(bool),
                typeof(NumberDragger),
                new PropertyMetadata(true));

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
        /// The number of digits to round the actual value to. Set to null to disable rounding
        /// </summary>
        public int? RoundedPlaces {
            get => (int?) this.GetValue(RoundedPlacesProperty);
            set => this.SetValue(RoundedPlacesProperty, value);
        }

        /// <summary>
        /// The number of digits to round the preview value (not the actual value). Set to null to disable rounding
        /// </summary>
        public int? PreviewRoundedPlaces {
            get => (int?) this.GetValue(PreviewRoundedPlacesProperty);
            set => this.SetValue(PreviewRoundedPlacesProperty, value);
        }

        private bool isUpdatingExternalMouse;

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

        public bool? ForcedReadOnlyState {
            get => (bool?) this.GetValue(ForcedReadOnlyStateProperty);
            set => this.SetValue(ForcedReadOnlyStateProperty, value.BoxNullable());
        }

        /// <summary>
        /// Whether or not to restore the value property when the drag is cancelled. Default is true
        /// </summary>
        public bool RestoreValueOnCancel {
            get { return (bool) this.GetValue(RestoreValueOnCancelProperty); }
            set => this.SetValue(RestoreValueOnCancelProperty, value.Box());
        }

        public bool IsValueReadOnly {
            get {
                if (this.GetValue(ForcedReadOnlyStateProperty) is bool forced) {
                    return forced;
                }

                Binding binding;
                BindingExpression expression = this.GetBindingExpression(ValueProperty);
                if (expression == null || (binding = expression.ParentBinding) == null || binding.Mode == BindingMode.Default) {
                    return false;
                }

                return binding.Mode == BindingMode.OneWay || binding.Mode == BindingMode.OneTime;
            }
        }

        #endregion

        public static readonly RoutedEvent EditStartedEvent = EventManager.RegisterRoutedEvent("EditStarted", RoutingStrategy.Bubble, typeof(EditStartEventHandler), typeof(NumberDragger));
        public static readonly RoutedEvent EditCompletedEvent = EventManager.RegisterRoutedEvent("EditCompleted", RoutingStrategy.Bubble, typeof(EditCompletedEventHandler), typeof(NumberDragger));

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

        private TextBlock PART_TextBlock;
        private TextBox PART_TextBox;
        private Point? lastClickPoint;
        private Point? lastMouseMove;
        private double? previousValue;
        private bool ignoreMouseMove;

        public NumberDragger() {
            this.Loaded += (s, e) => {
                this.CoerceValue(IsEditingTextBoxProperty);
                this.UpdateText();
                this.UpdateCursor();
            };
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_TextBlock = this.GetTemplateChild("PART_TextBlock") as TextBlock ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBlock));
            this.PART_TextBox = this.GetTemplateChild("PART_TextBox") as TextBox ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBox));
            this.PART_TextBox.Focusable = true;
            this.PART_TextBox.KeyDown += this.OnTextBoxKeyDown;
            this.PART_TextBox.GotFocus += (s, e) => {
                if (this.PART_TextBox.IsFocused || this.PART_TextBox.IsMouseCaptured) {
                    this.IsEditingTextBox = true;
                }
            };

            this.PART_TextBox.LostFocus += (s, e) => {
                if (this.IsEditingTextBox && this.CompleteEditOnTextBoxLostFocus is bool complete) {
                    if (!complete || !this.TryCompleteEdit()) {
                        this.CancelInputEdit();
                    }
                }

                this.IsEditingTextBox = false;
            };

            this.CoerceValue(IsEditingTextBoxProperty);
        }

        public double GetRoundedValue(double value, bool isPreview, out int? places) {
            if (this.RoundedPlaces is int rounding) {
                value = Math.Round(value, rounding);
                places = rounding;
            }
            else {
                places = null;
            }

            if (isPreview && this.PreviewRoundedPlaces is int roundingPreview) {
                value = Math.Round(value, roundingPreview);
                places = places != null ? Math.Min(places.Value, roundingPreview) : roundingPreview;
            }

            return value;
        }

        public double GetRoundedValue(double value, bool isPreview) {
            if (this.RoundedPlaces is int rounding)
                value = Math.Round(value, rounding);
            if (isPreview && this.PreviewRoundedPlaces is int roundingPreview)
                value = Math.Round(value, roundingPreview);
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

            this.UpdateText();
            if (oldValue != newValue) {
                this.PART_TextBox.Focus();
                this.PART_TextBox.SelectAll();
            }

            this.UpdateCursor();
        }

        private object OnCoerceIsEditingTextBox(object isEditing) {
            if (this.PART_TextBox == null || this.PART_TextBlock == null) {
                return isEditing;
            }

            if ((bool) isEditing) {
                this.PART_TextBox.Visibility = Visibility.Visible;
                this.PART_TextBlock.Visibility = Visibility.Hidden;
            }
            else {
                this.PART_TextBox.Visibility = Visibility.Hidden;
                this.PART_TextBlock.Visibility = Visibility.Visible;
            }

            this.PART_TextBox.IsReadOnly = this.IsValueReadOnly;
            return isEditing;
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

                if (this.IsDragging) {
                    this.Cursor = cursor;
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
            if (newValue != null) {
                this.UpdateText();
            }
        }

        protected virtual void OnPreviewRoundedPlacesChanged(int? oldValue, int? newValue) {
            if (newValue != null) {
                this.UpdateText();
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue) {
            base.OnValueChanged(oldValue, newValue);
            this.UpdateText();
        }

        protected void UpdateText() {
            if (this.PART_TextBox == null && this.PART_TextBlock == null) {
                return;
            }

            double value = this.GetRoundedValue(this.Value, true, out int? places);
            string valueTextPreview = places.HasValue ? value.ToString("F" + places.Value) : value.ToString();
            if (this.IsEditingTextBox) {
                if (this.PART_TextBox == null)
                    return;
                this.PART_TextBox.Text = places.HasValue ? value.ToString("F" + places.Value) : value.ToString();
            }
            else {
                if (this.PART_TextBlock == null)
                    return;
                string text = this.DisplayTextOverride;
                if (string.IsNullOrEmpty(text))
                    text = valueTextPreview;
                this.PART_TextBlock.Text = text;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            if (!this.IsDragging && !this.IsValueReadOnly) {
                e.Handled = true;
                this.Focus();

                this.ignoreMouseMove = true;
                try {
                    this.CaptureMouse();
                }
                finally {
                    this.ignoreMouseMove = false;
                }

                this.lastMouseMove = this.lastClickPoint = e.GetPosition(this);
                this.UpdateCursor();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            e.Handled = true;
            if (this.IsDragging) {
                this.CompleteDrag();
            }
            else if (this.IsMouseOver && !this.IsValueReadOnly) {
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                this.IsEditingTextBox = true;
                this.UpdateCursor();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove || this.isUpdatingExternalMouse || this.IsValueReadOnly) {
                return;
            }

            if (this.IsEditingTextBox) {
                if (this.IsDragging) {
                    Debug.WriteLine("IsDragging and IsEditingTextBox were both true");
                    this.previousValue = null;
                    this.CancelDrag();
                }

                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed) {
                if (this.IsDragging) {
                    this.CompleteDrag();
                }

                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape) && this.IsDragging) {
                this.CancelDrag();
                return;
            }

            Point mouse = e.GetPosition(this);
            if (this.lastClickPoint is Point lastClick && !this.IsDragging) {
                if (Math.Abs(mouse.X - lastClick.X) < 5d && Math.Abs(mouse.Y - lastClick.Y) < 5d) {
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

            if (!(this.lastMouseMove is Point lastMouse)) {
                return;
            }


            double change;
            Orientation orientation = this.Orientation;
            switch (orientation) {
                case Orientation.Horizontal: {
                    change = mouse.X - lastMouse.X;
                    break;
                }
                case Orientation.Vertical: {
                    change = mouse.Y - lastMouse.Y;
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

            double roundedValue = Maths.Clamp(this.GetRoundedValue(newValue, false), this.Minimum, this.Maximum);
            if (Maths.Equals(this.GetRoundedValue(this.Value, false), roundedValue)) {
                return;
            }

            this.Value = roundedValue;
            this.lastMouseMove = mouse;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Handled || !this.IsDragging || e.Key != Key.Escape) {
                return;
            }

            e.Handled = true;
            this.CancelInputEdit();
            if (this.IsDragging) {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
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
            if (this.IsDragging) {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
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
        }

        public void CancelInputEdit() {
            this.IsEditingTextBox = false;
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
        }

        public void CompleteDrag() {
            if (!this.IsDragging)
                return;

            this.ProcessDragCompletion(false);
            this.previousValue = null;
        }

        public void CancelDrag() {
            if (!this.IsDragging)
                return;

            this.ProcessDragCompletion(true);
            if (this.previousValue is double oldVal) {
                this.previousValue = null;
                if (this.RestoreValueOnCancel) {
                    this.Value = oldVal;
                }
            }
        }

        protected void ProcessDragCompletion(bool cancelled) {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.ClearValue(IsDraggingPropertyKey);

            this.lastMouseMove = null;
            this.lastClickPoint = null;
            this.UpdateCursor();

            this.RaiseEvent(new EditCompletedEventArgs(cancelled));
        }
    }

    // internal static class MouseUtils {
    //     [DllImport("user32.dll")]
    //     public static extern bool SetCursorPos(int x, int y);
    // 
    //     public static void SetCursorPos(Point position) {
    //         SetCursorPos((int) position.X, (int) position.Y);
    //     }
    // }
}
