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

using System.Windows;

namespace FramePFX.Editors.Controls
{
    /// <summary>
    /// An interface that represents a UI component that follows the connection-disconnection pattern
    /// </summary>
    public interface IUIConnection<TParent, TModel> where TParent : DependencyObject where TModel : class
    {
        /// <summary>
        /// Gets the owner control for this object
        /// </summary>
        TParent Owner { get; }

        /// <summary>
        /// Gets the model control for this object
        /// </summary>
        TModel Model { get; }

        /// <summary>
        /// Connects this UI object to the given owner and model. This should be called after the control is
        /// </summary>
        /// <param name="owner">The connected parent</param>
        /// <param name="model">The connected model</param>
        void Connect(TParent owner, TModel model);

        /// <summary>
        /// Disconnects this UI object from its owner and model
        /// </summary>
        void Disconnect();
    }
}