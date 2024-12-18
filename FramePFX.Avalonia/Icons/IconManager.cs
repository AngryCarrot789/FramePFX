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

namespace FramePFX.Avalonia.Icons;

/// <summary>
/// A class that manages a set of registered icons throughout the application. This is used to simply icon usage
/// </summary>
public class IconManager
{
    public IconKey RegisterIconByFilePath(string name, string filePath)
    {
        // Try to find an existing icon with the same file path. Share pixel data, maybe using a wrapper, because icons are lazily loaded
        return null;
    }
    
    public IconKey RegisterIconUsingBitmap(string name, SKBitmap bitmap)
    {
        // Try to find an existing icon with the same file path. Share pixel data, maybe using a wrapper, because icons are lazily loaded
        return null;
    }
}