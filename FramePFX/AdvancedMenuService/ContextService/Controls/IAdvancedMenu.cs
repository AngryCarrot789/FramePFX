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

using System;
using System.Windows;

namespace FramePFX.AdvancedMenuService.ContextService.Controls
{
    /// <summary>
    /// An interface for an advanced context menu or advanced menu
    /// </summary>
    public interface IAdvancedMenu
    {
        bool PushCachedItem(Type entryType, FrameworkElement element);

        FrameworkElement PopCachedItem(Type entryType);

        FrameworkElement CreateChildItem(IContextEntry entry);
    }
}