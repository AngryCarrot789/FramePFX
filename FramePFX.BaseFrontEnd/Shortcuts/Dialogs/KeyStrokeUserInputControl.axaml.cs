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
using Avalonia.Interactivity;
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Services.UserInputs;
using FramePFX.BaseFrontEnd.Shortcuts.Avalonia;
using FramePFX.BaseFrontEnd.Shortcuts.Converters;
using FramePFX.Services.InputStrokes;
using FramePFX.Services.UserInputs;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.BaseFrontEnd.Shortcuts.Dialogs;

public partial class KeyStrokeUserInputControl : UserControl, IUserInputContent {
    public KeyStrokeUserInputInfo? InputInfo { get; private set; }

    private readonly IBinder<KeyStrokeUserInputInfo> keyStrokeBinder = new DataParameterPropertyBinder<KeyStrokeUserInputInfo>(TextBox.TextProperty, KeyStrokeUserInputInfo.KeyStrokeParameter, (p) => {
        KeyStroke s = (KeyStroke?) p ?? default;
        return KeyStrokeStringConverter.ToStringFunction(s.KeyCode, s.Modifiers, s.IsRelease, false, true);
    }) {
        CanUpdateModel = false
    };

    private UserInputDialog? myDialog;

    public KeyStrokeUserInputControl() {
        this.InitializeComponent();
        this.keyStrokeBinder.AttachControl(this.InputBox);
        this.InputBox.AddHandler(TextBox.KeyDownEvent, this.InputBox_KeyDown, RoutingStrategies.Tunnel);
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e) {
        if (this.InputInfo == null)
            return;

        KeyStroke s = this.InputInfo!.KeyStroke ?? default;
        this.InputInfo!.KeyStroke = new KeyStroke(s.KeyCode, s.Modifiers, this.IsKeyReleaseCheckBox.IsChecked ?? false);
        this.myDialog!.UpdateAllErrors();
    }

    private void InputBox_KeyDown(object? sender, KeyEventArgs e) {
        if (ShortcutUtils.GetKeyStrokeForEvent(e, out KeyStroke stroke, this.IsKeyReleaseCheckBox.IsChecked ?? false)) {
            this.InputInfo!.KeyStroke = stroke;
            this.myDialog!.UpdateAllErrors();
            e.Handled = true;
        }
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info) {
        this.InputInfo = (KeyStrokeUserInputInfo) info;
        this.myDialog = dialog;
        this.keyStrokeBinder.AttachModel(this.InputInfo);
    }

    public void Disconnect() {
        this.keyStrokeBinder.DetachModel();
        this.InputInfo = null;
        this.myDialog = null;
    }

    public bool FocusPrimaryInput() {
        this.InputBox.Focus();
        return true;
    }
}