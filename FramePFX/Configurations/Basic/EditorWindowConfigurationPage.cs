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

using FramePFX.Editing;

namespace FramePFX.Configurations.Basic;

public delegate void EditorWindowConfigurationPageTitleBarChangedEventHandler(EditorWindowConfigurationPage sender);

public class EditorWindowConfigurationPage : ConfigurationPage {
    private string? titleBar;

    public string? TitleBar {
        get => this.titleBar;
        set {
            if (this.titleBar == value)
                return;

            this.titleBar = value;
            this.TitleBarChanged?.Invoke(this);
            this.MarkModified();
        }
    }

    public event EditorWindowConfigurationPageTitleBarChangedEventHandler? TitleBarChanged;

    public EditorWindowConfigurationPage() {
    }

    public override async ValueTask OnContextCreated(ConfigurationContext context) {
        EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
        this.titleBar = options.TitleBarPrefix;
    }

    public override async ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
        if (!string.IsNullOrWhiteSpace(this.titleBar)) {
            options.TitleBarPrefix = this.titleBar;
        }

        // await IoC.MessageService.ShowMessage("Change title", "Change window title to: " + this.TitleBar);
    }
}