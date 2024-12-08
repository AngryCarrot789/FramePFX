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

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FramePFX.DataTransfer;

namespace FramePFX.Avalonia.Bindings.Enums;

/// <summary>
/// A class which helps bind radio buttons to an enum data parameter
/// </summary>
public class DataParameterEnumBinder<TEnum> where TEnum : struct
{
    public delegate void DataParameterEnumBinderCurrentValueChangedEventHandler(DataParameterEnumBinder<TEnum> sender, TEnum oldCurrentValue, TEnum newCurrentValue);

    private readonly Dictionary<RadioButton, TEnum> buttonToState;
    private readonly List<RadioButton> defaultButtons;
    private readonly TEnum defaultEnum;
    private TEnum currentValue;

    public TEnum CurrentValue
    {
        get => this.currentValue;
        set
        {
            TEnum oldCurrentValue = this.currentValue;
            this.currentValue = value;
            this.CurrentValueChanged?.Invoke(this, oldCurrentValue, value);
            if (this.Owner != null)
            {
                this.Parameter.SetValue(this.Owner, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the active transferable data owner
    /// </summary>
    public ITransferableData? Owner { get; private set; }

    public DataParameter<TEnum> Parameter { get; }

    public event DataParameterEnumBinderCurrentValueChangedEventHandler? CurrentValueChanged;

    public DataParameterEnumBinder(DataParameter<TEnum> parameter, TEnum defaultValue = default)
    {
        this.buttonToState = new Dictionary<RadioButton, TEnum>();
        this.defaultButtons = new List<RadioButton>();
        this.currentValue = defaultValue;
        this.defaultEnum = defaultValue;
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    public void Assign(RadioButton button, TEnum enumValue)
    {
        // If now added then add event, otherwise, just update target enum value
        if (this.buttonToState.TryAdd(button, enumValue))
        {
            button.IsCheckedChanged += this.OnCheckChanged;
        }
        else
        {
            this.buttonToState[button] = enumValue;
        }

        if (enumValue.Equals(this.defaultEnum))
        {
            this.defaultButtons.Add(button);
        }
    }

    public void Unasign(RadioButton button)
    {
        if (this.buttonToState.Remove(button))
        {
            button.IsCheckedChanged -= this.OnCheckChanged;
            this.defaultButtons.Remove(button);
        }
    }

    private void OnCheckChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button && button.IsChecked == true)
            this.CurrentValue = this.buttonToState[button];
    }

    public void Attach(ITransferableData owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        this.Owner = owner;
        foreach (KeyValuePair<RadioButton, TEnum> pair in this.buttonToState)
        {
            if (pair.Key.IsChecked == true)
            {
                this.CurrentValue = pair.Value;
                return;
            }
        }

        if (this.defaultButtons.Count > 0)
        {
            foreach (RadioButton button in this.defaultButtons)
            {
                button.IsChecked = true;
            }
        }
    }

    public void Detach()
    {
        if (this.Owner != null)
        {
            this.Owner = null;
        }
    }
}