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

using System;
using Avalonia.Controls;
using FramePFX.AdvancedMenuService;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.AdvancedMenuService;

public interface IAdvancedContainer {
    /// <summary>
    /// Gets the context for the container menu or root container menu item
    /// </summary>
    IContextData? Context { get; }

    bool PushCachedItem(Type entryType, Control element);

    Control? PopCachedItem(Type entryType);

    Control CreateChildItem(IContextObject entry);

    void UpdateSubListVisibility();
}