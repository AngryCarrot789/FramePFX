using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using FramePFX.Core.Utils;
using OpenTK.Audio.OpenAL;

namespace FramePFX.Controls.Dragger
{
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class NumberDragger : RangeBase
    {
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

        public static readonly DependencyProperty LockCursorWhileDraggingProperty =
            DependencyProperty.Register(
                "LockCursorWhileDragging",
                typeof(bool),
                typeof(NumberDragger),
                new PropertyMetadata(BoolBox.True));

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
                new PropertyMetadata(BoolBox.True));

        public static readonly DependencyProperty ChangeMapperProperty =
            DependencyProperty.Register(
                "ChangeMapper",
                typeof(IChangeMapper),
                typeof(NumberDragger),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ValuePreProcessorProperty =
            DependencyProperty.Register(
                "ValuePreProcessor",
                typeof(IValuePreProcessor),
                typeof(NumberDragger),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ValueFormatterProperty = DependencyProperty.Register("ValueFormatter", typeof(IValueFormatter), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty EditStartedCommandProperty = DependencyProperty.Register("EditStartedCommand", typeof(ICommand), typeof(NumberDragger), new PropertyMetadata(null));
        public static readonly DependencyProperty EditCompletedCommandProperty = DependencyProperty.Register("EditCompletedCommand", typeof(ICommand), typeof(NumberDragger), new PropertyMetadata(null));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the tiny-change value. This is added or subtracted when CTRL + SHIFT is pressed
        /// </summary>
        public double TinyChange
        {
            get => (double) this.GetValue(TinyChangeProperty);
            set => this.SetValue(TinyChangeProperty, value);
        }

        /// <summary>
        /// Gets or sets the massive change value. This is added or subtracted when CTRL is pressed
        /// </summary>
        public double MassiveChange
        {
            get => (double) this.GetValue(MassiveChangeProperty);
            set => this.SetValue(MassiveChangeProperty, value);
        }

        public bool IsDragging
        {
            get => (bool) this.GetValue(IsDraggingProperty);
        }

        public bool? CompleteEditOnTextBoxLostFocus
        {
            get => (bool?) this.GetValue(CompleteEditOnTextBoxLostFocusProperty);
            set => this.SetValue(CompleteEditOnTextBoxLostFocusProperty, value.BoxNullable());
        }

        public Orientation Orientation
        {
            get => (Orientation) this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public HorizontalIncrement HorizontalIncrement
        {
            get => (HorizontalIncrement) this.GetValue(HorizontalIncrementProperty);
            set => this.SetValue(HorizontalIncrementProperty, value);
        }

        public VerticalIncrement VerticalIncrement
        {
            get => (VerticalIncrement) this.GetValue(VerticalIncrementProperty);
            set => this.SetValue(VerticalIncrementProperty, value);
        }

        public bool IsEditingTextBox
        {
            get => (bool) this.GetValue(IsEditingTextBoxProperty);
            protected set => this.SetValue(IsEditingTextBoxPropertyKey, value.Box());
        }

        /// <summary>
        /// The number of digits to round the actual value to. Set to null to disable rounding
        /// </summary>
        public int? RoundedPlaces
        {
            get => (int?) this.GetValue(RoundedPlacesProperty);
            set => this.SetValue(RoundedPlacesProperty, value);
        }

        public bool LockCursorWhileDragging
        {
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
        public string DisplayTextOverride
        {
            get => (string) this.GetValue(DisplayTextOverrideProperty);
            set => this.SetValue(DisplayTextOverrideProperty, value);
        }

        public bool? ForcedReadOnlyState
        {
            get => (bool?) this.GetValue(ForcedReadOnlyStateProperty);
            set => this.SetValue(ForcedReadOnlyStateProperty, value.BoxNullable());
        }

        /// <summary>
        /// Whether or not to restore the value property when the drag is cancelled. Default is true
        /// </summary>
        public bool RestoreValueOnCancel
        {
            get => (bool) this.GetValue(RestoreValueOnCancelProperty);
            set => this.SetValue(RestoreValueOnCancelProperty, value.Box());
        }

        public IChangeMapper ChangeMapper
        {
            get => (IChangeMapper) this.GetValue(ChangeMapperProperty);
            set => this.SetValue(ChangeMapperProperty, value);
        }

        public IValuePreProcessor ValuePreProcessor
        {
            get => (IValuePreProcessor) this.GetValue(ValuePreProcessorProperty);
            set => this.SetValue(ValuePreProcessorProperty, value);
        }

        /// <summary>
        /// An interface used to convert the value into a string in any form
        /// </summary>
        public IValueFormatter ValueFormatter
        {
            get => (IValueFormatter) this.GetValue(ValueFormatterProperty);
            set => this.SetValue(ValueFormatterProperty, value);
        }

        /// <summary>
        /// Gets or sets a command executed when an edit begins
        /// </summary>
        public ICommand EditStartedCommand
        {
            get => (ICommand) this.GetValue(EditStartedCommandProperty);
            set => this.SetValue(EditStartedCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets a command executed when an edit is completed, passing the cancelled state as a parameter
        /// </summary>
        public ICommand EditCompletedCommand
        {
            get => (ICommand) this.GetValue(EditCompletedCommandProperty);
            set => this.SetValue(EditCompletedCommandProperty, value);
        }

        public bool IsValueReadOnly
        {
            get
            {
                if (this.GetValue(ForcedReadOnlyStateProperty) is bool forced)
                    return forced;

                Binding binding;
                BindingExpression expression = this.GetBindingExpression(ValueProperty);
                if (expression == null || (binding = expression.ParentBinding) == null || binding.Mode == BindingMode.Default)
                    return false;

                return binding.Mode == BindingMode.OneWay || binding.Mode == BindingMode.OneTime;
            }
        }

        #endregion

        public static readonly RoutedEvent EditStartedEvent = EventManager.RegisterRoutedEvent(nameof(EditStarted), RoutingStrategy.Bubble, typeof(EditStartEventHandler), typeof(NumberDragger));
        public static readonly RoutedEvent EditCompletedEvent = EventManager.RegisterRoutedEvent(nameof(EditCompleted), RoutingStrategy.Bubble, typeof(EditCompletedEventHandler), typeof(NumberDragger));

        [Category("Behavior")]
        public event EditStartEventHandler EditStarted
        {
            add => this.AddHandler(EditStartedEvent, value);
            remove => this.RemoveHandler(EditStartedEvent, value);
        }

        [Category("Behavior")]
        public event EditCompletedEventHandler EditCompleted
        {
            add => this.AddHandler(EditCompletedEvent, value);
            remove => this.RemoveHandler(EditCompletedEvent, value);
        }

        private TextBlock PART_TextBlock;
        private TextBox PART_TextBox;
        private Point? lastClickPoint;
        private Point? lastMouseMove;
        private (int, int)? screenClip;
        private double? previousValue;
        private bool ignoreMouseMove;
        private bool isUpdatingExternalMouse;

        public NumberDragger()
        {
            this.Loaded += (s, e) =>
            {
                this.CoerceValue(IsEditingTextBoxProperty);
                this.UpdateText();
                this.UpdateCursor();
                this.RequeryChangeMapper(this.Value);
            };
        }

        static NumberDragger()
        {
            ValueProperty.OverrideMetadata(typeof(NumberDragger), new FrameworkPropertyMetadata(null, (o, value) => ((NumberDragger) o).OnCoerceValue(value)));
        }

        private object OnCoerceValue(object value)
        {
            double val = Maths.Clamp(this.GetRoundedValue((double) value, false), this.Minimum, this.Maximum);
            if (this.ValuePreProcessor is IValuePreProcessor processor)
            {
                double proc = processor.Process(val, this.Minimum, this.Maximum);
                if (!Maths.Equals(val, proc, 0.00000000001d))
                {
                    return proc;
                }
            }

            return val;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_TextBlock = this.GetTemplateChild("PART_TextBlock") as TextBlock ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBlock));
            this.PART_TextBox = this.GetTemplateChild("PART_TextBox") as TextBox ?? throw new Exception("Missing template part: " + nameof(this.PART_TextBox));
            this.PART_TextBox.Focusable = true;
            this.PART_TextBox.KeyDown += this.OnTextBoxKeyDown;
            this.PART_TextBox.GotFocus += (s, e) =>
            {
                if (this.PART_TextBox.IsFocused || this.PART_TextBox.IsMouseCaptured)
                {
                    this.IsEditingTextBox = true;
                }
            };

            this.PART_TextBox.LostFocus += (s, e) =>
            {
                if (this.IsEditingTextBox && this.CompleteEditOnTextBoxLostFocus is bool complete)
                {
                    if (!complete || !this.TryCompleteEdit())
                    {
                        this.CancelInputEdit();
                    }
                }

                this.IsEditingTextBox = false;
            };

            this.CoerceValue(IsEditingTextBoxProperty);
        }

        public double GetRoundedValue(double value, bool isPreview, out int? places)
        {
            if ((places = this.RoundedPlaces) is int rounding)
            {
                value = Math.Round(value, rounding);
            }

            return value;
        }

        public double GetRoundedValue(double value, bool isPreview)
        {
            if (this.RoundedPlaces is int rounding)
                value = Math.Round(value, rounding);
            return value;
        }

        protected virtual void OnIsDraggingChanged(bool oldValue, bool newValue)
        {
        }

        protected virtual void OnOrientationChanged(Orientation oldValue, Orientation newValue)
        {
            if (this.IsDragging)
            {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
        }

        protected virtual void OnIsEditingTextBoxChanged(bool oldValue, bool newValue)
        {
            if (newValue && this.IsDragging)
            {
                this.CancelDrag();
            }

            this.UpdateText();
            if (oldValue != newValue)
            {
                this.PART_TextBox.Focus();
                this.PART_TextBox.SelectAll();
            }

            this.UpdateCursor();
        }

        private object OnCoerceIsEditingTextBox(object isEditing)
        {
            if (this.PART_TextBox == null || this.PART_TextBlock == null)
            {
                return isEditing;
            }

            if ((bool) isEditing)
            {
                this.PART_TextBox.Visibility = Visibility.Visible;
                this.PART_TextBlock.Visibility = Visibility.Hidden;
            }
            else
            {
                this.PART_TextBox.Visibility = Visibility.Hidden;
                this.PART_TextBlock.Visibility = Visibility.Visible;
            }

            this.PART_TextBox.IsReadOnly = this.IsValueReadOnly;
            return isEditing;
        }

        public void UpdateCursor()
        {
            if (this.IsValueReadOnly)
            {
                if (this.IsEditingTextBox)
                {
                    if (this.PART_TextBlock != null)
                    {
                        this.PART_TextBlock.ClearValue(CursorProperty);
                    }
                    else
                    {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }

                    this.ClearValue(CursorProperty);
                }
                else
                {
                    this.Cursor = Cursors.No;
                    if (this.PART_TextBlock != null)
                    {
                        this.PART_TextBlock.Cursor = Cursors.No;
                    }
                    else
                    {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }
                }
            }
            else
            {
                if (this.IsDragging)
                {
                    this.Cursor = this.LockCursorWhileDragging ? Cursors.None : this.GetCursorForOrientation();
                    if (this.PART_TextBlock != null)
                    {
                        this.PART_TextBlock.ClearValue(CursorProperty);
                    }
                    else
                    {
                        Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                    }
                }
                else
                {
                    if (this.IsEditingTextBox)
                    {
                        if (this.PART_TextBlock != null)
                        {
                            this.PART_TextBlock.ClearValue(CursorProperty);
                        }
                        else
                        {
                            Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                        }

                        this.ClearValue(CursorProperty);
                    }
                    else
                    {
                        Cursor cursor = this.GetCursorForOrientation();
                        this.Cursor = cursor;
                        if (this.PART_TextBlock != null)
                        {
                            this.PART_TextBlock.Cursor = cursor;
                        }
                        else
                        {
                            Debug.WriteLine(nameof(this.PART_TextBlock) + " is null?");
                        }
                    }
                }
            }
        }

        protected virtual void OnRoundedPlacesChanged(int? oldValue, int? newValue)
        {
            if (newValue != null)
            {
                this.UpdateText();
            }
        }

        protected virtual void OnPreviewRoundedPlacesChanged(int? oldValue, int? newValue)
        {
            if (newValue != null)
            {
                this.UpdateText();
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            this.UpdateText();
            this.RequeryChangeMapper(newValue);
        }

        private void RequeryChangeMapper(double value)
        {
            if (this.ChangeMapper is IChangeMapper mapper)
            {
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

        private string GetPreviewText(out double value)
        {
            value = this.GetRoundedValue(this.Value, true, out int? places);
            if (this.ValueFormatter is IValueFormatter formatter)
            {
                return formatter.ToString(value, places);
            }

            return places.HasValue ? value.ToString("F" + places.Value.ToString()) : value.ToString();
        }

        protected void UpdateText()
        {
            if (this.PART_TextBox == null && this.PART_TextBlock == null)
            {
                return;
            }

            if (this.IsEditingTextBox)
            {
                if (this.PART_TextBox == null)
                    return;
                this.PART_TextBox.Text = this.GetPreviewText(out _);
            }
            else
            {
                if (this.PART_TextBlock == null)
                    return;
                string text = this.DisplayTextOverride;
                if (string.IsNullOrEmpty(text))
                    text = this.GetPreviewText(out _);
                this.PART_TextBlock.Text = text;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (e.LeftButton != MouseButtonState.Pressed && this.IsDragging)
            {
                this.CompleteDrag();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!this.IsDragging && !this.IsValueReadOnly)
            {
                e.Handled = true;
                this.Focus();

                this.ignoreMouseMove = true;
                try
                {
                    this.CaptureMouse();
                }
                finally
                {
                    this.ignoreMouseMove = false;
                }

                this.lastMouseMove = this.lastClickPoint = e.GetPosition(this);
                this.UpdateCursor();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (this.IsDragging)
            {
                this.CompleteDrag();
            }
            else if (this.IsMouseOver && !this.IsValueReadOnly)
            {
                if (this.IsMouseCaptured)
                {
                    this.ReleaseMouseCapture();
                }

                this.IsEditingTextBox = true;
                this.UpdateCursor();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove || this.isUpdatingExternalMouse || this.IsValueReadOnly)
            {
                return;
            }

            if (this.IsEditingTextBox)
            {
                if (this.IsDragging)
                {
                    Debug.WriteLine("IsDragging and IsEditingTextBox were both true");
                    this.previousValue = null;
                    this.CancelDrag();
                }

                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                if (this.IsDragging)
                {
                    this.CompleteDrag();
                }

                return;
            }

            if (Keyboard.IsKeyDown(Key.Escape) && this.IsDragging)
            {
                this.CancelDrag();
                return;
            }

            if (!(this.lastMouseMove is Point lastPos))
            {
                return;
            }

            Point mpos = e.GetPosition(this);
            if (this.LockCursorWhileDragging)
            {
                bool wrap = false;
                double x = mpos.X, y = mpos.Y;
                if (this.Orientation == Orientation.Horizontal)
                {
                    if (mpos.X < 0)
                    {
                        x = this.ActualWidth;
                        wrap = true;
                    }
                    else if (mpos.X > this.ActualWidth)
                    {
                        x = 0;
                        wrap = true;
                    }
                }
                else
                {
                    if (mpos.Y < 0)
                    {
                        y = this.ActualHeight;
                        wrap = true;
                    }
                    else if (mpos.X > this.ActualHeight)
                    {
                        y = 0;
                        wrap = true;
                    }
                }

                if (wrap)
                {
                    this.isUpdatingExternalMouse = true;
                    try
                    {
                        Point mp = new Point(x, y);
                        this.lastMouseMove = mp;
                        Point sp = this.PointToScreen(mp);
                        CursorUtils.SetCursorPos((int) sp.X, (int) sp.Y);
                    }
                    finally
                    {
                        this.isUpdatingExternalMouse = false;
                    }

                    return;
                }
            }

            if (this.lastClickPoint is Point lastClick && !this.IsDragging)
            {
                if (Math.Abs(mpos.X - lastClick.X) < 5d && Math.Abs(mpos.Y - lastClick.Y) < 5d)
                {
                    return;
                }

                this.BeginMouseDrag();
            }

            if (!this.IsDragging)
            {
                return;
            }

            if (this.IsEditingTextBox)
            {
                Debug.WriteLine("IsEditingTextBox and IsDragging were both true");
                this.IsEditingTextBox = false;
            }

            double change;
            Orientation orientation = this.Orientation;
            switch (orientation)
            {
                case Orientation.Horizontal:
                {
                    change = mpos.X - lastPos.X;
                    break;
                }
                case Orientation.Vertical:
                {
                    change = mpos.Y - lastPos.Y;
                    break;
                }
                default:
                {
                    throw new Exception("Invalid orientation: " + orientation);
                }
            }

            bool isShiftDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool isCtrlDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (isShiftDown)
            {
                if (isCtrlDown)
                {
                    change *= this.TinyChange;
                }
                else
                {
                    change *= this.SmallChange;
                }
            }
            else if (isCtrlDown)
            {
                change *= this.MassiveChange;
            }
            else
            {
                change *= this.LargeChange;
            }

            double newValue;
            if ((orientation == Orientation.Horizontal && this.HorizontalIncrement == HorizontalIncrement.LeftDecrRightIncr) ||
                (orientation == Orientation.Vertical && this.VerticalIncrement == VerticalIncrement.UpDecrDownIncr))
            {
                newValue = this.Value + change;
            }
            else
            {
                newValue = this.Value - change;
            }

            double roundedValue = Maths.Clamp(this.GetRoundedValue(newValue, false), this.Minimum, this.Maximum);
            if (Maths.Equals(this.GetRoundedValue(this.Value, false), roundedValue))
            {
                return;
            }

            this.Value = roundedValue;
            this.lastMouseMove = mpos;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled || !this.IsDragging || e.Key != Key.Escape)
            {
                return;
            }

            e.Handled = true;
            this.CancelInputEdit();
            if (this.IsDragging)
            {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled && !this.IsDragging && this.IsEditingTextBox)
            {
                if ((e.Key == Key.Enter || e.Key == Key.Escape))
                {
                    if (e.Key != Key.Enter || !this.TryCompleteEdit())
                    {
                        this.CancelInputEdit();
                    }

                    e.Handled = true;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (this.IsDragging)
            {
                this.CancelDrag();
            }

            this.IsEditingTextBox = false;
        }

        public bool TryCompleteEdit()
        {
            if (!this.IsValueReadOnly && double.TryParse(this.PART_TextBox.Text, out double value))
            {
                this.CompleteInputEdit(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CompleteInputEdit(double value)
        {
            this.IsEditingTextBox = false;
            // TODO: figure out "trimmed" out part (due to rounding) and use that to figure out if the value is actually different
            this.Value = value;
        }

        public void CancelInputEdit()
        {
            this.IsEditingTextBox = false;
        }

        public void BeginMouseDrag()
        {
            this.IsEditingTextBox = false;
            this.previousValue = this.Value;
            this.Focus();
            this.CaptureMouse();
            this.SetValue(IsDraggingPropertyKey, BoolBox.True);
            this.UpdateCursor();

            bool fail = true;
            try
            {
                this.RaiseEvent(new EditStartEventArgs());
                fail = false;
            }
            finally
            {
                if (fail)
                {
                    this.CancelDrag();
                }
            }

            if (this.EditStartedCommand is ICommand command && command.CanExecute(null))
            {
                command.Execute(null);
            }
        }

        public void CompleteDrag()
        {
            if (!this.IsDragging)
                return;

            this.ProcessDragCompletion(false);
            this.previousValue = null;
        }

        public void CancelDrag()
        {
            if (!this.IsDragging)
                return;

            this.ProcessDragCompletion(true);
            if (this.previousValue is double oldVal)
            {
                this.previousValue = null;
                if (this.RestoreValueOnCancel)
                {
                    this.Value = oldVal;
                }
            }
        }

        protected void ProcessDragCompletion(bool cancelled)
        {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.ClearValue(IsDraggingPropertyKey);

            this.lastMouseMove = null;
            if (this.lastClickPoint is Point point)
            {
                this.isUpdatingExternalMouse = true;
                try
                {
                    Point p = this.PointToScreen(point);
                    CursorUtils.SetCursorPos((int) p.X, (int) p.Y);
                }
                finally
                {
                    this.isUpdatingExternalMouse = false;
                }
            }

            this.lastClickPoint = null;
            this.UpdateCursor();

            this.RaiseEvent(new EditCompletedEventArgs(cancelled));
            if (this.EditCompletedCommand is ICommand command && command.CanExecute(cancelled.Box()))
            {
                command.Execute(cancelled.Box());
            }
        }

        private Cursor GetCursorForOrientation()
        {
            Cursor cursor;
            switch (this.Orientation)
            {
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