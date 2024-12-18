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

using SkiaSharp;

namespace FramePFX.Editing;

public delegate void EditorConfigurationOptionsTitleBarPrefixChangedEventHandler(EditorConfigurationOptions sender);
public delegate void EditorConfigurationOptionsTitleBarBrushChangedEventHandler(EditorConfigurationOptions sender);

/// <summary>
/// A singleton object which contains properties that all editors share in common such as the titlebar prefix (for version control)
/// </summary>
public sealed class EditorConfigurationOptions
{
    public static EditorConfigurationOptions Instance => Application.Instance.Services.GetService<EditorConfigurationOptions>();
    
    private string titleBar;
    private SKColor titleBarBrush;

    public string TitleBarPrefix
    {
        get => this.titleBar;
        set
        {
            if (this.titleBar == value)
                return;

            this.titleBar = value;
            this.TitleBarPrefixChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Not assigned the actual titlebar default value for now, since this feature might
    /// not stick since it's wonky having this and no other customisation features
    /// </summary>
    public SKColor TitleBarBrush
    {
        get => this.titleBarBrush;
        set
        {
            if (this.titleBarBrush == value)
                return;
        
            this.titleBarBrush = value;
            this.TitleBarBrushChanged?.Invoke(this);
        }
    }

    public event EditorConfigurationOptionsTitleBarBrushChangedEventHandler? TitleBarBrushChanged;

    public event EditorConfigurationOptionsTitleBarPrefixChangedEventHandler? TitleBarPrefixChanged;

    public EditorConfigurationOptions()
    {
        this.titleBar = "Bootleg song vegas (FramePFX v2.0.1)";
        this.TitleBarBrush = SKColors.Red;
    }
}