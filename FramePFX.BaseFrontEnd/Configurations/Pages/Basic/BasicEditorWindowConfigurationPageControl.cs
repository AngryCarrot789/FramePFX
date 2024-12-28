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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Configurations.Basic;

namespace FramePFX.BaseFrontEnd.Configurations.Pages.Basic;

public class BasicEditorWindowConfigurationPageControl : BaseConfigurationPageControl {
    private TextBox? editorWindowTitleTextBox;
    private CheckBox? toggleUseAliasingCheckBox;
    private readonly IBinder<EditorWindowConfigurationPage> titleBarBinder;
    private readonly IBinder<EditorWindowConfigurationPage> useIconAntiAliasingBinder;

    public BasicEditorWindowConfigurationPageControl() {
        this.titleBarBinder = new AutoUpdateAndEventPropertyBinder<EditorWindowConfigurationPage>(TextBox.TextProperty, nameof(EditorWindowConfigurationPage.TitleBarChanged), obj => obj.Control.SetValue(TextBox.TextProperty, obj.Model.TitleBar), obj => obj.Model.TitleBar = obj.Control.GetValue(TextBox.TextProperty) ?? "");
        this.useIconAntiAliasingBinder = new GetSetAutoUpdateAndEventPropertyBinder<EditorWindowConfigurationPage>(ToggleButton.IsCheckedProperty, nameof(EditorWindowConfigurationPage.UseIconAntiAliasingChanged), b => b.Model.UseIconAntiAliasing, (b, v) => b.Model.UseIconAntiAliasing = (bool?) v == true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.editorWindowTitleTextBox = e.NameScope.GetTemplateChild<TextBox>("PART_TitleBarTextBox");
        this.toggleUseAliasingCheckBox = e.NameScope.GetTemplateChild<CheckBox>("PART_ToggleUseAliasing");
        this.titleBarBinder.AttachControl(this.editorWindowTitleTextBox);
        this.useIconAntiAliasingBinder.AttachControl(this.toggleUseAliasingCheckBox);
    }

    public override void OnConnected() {
        base.OnConnected();
        this.titleBarBinder.AttachModel((EditorWindowConfigurationPage) this.Page!);
        this.useIconAntiAliasingBinder.AttachModel((EditorWindowConfigurationPage) this.Page!);
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.titleBarBinder.DetachModel();
        this.useIconAntiAliasingBinder.DetachModel();
    }
}