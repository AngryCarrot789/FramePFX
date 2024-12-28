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

using FramePFX.Interactivity.Contexts;

namespace FramePFX.Configurations.UI;

/// <summary>
/// A shortcut tree UI control
/// </summary>
public interface IShortcutTreeElement {
    public static DataKey<IShortcutTreeElement> TreeElementKey { get; } = DataKey<IShortcutTreeElement>.Create("ShortcutTreeElement");

    /// <summary>
    /// Expands the entire tree
    /// </summary>
    void ExpandAll();
    
    /// <summary>
    /// Collapses the entire tree
    /// </summary>
    void CollapseAll();
}