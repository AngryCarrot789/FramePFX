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

using FramePFX.AdvancedMenuService;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.AdvancedMenuService;

public interface IAdvancedContextElement
{
    IContextData? Context { get; }

    IAdvancedContainer? Container { get; }

    /// <summary>
    /// Stores the dynamic group for insertion at the given index inside this element's item
    /// list. This is a marker index, so post-processing must be done during generation
    /// </summary>
    /// <param name="group"></param>
    /// <param name="index"></param>
    void StoreDynamicGroup(DynamicGroupContextObject group, int index);
}