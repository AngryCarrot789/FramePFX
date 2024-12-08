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

using Avalonia;
using Avalonia.Controls;

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// A helper class for storing a control reference and model reference
/// that automatically sets the property when possible
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
/// <typeparam name="TControl">Control type</typeparam>
public class PropertyAutoSetter<TModel, TControl> where TModel : class where TControl : Control
{
    public AvaloniaProperty<TModel?> Property { get; }

    public TControl? TargetControl { get; private set; }

    public TModel? Model { get; private set; }

    public PropertyAutoSetter(AvaloniaProperty<TModel?> property, TModel model = null)
    {
        this.Property = property;
        this.Model = model;
    }

    public void SetModel(TModel? model)
    {
        this.Model = model;
        if (this.TargetControl != null)
        {
            if (model != null)
            {
                this.TargetControl.SetValue(this.Property, model);
            }
            else
            {
                this.TargetControl.ClearValue(this.Property);
            }
        }
    }

    public void SetControl(TControl control)
    {
        if (this.TargetControl != null)
        {
            this.TargetControl.ClearValue(this.Property);
        }

        this.TargetControl = control;
        if (this.Model != null)
        {
            this.TargetControl.SetValue(this.Property, this.Model);
        }
    }
}