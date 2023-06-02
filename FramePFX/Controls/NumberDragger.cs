﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Controls {
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class NumberDragger : RangeBase {
        public static readonly DependencyProperty TinyChangeProperty =
            DependencyProperty.Register(
                "TinyChange",
                typeof(double),
                typeof(NumberDragger),
                new PropertyMetadata(0.01d));

        public static readonly DependencyProperty MassiveChangeProperty =
            DependencyProperty.Register(
                "MassiveChange",
                typeof(double),
                typeof(NumberDragger),
                new PropertyMetadata(1d));

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
                    (d, v) => ((NumberDragger) d).OnCoerceIsEditingTextBox((bool) v)));

        public static readonly DependencyProperty IsEditingTextBoxProperty = IsEditingTextBoxPropertyKey.DependencyProperty;

        public static readonly DependencyProperty RoundedPlacesProperty =
            DependencyProperty.Register(
                "RoundedPlaces",
                typeof(int),
                typeof(NumberDragger),
                new PropertyMetadata(4,
                    (d, e) => ((NumberDragger) d).OnRoundedPlacesChanged((int) e.OldValue, (int) e.NewValue)));

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
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that is displayed while the value preview is active, instead of displaying the
        /// actual value. A text box will still appear if the control is clicked
        /// </summary>
        public string DisplayTextOverride {
            get => (string) this.GetValue(DisplayTextOverrideProperty);
            set => this.SetValue(DisplayTextOverrideProperty, value);
        }

        private TextBlock PART_TextBlock;
        private TextBox PART_TextBox;
        private Point? lastClickPoint;
        private Point? lastMouseMove;
        private double? previousValue;
        private bool ignoreMouseMove;

        public double TinyChange {
            get => (double) this.GetValue(TinyChangeProperty);
            set => this.SetValue(TinyChangeProperty, value);
        }

        /// <summary>
        /// The amount to add per pixel of change while dragging
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

        public int RoundedPlaces {
            get => (int) this.GetValue(RoundedPlacesProperty);
            set => this.SetValue(RoundedPlacesProperty, value);
        }

        private bool isUpdatingExternalMouse;

        public bool LockCursorWhileDragging {
            get => (bool) this.GetValue(LockCursorWhileDraggingProperty);
            set => this.SetValue(LockCursorWhileDraggingProperty, value.Box());
        }

        public double RoundedValue => this.GetRoundedValue(this.Value);

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

        public double GetRoundedValue(double value) {
            return Math.Round(value, this.RoundedPlaces);
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

        protected virtual void OnDisplayTextOverrideChanged(string oldValue, string newValue) {
            this.UpdateText();
        }

        private bool OnCoerceIsEditingTextBox(bool isEditing) {
            if (this.PART_TextBox == null || this.PART_TextBlock == null) {
                return isEditing;
            }

            if (isEditing) {
                this.PART_TextBox.Visibility = Visibility.Visible;
                this.PART_TextBlock.Visibility = Visibility.Hidden;
            }
            else {
                this.PART_TextBox.Visibility = Visibility.Hidden;
                this.PART_TextBlock.Visibility = Visibility.Visible;
            }

            return isEditing;
        }

        public void UpdateCursor() {
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
                this.PART_TextBlock.ClearValue(CursorProperty);
            }
            else {
                if (this.IsEditingTextBox) {
                    this.PART_TextBlock.ClearValue(CursorProperty);
                    this.ClearValue(CursorProperty);
                }
                else {
                    this.Cursor = cursor;
                    this.PART_TextBlock.Cursor = cursor;
                }
            }
        }

        protected virtual void OnRoundedPlacesChanged(int? oldValue, int? newValue) {
            if (newValue != null) {
                this.UpdateText();
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue) {
            base.OnValueChanged(oldValue, newValue);
            this.UpdateText();
        }

        protected void UpdateText() {
            if (this.IsEditingTextBox) {
                if (this.PART_TextBox == null)
                    return;
                this.PART_TextBox.Text = this.RoundedValue.ToString();
            }
            else {
                if (this.PART_TextBlock == null)
                    return;
                string text = this.DisplayTextOverride;
                if (string.IsNullOrEmpty(text))
                    text = this.RoundedValue.ToString();
                this.PART_TextBlock.Text = text;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            if (!this.IsDragging) {
                e.Handled = true;
                this.Focus();

                this.ignoreMouseMove = true;
                try {
                    this.CaptureMouse();
                    Debug.WriteLine("Mouse Captured");
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
            else if (this.IsMouseOver) {
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                    Debug.WriteLine("Mouse Capture Released");
                }

                this.IsEditingTextBox = true;
                this.UpdateCursor();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove || this.isUpdatingExternalMouse) {
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

            double roundedValue = Maths.Clamp(this.GetRoundedValue(newValue), this.Minimum, this.Maximum);
            if (Maths.Equals(this.RoundedValue, roundedValue)) {
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
            if (double.TryParse(this.PART_TextBox.Text, out double value)) {
                this.CompleteInputEdit(value);
                return true;
            }
            else {
                return false;
            }
        }

        public void CompleteInputEdit(double value) {
            this.IsEditingTextBox = false;
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
            Debug.WriteLine("[BeginMouseDrag] Mouse Captured");
            this.IsDragging = true;
            this.UpdateCursor();
        }

        public void CompleteDrag() {
            this.CleanUpDrag();
            this.previousValue = null;
        }

        public void CancelDrag() {
            this.CleanUpDrag();
            if (this.previousValue is double oldVal) {
                this.previousValue = null;
                this.Value = oldVal;
            }
        }

        protected void CleanUpDrag() {
            if (!this.IsDragging)
                return;
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.ClearValue(IsDraggingPropertyKey);

            this.lastMouseMove = null;
            this.lastClickPoint = null;

            this.UpdateCursor();
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
