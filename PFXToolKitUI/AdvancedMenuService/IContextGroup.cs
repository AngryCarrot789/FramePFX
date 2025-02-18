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

namespace PFXToolKitUI.AdvancedMenuService;

/// <summary>
/// An interface for any group in a context registry. So far there are fixed and dynamic groups.
/// <para>
/// Fixed groups, represented by <see cref="FixedContextGroup"/>, have fixed number of
/// <see cref="IContextObject"/> entries typically created before the context registry is fully initialised
/// </para>
/// <para>
/// Dynamic groups, represented by <see cref="DynamicContextGroup"/>, generate their entries
/// when required. Their generator callback is given the context available at generation, which
/// allows highly customisable menu options based on the available context and therefore states of
/// objects, rather than staying static and unchangable like <see cref="FixedContextGroup"/>
/// </para>
/// </summary>
public interface IContextGroup {
}