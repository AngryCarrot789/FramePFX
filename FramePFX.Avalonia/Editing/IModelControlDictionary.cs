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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Avalonia.Editing;

public interface IModelControlDictionary<TModel, TControl> where TModel : class where TControl : class {
    /// <summary>
    /// Gets all of the models in this dictionary
    /// </summary>
    public IEnumerable<TModel> Models { get; }

    /// <summary>
    /// Gets all of the controls in this dictionary
    /// </summary>
    public IEnumerable<TControl> Controls { get; }

    /// <summary>
    /// Gets the control-model entries
    /// </summary>
    public IEnumerable<KeyValuePair<TModel, TControl>> Entries { get; }

    /// <summary>
    /// Gets the control mapped to the model
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The control mapping</returns>
    public TControl GetControl(TModel model);

    /// <summary>
    /// Gets the model mapped to the control
    /// </summary>
    /// <param name="control">The control</param>
    /// <returns>The model mapping</returns>
    public TModel GetModel(TControl control);

    /// <summary>
    /// Tries to get the control mapped to the model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="control">The control mapping, if it exists</param>
    /// <returns>Success when the mapping was found</returns>
    public bool TryGetControl(TModel model, [NotNullWhen(true)] out TControl? control);

    /// <summary>
    /// Tries to get the model mapped to the control
    /// </summary>
    /// <param name="control">The control</param>
    /// <param name="model">The model mapping, if it exists</param>
    /// <returns>Success when the mapping was found</returns>
    public bool TryGetModel(TControl control, [NotNullWhen(true)] out TModel? model);
}