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

using FramePFX.CommandSystem;
using FramePFX.Configurations.UI;
using FramePFX.Services.UserInputs;
using FramePFX.Themes.Configurations;

namespace FramePFX.Themes.Commands;

public class CreateThemeCommand : AsyncCommand {
    public bool CopyKeys { get; }

    public CreateThemeCommand(bool copyKeys) {
        this.CopyKeys = copyKeys;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!IThemeConfigurationTreeElement.TreeElementKey.TryGetContext(e.ContextData, out IThemeConfigurationTreeElement? tree)) {
            return;
        }

        if (!(tree.ThemeConfigurationPage is ThemeConfigurationPage page)) {
            return;
        }

        Theme? theme = page.TargetTheme;
        if (theme == null) {
            return;
        }

        ThemeManager manager = theme.ThemeManager;
        SingleUserInputInfo info = new SingleUserInputInfo("Create a new theme", "What do you want the theme to be called?", theme.Name + " (copy)") {
            Validator = s => {
                if (string.IsNullOrWhiteSpace(s))
                    return "Theme name cannot be an empty string or consist of only whitespaces";
                
                if (manager.GetTheme(s) != null)
                    return "Theme already exists with this name";

                return null;
            },
            Footer = "*This will not change the original theme, but will apply the current colours to the copied theme"
        };
        
        if (await IUserInputDialogService.Instance.ShowInputDialogAsync(info) != true) {
            return;
        }

        if (theme != page.TargetTheme) {
            // ...
            return;
        }
        
        Theme newTheme = theme.ThemeManager.RegisterTheme(info.Text!, theme, this.CopyKeys);
        page.ApplyAndRevertChanges(theme, newTheme);
        
        theme.ThemeManager.SetTheme(newTheme);
        page.TargetTheme = newTheme;
    }
}