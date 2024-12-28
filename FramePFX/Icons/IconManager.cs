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

using FramePFX.Themes;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Icons;

/// <summary>
/// A class that manages a set of registered icons throughout the application. This is used to simply icon usage
/// </summary>
public abstract class IconManager {
    public static IconManager Instance => Application.Instance.ServiceManager.GetService<IconManager>();
    
    private readonly Dictionary<string, Icon> nameToIcon;

    protected IconManager() {
        this.nameToIcon = new Dictionary<string, Icon>();
    }

    protected void ValidateName(string name) {
        Validate.NotNullOrWhiteSpaces(name);
        if (this.nameToIcon.ContainsKey(name))
            throw new InvalidOperationException("Icon name already in use: '" + name + "'");
    }

    protected Icon RegisterCore(Icon icon) {
        this.ValidateName(icon.Name);
        this.AddIcon(icon.Name, icon);
        return icon;
    }
    
    public bool IconExists(string name) {
        Validate.NotNullOrWhiteSpaces(name);
        return this.nameToIcon.ContainsKey(name);
    }

    /// <summary>
    /// Gets an icon key from the name it was registered with
    /// </summary>
    /// <param name="name">The name of the icon</param>
    /// <returns>The icon key, if found</returns>
    public virtual Icon? GetIconFromName(string name) {
        Validate.NotNullOrWhiteSpaces(name);
        return this.nameToIcon.GetValueOrDefault(name);
    }

    /// <summary>
    /// Registers an icon that is based on an image on the disk somewhere
    /// </summary>
    /// <param name="name">A globally identifiable key for the icon</param>
    /// <param name="filePath"></param>
    /// <param name="lazilyLoad">True to load only on demand, False to load during the execution of this method (blocking)</param>
    /// <returns></returns>
    public abstract Icon RegisterIconByFilePath(string name, string filePath, bool lazilyLoad = true);

    public abstract Icon RegisterIconUsingBitmap(string name, SKBitmap bitmap);
    
    public abstract Icon RegisterGeometryIcon(string name, IColourBrush? brush, IColourBrush? stroke, string[] geometry, double strokeThickness = 0.0);

    /// <summary>
    /// Adds the icon key, with the given name. Throws if the name is
    /// invalid (null, empty or whitespaces) or an icon exists with the name already
    /// </summary>
    /// <param name="name">The icon name</param>
    /// <param name="key">The icon key</param>
    protected void AddIcon(string name, Icon key) {
        this.ValidateName(name);
        Validate.NotNull(key);
        
        this.nameToIcon.Add(name, key);
    }
}