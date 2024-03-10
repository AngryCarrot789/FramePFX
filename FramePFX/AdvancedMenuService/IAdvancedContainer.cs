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
using FramePFX.AdvancedMenuService.ContextService;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.AdvancedMenuService
{
    /// <summary>
    /// An interface for an object that stores menu item entries. This could be a menu, context menu or a menu item
    /// </summary>
    public interface IAdvancedContainer
    {
        /// <summary>
        /// Gets the context for the container menu or root container menu item
        /// </summary>
        IContextData Context { get; }

        bool PushCachedItem(Type entryType, FrameworkElement element);

        FrameworkElement PopCachedItem(Type entryType);

        FrameworkElement CreateChildItem(IContextEntry entry);
    }
}