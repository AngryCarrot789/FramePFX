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
using FramePFX.Editing;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Configurations.Commands;

public class OpenProjectSettingsCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!DataKeys.VideoEditorUIKey.TryGetContext(e.ContextData, out IVideoEditorWindow? editor))
            return Executability.Invalid;

        return editor.VideoEditor?.Project != null ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!DataKeys.VideoEditorUIKey.TryGetContext(e.ContextData, out IVideoEditorWindow? editorUI))
            return;

        if (editorUI.VideoEditor?.Project is Project project) {
            ProjectConfigurationManager config = ProjectConfigurationManager.GetInstance(project, editorUI);
            await IConfigurationDialogService.Instance.ShowConfigurationDialog(config);
            editorUI.TimelineElement.Timeline!.InvalidateRender();
        }
    }
}