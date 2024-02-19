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

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// The base interface for all context entries. Currently, this is only used for menu items and separators
    /// <para>
    /// Instead of view models containing a list of context menu item entries and then just dynamically
    /// updating each one when required (which would be really annoying to do), instances of these entries are
    /// instead created on-demand and their state is setup when created (with optional bindable properties to further
    /// update the state of the entry). And then, a generator can be used to generate the items
    /// </para>
    /// </summary>
    public interface IContextEntry {
    }
}