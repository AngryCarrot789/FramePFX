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

using FramePFX.CommandSystem;
using FramePFX.Editing.Timelines;
using FramePFX.Interactivity.Contexts;
using FramePFX.Toolbars;
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.Editing.Toolbars;

/// <summary>
/// Manages the toolbar that is below the timeline itself
/// </summary>
public sealed class ControlSurfaceListToolBarManager : BaseToolBarManager {
    /// <summary>
    /// Gets the toolbar buttons that are docked to the west (left) side of the toolbar
    /// </summary>
    public ObservableList<ToolBarButton> Buttons { get; }

    public ControlSurfaceListToolBarManager() {
        this.Buttons = new ObservableList<ToolBarButton>();
        
        // Setup standard buttons
        this.Buttons.Add(new AddTrackToolBarButton() { Button = { ToolTip = "Adds a new video track" } });
    }

    public static ControlSurfaceListToolBarManager GetInstance(VideoEditor editor) {
        return editor.ServiceManager.GetService<ControlSurfaceListToolBarManager>();
    }

    private class AddTrackToolBarButton : SimpleCommandToolBarButton {
        public AddTrackToolBarButton() : base("commands.editor.CreateVideoTrack", ToolbarButtonFactory.Instance.CreateButton()) {
            this.Icon = SimpleIcons.VideoIcon;
        }
    }
}