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

namespace PFXToolKitUI.DataTransfer;

[Flags]
public enum DataParameterFlags {
    /// <summary>
    /// This data parameter does nothing special on its own
    /// </summary>
    None = 0,

    /// <summary>
    /// The data parameter invalidates the state of the currently rendered frame, causing a re-render to be required to be up to date
    /// </summary>
    AffectsRender = 1,

    /// <summary>
    /// The modification of this parameter value modifies the project in such a way that a save is required to be up to date with the file
    /// </summary>
    ModifiesProject = 2,

    /// <summary>
    /// A flag which combines <see cref="AffectsRender"/> and <see cref="ModifiesProject"/>
    /// </summary>
    StandardProjectVisual = AffectsRender | ModifiesProject
}