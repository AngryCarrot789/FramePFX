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

using System.Collections.ObjectModel;

namespace FramePFX.Editing.Toolbars;

/// <summary>
/// Manages the toolbar that is below the timeline itself
/// </summary>
public abstract class TimelineToolBarManager {
    public static TimelineToolBarManager Instance => Application.Instance.ServiceManager.GetService<TimelineToolBarManager>();

    /// <summary>
    /// Gets the toolbar buttons that are docked to the west (left) side of the toolbar
    /// </summary>
    public ObservableCollection<ToolbarButton> WestButtons { get; }
    
    /// <summary>
    /// Gets the toolbar buttons that are docked to the east (right) side of the toolbar
    /// </summary>
    public ObservableCollection<ToolbarButton> EastButtons { get; }

    public TimelineToolBarManager() {
        this.WestButtons = new ObservableCollection<ToolbarButton>();
        this.EastButtons = new ObservableCollection<ToolbarButton>();
    }
}