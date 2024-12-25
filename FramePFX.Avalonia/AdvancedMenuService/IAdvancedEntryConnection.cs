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

using Avalonia.Controls;
using FramePFX.AdvancedMenuService;

namespace FramePFX.Avalonia.AdvancedMenuService;

/// <summary>
/// An interface for some sort of object inside an advanced menu that
/// can be connected to and from a context menu entry
/// </summary>
public interface IAdvancedEntryConnection {
    IContextObject? Entry { get; }

    void OnAdding(IAdvancedContainer container, ItemsControl parent, IContextObject entry);
    void OnAdded();
    void OnRemoving();
    void OnRemoved();
}