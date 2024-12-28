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

using FramePFX.Persistence;
using SkiaSharp;

namespace FramePFX.Editing;

/// <summary>
/// A singleton object which contains properties that all editors share in common such as the titlebar prefix (for version control)
/// </summary>
public sealed class EditorConfigurationOptions : PersistentConfiguration {
    public static EditorConfigurationOptions Instance => Application.Instance.PersistentStorageManager.GetConfiguration<EditorConfigurationOptions>();
    
    public static readonly PersistentProperty<string> TitleBarPrefixProperty = PersistentProperty.RegisterString<EditorConfigurationOptions>(nameof(TitleBarPrefix), "Bootleg sony vegas (FramePFX v2.0.1)", x => x.titleBar, (x, y) => x.titleBar = y, true);
    public static readonly PersistentProperty<ulong> TitleBarBrushProperty = PersistentProperty.RegisterParsable<ulong, EditorConfigurationOptions>(nameof(TitleBarBrush), (ulong) SKColors.Red, x => (ulong) x.titleBarBrush, (x, y) => x.titleBarBrush = (SKColor) y, true);
    public static readonly PersistentProperty<bool> UseIconAntiAliasingProperty = PersistentProperty.RegisterBool<EditorConfigurationOptions>(nameof(UseIconAntiAliasing), false, x => x.useIconAntiAliasing, (x, y) => x.useIconAntiAliasing = y, false);
    
    private string titleBar = null!;
    private SKColor titleBarBrush;
    private bool useIconAntiAliasing;

    public string TitleBarPrefix {
        get => TitleBarPrefixProperty.GetValue(this);
        set => TitleBarPrefixProperty.SetValue(this, value);
    }

    /// <summary>
    /// Not assigned the actual titlebar default value for now, since this feature might
    /// not stick since it's wonky having this and no other customisation features
    /// </summary>
    public SKColor TitleBarBrush {
        get => (SKColor) TitleBarBrushProperty.GetValue(this);
        set => TitleBarBrushProperty.SetValue(this, (ulong) value);
    }
    
    public bool UseIconAntiAliasing {
        get => UseIconAntiAliasingProperty.GetValue(this);
        set => UseIconAntiAliasingProperty.SetValue(this, value);
    }

    public event PersistentPropertyInstanceValueChangeEventHandler<string>? TitleBarPrefixChanged {
        add => TitleBarPrefixProperty.AddValueChangeHandler(this, value);
        remove => TitleBarPrefixProperty.RemoveValueChangeHandler(this, value);
    }
    
    public event PersistentPropertyInstanceValueChangeEventHandler<ulong>? TitleBarBrushChanged {
        add => TitleBarBrushProperty.AddValueChangeHandler(this, value);
        remove => TitleBarBrushProperty.RemoveValueChangeHandler(this, value);
    }

    public EditorConfigurationOptions() {
    }
}