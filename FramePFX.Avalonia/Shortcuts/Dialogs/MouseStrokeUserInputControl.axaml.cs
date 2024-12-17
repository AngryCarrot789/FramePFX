// 
// Copyright (c) 2024-2024 REghZy
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

using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Services.Messages.Controls;
using FramePFX.Avalonia.Shortcuts.Avalonia;
using FramePFX.Avalonia.Shortcuts.Converters;
using FramePFX.Services.InputStrokes;
using FramePFX.Services.UserInputs;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Avalonia.Shortcuts.Dialogs;

public partial class MouseStrokeUserInputControl : UserControl, IUserInputContent
{
    public MouseStrokeUserInputInfo? InputInfo { get; private set; }
    
    private readonly IBinder<MouseStrokeUserInputInfo> mouseStrokeBinder = new DataParameterPropertyBinder<MouseStrokeUserInputInfo>(TextBox.TextProperty, MouseStrokeUserInputInfo.MouseStrokeParameter, (p) =>
    {
        MouseStroke? stroke = (MouseStroke?) p;
        if (!stroke.HasValue || stroke.Value == default)
        {
            return "";
        }
        else
        {
            MouseStroke s = stroke.Value;
            return MouseStrokeStringConverter.ToStringFunction(s.MouseButton, s.Modifiers, s.ClickCount);
        }
    });
    
    public MouseStrokeUserInputControl()
    {
        InitializeComponent();
        this.mouseStrokeBinder.AttachControl(this.InputBox);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MouseStroke stroke = ShortcutUtils.GetMouseStrokeForEvent(e);
        this.InputInfo!.MouseStroke = stroke;
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ShortcutUtils.GetMouseStrokeForEvent(e, out MouseStroke stroke)) {
            this.InputInfo!.MouseStroke = stroke;
        }
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info)
    {
        this.InputInfo = (MouseStrokeUserInputInfo) info;
        this.mouseStrokeBinder.AttachModel(this.InputInfo);
    }

    public void Disconnect()
    {
        this.InputInfo = null;
        this.mouseStrokeBinder.DetachModel();
    }

    public bool FocusPrimaryInput()
    {
        this.InputBox.Focus();
        return true;
    }
}