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

namespace FramePFX.Themes;

public delegate void DynamicColourBrushChangedEventHandler(IDynamicColourBrush brush);

public interface IDynamicColourBrush : IColourBrush {
    /// <summary>
    /// Gets the key that locates the "brush" contents
    /// </summary>
    string ThemeKey { get; }

    /// <summary>
    /// An event fired when the "brush" contents of this colour brush change (typically caused
    /// by the application theme changing, or maybe the user modified the specific brush)
    /// </summary>
    event DynamicColourBrushChangedEventHandler? BrushChanged;
}