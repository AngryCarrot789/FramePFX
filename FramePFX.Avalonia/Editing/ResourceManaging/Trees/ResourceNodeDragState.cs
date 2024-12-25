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

namespace FramePFX.Avalonia.Editing.ResourceManaging.Trees;

public enum ResourceNodeDragState {
    // No drag drop has been started yet
    None = 0,

    // User left-clicked, so wait for enough move movement
    Initiated = 1,

    // User moved their mouse enough. DragDrop is running
    Active = 2,

    // Layer dropped, this is used to ensure we don't restart when the mouse moves
    // again e.g. if they right-click (which win32 takes as cancelling the drop) but
    // the user keeps left mouse pressed
    Completed = 3
}