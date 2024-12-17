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

public class OpenProjectSettingsCommand : AsyncCommand
{
    protected override Executability CanExecuteOverride(CommandEventArgs e)
    {
        return DataKeys.ProjectKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteAsync(CommandEventArgs e)
    {
        if (!DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project? project))
            return;

        await IoC.ConfigurationService.ShowConfigurationDialog(ProjectConfigurationManager.GetProjectConfigurationManager(project));
        
        if (DataKeys.VideoEditorUIKey.TryGetContext(e.ContextData, out IVideoEditorUI? editor))
            editor.TimelineElement.Timeline!.InvalidateRender();
    }
}