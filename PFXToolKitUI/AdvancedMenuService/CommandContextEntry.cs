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

using PFXToolKitUI.Icons;

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// A context entry that executes a <see cref="CommandSystem.Command"/>
/// </summary>
public class CommandContextEntry : BaseContextEntry {
    public string CommandId { get; }

    public CommandContextEntry(string commandId, string displayName, string? description = null, Icon? icon = null, StretchMode stretchMode = StretchMode.None) : base(displayName, description, icon, stretchMode) {
        this.CommandId = commandId;
    }
}