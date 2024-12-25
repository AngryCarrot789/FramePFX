// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.Avalonia.AvControls.Dragger.Expressions;
using FramePFX.Avalonia.Utils;
using FramePFX.Interactivity.Formatting;
using FramePFX.Utils;
using Key = Avalonia.Input.Key;

namespace FramePFX.Avalonia.AvControls.Dragger;

public class InvalidInputEnteredEventArgs : RoutedEventArgs {
    public string Input { get; }

    public InvalidInputEnteredEventArgs(string input, NumberDragger dragger) : base(NumberDragger.InvalidInputEnteredEvent, dragger) {
        Validate.NotNull(input);
        this.Input = input;
    }
}

public class NumberDragger : RangeBase {
    public static readonly RoutedEvent<InvalidInputEnteredEventArgs> InvalidInputEnteredEvent = RoutedEvent.Register<RangeBase, InvalidInputEnteredEventArgs>("InvalidInputEntered", RoutingStrategies.Bubble);
    public static readonly StyledProperty<double> TinyChangeProperty = AvaloniaProperty.Register<NumberDragger, double>("TinyChange", 0.1);
    public static readonly StyledProperty<double> NormalChangeProperty = AvaloniaProperty.Register<NumberDragger, double>("NormalChange", 1.0);
    public static readonly StyledProperty<DragDirection> DragDirectionProperty = AvaloniaProperty.Register<NumberDragger, DragDirection>("DragDirection", DragDirection.LeftDecrRightIncr);
    public static readonly StyledProperty<bool> LockCursorOnDragProperty = AvaloniaProperty.Register<NumberDragger, bool>("LockCursorOnDrag", true);
    public static readonly StyledProperty<int> NonFormattedRoundedPlacesProperty = AvaloniaProperty.Register<NumberDragger, int>("NonFormattedRoundedPlaces", 2);
    public static readonly StyledProperty<int> NonFormattedRoundedPlacesForEditProperty = AvaloniaProperty.Register<NumberDragger, int>("NonFormattedRoundedPlacesForEdit", 6);
    public static readonly StyledProperty<IValueFormatter?> ValueFormatterProperty = AvaloniaProperty.Register<NumberDragger, IValueFormatter?>("ValueFormatter");
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner<NumberDragger>();
    public static readonly StyledProperty<string?> TextPreviewOverrideProperty = AvaloniaProperty.Register<NumberDragger, string?>("TextPreviewOverride");
    public static readonly StyledProperty<bool?> CompleteEditOnTextBoxLostFocusProperty = AvaloniaProperty.Register<NumberDragger, bool?>("CompleteEditOnTextBoxLostFocus", true);
    public static readonly DirectProperty<NumberDragger, bool> IsEditingProperty = AvaloniaProperty.RegisterDirect<NumberDragger, bool>("IsEditing", o => o.isEditing);
    public static readonly StyledProperty<string?> FinalPreviewStringFormatProperty = AvaloniaProperty.Register<NumberDragger, string?>("FinalPreviewStringFormat");
    public static readonly StyledProperty<bool> IsIntegerValueProperty = AvaloniaProperty.Register<NumberDragger, bool>("IsIntegerValue");

    private TextBlock? PART_TextBlock;
    private TextBox? PART_TextBox;
    private Point lastClickPos, lastMouseMove;
    private int dragState; // 0 = default, 1 = standby, 2 = active
    private bool isEditing;
    private bool flagHasSpecialPropertyChangedWhileEditing;
    private double accumulator;

    public double NormalChange {
        get => this.GetValue(NormalChangeProperty);
        set => this.SetValue(NormalChangeProperty, value);
    }

    public double TinyChange {
        get => this.GetValue(TinyChangeProperty);
        set => this.SetValue(TinyChangeProperty, value);
    }

