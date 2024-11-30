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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Editing;

/// <summary>
/// A bi-dictionary that maps a model to a control and vice versa.
/// This data structure does not support multiple models per control nor vice versa
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
/// <typeparam name="TControl">Control type</typeparam>
public class ModelControlDictionary<TModel, TControl> : IModelControlDictionary<TModel, TControl> where TModel : class where TControl : class {
    private Dictionary<TModel, TControl>? modelToControl;
    private Dictionary<TControl, TModel>? controlToModel;

    public IEnumerable<TModel> Models => this.modelToControl?.Keys ?? Enumerable.Empty<TModel>();
    public IEnumerable<TControl> Controls => this.controlToModel?.Keys ?? Enumerable.Empty<TControl>();
    
    public IEnumerable<KeyValuePair<TModel, TControl>> Entries => this.modelToControl ?? Enumerable.Empty<KeyValuePair<TModel, TControl>>();

    public ModelControlDictionary() {
    }

    public void AddMapping(TModel model, TControl control) {
        Validate.NotNull(model);
        Validate.NotNull(control);
        
        if (this.controlToModel == null) {
            this.modelToControl = new Dictionary<TModel, TControl>();
            this.controlToModel = new Dictionary<TControl, TModel>();
        }
        else if (this.controlToModel!.ContainsKey(control)) {
            throw new InvalidOperationException("Attempt to add the same control twice");
        }
        else if (this.modelToControl!.ContainsKey(model)) {
            throw new InvalidOperationException("Attempt to add the same model twice");
        }

        this.modelToControl.Add(model, control);
        this.controlToModel.Add(control, model);
    }

    private void CheckMappingInternal(TModel model, TControl control) {
        Validate.NotNull(model);
        Validate.NotNull(control);
        if (this.controlToModel == null)
            throw new InvalidOperationException("Attempt to remove control that was never added (internal collections are empty)");
        
        if (!this.controlToModel.ContainsKey(control))
            throw new InvalidOperationException("Attempt to remove control that was never added");
    }
    
    public void CheckMapping(TModel model, TControl control) {
        this.CheckMappingInternal(model, control);
        if (!this.modelToControl!.ContainsKey(model))
            throw new InvalidOperationException("Attempt to remove model that was never added");
    }

    public void RemoveMapping(TModel model, TControl control) {
        this.CheckMappingInternal(model, control);
        if (!this.modelToControl!.Remove(model))
            throw new InvalidOperationException("Attempt to remove model that was never added");

        // Only remove control if the model was removed successfully, in case we goof up.
        // Not like it matters anyway since the exception will crash the app lmfao
        // Best to handle things as appropriately as possible before the universe explodes
        this.controlToModel!.Remove(control);
    }

    public void Clear() {
        this.modelToControl?.Clear();
        this.controlToModel?.Clear();
    }

    public TControl GetControl(TModel model) => this.TryGetControl(model, out TControl? control) ? control : throw new InvalidOperationException("Model not added");
    public TModel GetModel(TControl control) => this.TryGetModel(control, out TModel? model) ? model : throw new InvalidOperationException("Control not added");

    public bool TryGetControl(TModel model, [NotNullWhen(true)] out TControl? control) {
        Validate.NotNull(model);
        if (this.modelToControl == null)
            return (control = null) != null; // sexy one liner ;)
        return this.modelToControl.TryGetValue(model, out control);
    }

    public bool TryGetModel(TControl control, [NotNullWhen(true)] out TModel? model) {
        Validate.NotNull(control);
        return this.controlToModel == null ? (model = null) != null : this.controlToModel.TryGetValue(control, out model);
    }
}