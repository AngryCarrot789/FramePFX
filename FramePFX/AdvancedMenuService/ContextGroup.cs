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

namespace FramePFX.AdvancedMenuService;

/// <summary>
/// A group of context items, not a sub-list
/// </summary>
public class ContextGroup {
    private readonly List<IContextObject> items;
    
    /// <summary>
    /// Gets our items
    /// </summary>
    public IReadOnlyList<IContextObject> Items => this.items;

    public ContextGroup() {
        this.items = new List<IContextObject>();
    }

    public void AddEntry(IContextObject item) {
        this.items.Add(item);
    }

    public void AddSeparator() => this.AddEntry(new SeparatorEntry());

    public void AddCommand(string cmdId, string displayName, string? description = null) {
        this.AddEntry(new CommandContextEntry(displayName, description, cmdId));
    }
}