    public DragDirection DragDirection {
        get => this.GetValue(DragDirectionProperty);
        set => this.SetValue(DragDirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets if the mouse cursor should be locked in place while dragging. Only
    /// supported on windows, will crash on other operating systems due to this using Win32 functions
    /// </summary>
    public bool LockCursorOnDrag {
        get => this.GetValue(LockCursorOnDragProperty);
        set => this.SetValue(LockCursorOnDragProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of rounded places to use for the value in the value preview when
    /// not editing. This value is ignored when a <see cref="ValueFormatter"/> is present
    /// </summary>
    public int NonFormattedRoundedPlaces {
        get => this.GetValue(NonFormattedRoundedPlacesProperty);
        set => this.SetValue(NonFormattedRoundedPlacesProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of rounded places to use for the value in the text box when
    /// editing the value. This value is ignored when a <see cref="ValueFormatter"/> is present
    /// </summary>
    public int NonFormattedRoundedPlacesForEdit {
        get => this.GetValue(NonFormattedRoundedPlacesForEditProperty);
        set => this.SetValue(NonFormattedRoundedPlacesForEditProperty, value);
    }

    /// <summary>
    /// Gets or sets the value formatter used to post-process the final effective
    /// <see cref="RangeBase.Value"/> into a string presentable to use user
    /// </summary>
    public IValueFormatter? ValueFormatter {
        get => this.GetValue(ValueFormatterProperty);
        set => this.SetValue(ValueFormatterProperty, value);
    }

    /// <summary>
    /// Gets or sets the text alignment used for the preview and editor text
    /// </summary>
    public TextAlignment TextAlignment {
        get => this.GetValue(TextAlignmentProperty);
        set => this.SetValue(TextAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the text that is shown instead of the actual (non-editing formatted) value.
    /// Null by default, which disables this feature. This text is not shown when editing via the text box
    /// </summary>
    public string? TextPreviewOverride {
        get => this.GetValue(TextPreviewOverrideProperty);
        set => this.SetValue(TextPreviewOverrideProperty, value);
    }

    public bool? CompleteEditOnTextBoxLostFocus {
        get => this.GetValue(CompleteEditOnTextBoxLostFocusProperty);
        set => this.SetValue(CompleteEditOnTextBoxLostFocusProperty, value);
    }

    public bool IsEditing {
        get => this.isEditing;
        set {
            if (this.isEditing == value)
                return;

            this.isEditing = value;
            this.flagHasSpecialPropertyChangedWhileEditing = false;
            this.UpdateTextControlVisibility();
            this.UpdateTextBlockAndBox();
            this.RaisePropertyChanged(IsEditingProperty, !value, value);
            if (value && this.PART_TextBox != null) {
                BugFix.TextBox_FocusSelectAll(this.PART_TextBox);
            }
        }
    }

    /// <summary>
    /// A string format that controls the absolute final format of the value preview only (not the is-editing value)
    /// </summary>
    public string? FinalPreviewStringFormat {
        get => this.GetValue(FinalPreviewStringFormatProperty);
        set => this.SetValue(FinalPreviewStringFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets if this number dragger should treat our value like an integer. This obviously means our value cannot have decimal places
    /// </summary>
    public bool IsIntegerValue {
        get => this.GetValue(IsIntegerValueProperty);
        set => this.SetValue(IsIntegerValueProperty, value);
    }

    /// <summary>
    /// An event fired when the user inputs text (while <see cref="IsEditing"/> is true) that could not be converted
    /// back into a double value. This can be used to for example implement commands through the number dragger
    /// </summary>
    public event EventHandler<InvalidInputEnteredEventArgs>? InvalidInputEntered {
        add => this.AddHandler(InvalidInputEnteredEvent, value);
        remove => this.RemoveHandler(InvalidInputEnteredEvent, value);
    }

    public NumberDragger() {
    }

    static NumberDragger() {
        ValueProperty.Changed.AddClassHandler<NumberDragger, double>((o, e) => o.OnValueChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ValueProperty.OverrideMetadata<NumberDragger>(new StyledPropertyMetadata<double>(coerce: (o, value) => {
            double coerced = double.IsInfinity(value) || double.IsNaN(value) ? o.GetValue(ValueProperty) : Maths.Clamp(value, o.GetValue(MinimumProperty), o.GetValue(MaximumProperty));
            if (o.GetValue(IsIntegerValueProperty))
                coerced = (long) coerced;
            return coerced;
        }));

        TextPreviewOverrideProperty.Changed.AddClassHandler<NumberDragger, string?>((o, e) => o.UpdateTextBlockOnly());
        IsIntegerValueProperty.Changed.AddClassHandler<NumberDragger, bool>((o, e) => o.CoerceValue(ValueProperty));

        PropertyAffectsIgnoreLostFocusValueChange(NonFormattedRoundedPlacesForEditProperty, NonFormattedRoundedPlacesProperty, ValueFormatterProperty, ValueProperty);
        ValueFormatterProperty.Changed.AddClassHandler<NumberDragger, IValueFormatter?>((d, e) => {
            if (e.TryGetOldValue(out IValueFormatter? oldFormatter))
                oldFormatter.InvalidateFormat -= d.OnValueFormatInvalidated;
            if (e.TryGetNewValue(out IValueFormatter? newFormatter))
                newFormatter.InvalidateFormat += d.OnValueFormatInvalidated;
            if (!d.isEditing) {
                d.UpdateTextBlockAndBox();
            }
        });

        FinalPreviewStringFormatProperty.Changed.AddClassHandler<NumberDragger, string?>((d, e) => d.UpdateTextBlockAndBox());
    }

    private static void PropertyAffectsIgnoreLostFocusValueChange(params AvaloniaProperty[] properties) {
        foreach (AvaloniaProperty property in properties) {
            property.Changed.AddClassHandler<NumberDragger>(InvalidateThingy);
        }
    }

    private static void InvalidateThingy(NumberDragger dragger, AvaloniaPropertyChangedEventArgs arg2) {
        if (dragger.isEditing) {
            dragger.flagHasSpecialPropertyChangedWhileEditing = true;
        }
    }

    private void OnValueFormatInvalidated(object? sender, EventArgs e) {
        if (this.isEditing) {
            this.flagHasSpecialPropertyChangedWhileEditing = true;
        }
        else {
            this.UpdateTextBlockAndBox();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_TextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_TextBlock));
        this.PART_TextBox = e.NameScope.GetTemplateChild<TextBox>(nameof(this.PART_TextBox));
        if (this.PART_TextBox != null) {
            this.PART_TextBox.KeyDown += this.OnTextInputKeyPress;
            this.PART_TextBox.LostFocus += this.OnTextInputFocusLost;
        }
    }

    private string GetValueToString(bool isEditing) => this.GetValueToString(this.Value, isEditing);

    private string GetValueToString(double value, bool isEditing) {
        if (this.ValueFormatter is IValueFormatter formatter) {
            return formatter.ToString(value, isEditing);
        }
        else {
            int roundedPlaces = isEditing ? this.NonFormattedRoundedPlacesForEdit : this.NonFormattedRoundedPlaces;
            return value.ToString("F" + Math.Max(roundedPlaces, 0));
        }
    }

    private void UpdateTextBlockOnly() {
        string? reff = null;
        this.UpdateTextBlockOnly(ref reff);
    }

    private void UpdateTextBlockOnly(ref string? textBlock) {
        if (this.PART_TextBlock != null && !this.isEditing) {
            string value = this.TextPreviewOverride ?? (textBlock = this.GetValueToString(false));
            if (this.FinalPreviewStringFormat is string format) {
                value = string.Format(format, value);
            }

            this.PART_TextBlock.Text = value;
        }
    }

    private void UpdateTextBlockAndBox() {
        string? textBlock = null;
        this.UpdateTextBlockOnly(ref textBlock);
        if (this.PART_TextBox != null)
            this.PART_TextBox.Text = this.isEditing ? this.GetValueToString(true) : (textBlock ?? this.GetValueToString(false));
    }

    private void OnValueChanged(double oldValue, double newValue) => this.UpdateTextBlockAndBox();

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.UpdateCursor();
        this.UpdateTextControlVisibility();
        this.UpdateTextBlockAndBox();
    }

    private void UpdateTextControlVisibility() {
        if (this.PART_TextBlock != null)
            this.PART_TextBlock!.IsVisible = !this.isEditing;
        if (this.PART_TextBox != null)
            this.PART_TextBox!.IsVisible = this.isEditing;
    }

    private void UpdateCursor() {
        this.Cursor = new Cursor(this.dragState != 2 ? StandardCursorType.Arrow : StandardCursorType.None);
    }

    private void OnTextInputFocusLost(object? sender, RoutedEventArgs e) {
        if (this.IsEditing && this.CompleteEditOnTextBoxLostFocus == true) {
            this.CompleteEdit(Key.Enter, true); // Simulate pressing enter
        }

        this.IsEditing = false;
    }

    private void OnTextInputKeyPress(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Enter || e.Key == Key.Escape) {
            this.CompleteEdit(e.Key, false);
        }
    }

    private bool CompleteEdit(Key inputKey, bool isCompletingFromLostFocus) {
        bool specialFlag = this.flagHasSpecialPropertyChangedWhileEditing;
        string? parseText = this.PART_TextBox!.Text;
        this.IsEditing = false;
        if (parseText == null || inputKey == Key.Escape) {
            return false;
        }

        // If all the conditions are right, we can prevent the ValueChanged event firing
        // when IsEditing is set to false since it can mess up certain systems if the value
        // doesn't truly change by any marginal amount
        if (isCompletingFromLostFocus && !specialFlag && parseText.Equals(this.GetValueToString(true))) {
            return true;
        }

        if (this.ParseInput(parseText, out double parsedValue)) {
            this.Value = parsedValue;
            return true;
        }

        using ComplexNumericExpression.ExpressionState state = ComplexNumericExpression.DefaultParser.PushState();
        state.SetVariable("value", this.Value);
        state.SetVariable("pi", Math.PI);
        state.SetVariable("e", Math.E);
        try {
            parsedValue = state.Expression.Parse(parseText);
        }
        catch {
            this.RaiseEvent(new InvalidInputEnteredEventArgs(parseText, this));
            return false;
        }

        if (this.ValueFormatter is IValueFormatter formatter) {
            if (formatter.TryConvertToDouble(parsedValue.ToString(), out double value)) {
                this.Value = value;
                return true;
            }
        }

        this.Value = parsedValue;
        return true;
    }

    private bool ParseInput(string parseText, out double output) {
        if (this.ValueFormatter is IValueFormatter formatter) {
            if (formatter.TryConvertToDouble(parseText, out double value)) {
                output = value;
                return true;
            }
        }
        else if (double.TryParse(parseText, out double newValue)) {
            output = newValue;
            return true;
        }

        output = default;
        return false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        e.Handled = true;
        this.dragState = 1;
        this.lastClickPos = this.lastMouseMove = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.PreventGestureRecognition();
        this.UpdateCursor();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        e.Handled = true;
        int state = this.dragState;
        this.dragState = 0;

        if (state == 1) {
            this.IsEditing = true;
        }
        else {
            if (ReferenceEquals(e.Pointer.Captured, this))
                e.Pointer.Capture(null);
        }

        this.UpdateCursor();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) {
        base.OnPointerCaptureLost(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && this.dragState != 0) {
            e.Handled = true;
            this.dragState = 0;
            this.UpdateCursor();
        }
        else if (this.dragState == 0 && this.IsKeyboardFocusWithin && !this.IsModifierKey(e.Key)) {
            // Begin editing when we have focus, e.g. via tab
            // indexing, and a non-modifier key is pressed
            this.IsEditing = true;
            this.UpdateCursor();
        }
    }

    private bool IsModifierKey(Key k) {
        return k == Key.LWin || k == Key.RWin || (k >= Key.LeftShift && k <= Key.RightAlt);
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (this.dragState == 0) {
            return;
        }

        PointerPoint pointer = e.GetCurrentPoint(this);
        if (!pointer.Properties.IsLeftButtonPressed) {
            this.dragState = 0;
            if (ReferenceEquals(e.Pointer.Captured, this))
                e.Pointer.Capture(null);
            this.UpdateCursor();
            return;
        }

        Point point = e.GetPosition(this);
        DragDirection dir = this.DragDirection;
        Point delta = point - this.lastMouseMove;

        if (this.dragState == 1) {
            if (dir == DragDirection.LeftDecrRightIncr || dir == DragDirection.LeftIncrRightDecr) {
                if (!(Math.Abs(delta.X) > 4))
                    return;
            }
            else if (!(Math.Abs(delta.Y) > 4)) {
                return;
            }

            this.dragState = 2;
            this.UpdateCursor();
        }

        bool isShiftDown = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        bool isCtrlDown = (e.KeyModifiers & KeyModifiers.Control) != 0;

        if (isShiftDown) {
            if (isCtrlDown) {
                delta *= this.TinyChange;
            }
            else {
                delta *= this.SmallChange;
            }
        }
        else if (isCtrlDown) {
            delta *= this.LargeChange;
        }
        else {
            delta *= this.NormalChange;
        }

        double oldValue = this.Value, newValue;
        switch (dir) {
            case DragDirection.LeftDecrRightIncr: newValue = oldValue + delta.X; break;
            case DragDirection.LeftIncrRightDecr: newValue = oldValue - delta.X; break;
            case DragDirection.UpDecrDownIncr:    newValue = oldValue + delta.Y; break;
            case DragDirection.UpIncrDownDecr:    newValue = oldValue - delta.Y; break;
            default:                              throw new ArgumentOutOfRangeException();
        }

        newValue = Maths.Clamp(newValue + this.accumulator, this.Minimum, this.Maximum);
        this.accumulator = 0;
        if (!DoubleUtils.AreClose(newValue, oldValue)) {
            this.Value = newValue;
            oldValue = this.Value;
        }

        if (!DoubleUtils.AreClose(newValue, oldValue)) {
            this.accumulator += (newValue - oldValue);
        }

        if (this.LockCursorOnDrag && OperatingSystem.IsWindows()) {
            PixelPoint sp = this.PointToScreen(this.lastClickPos);
            CursorUtils.SetCursorPos(sp.X, sp.Y);
        }
        else if (!DoubleUtils.AreClose(newValue, oldValue)) {
            this.lastMouseMove = point;
        }
    }
}