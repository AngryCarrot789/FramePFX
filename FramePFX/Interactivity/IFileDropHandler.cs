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

namespace FramePFX.Interactivity;

public interface IFileDropNotifier {
    /// <summary>
    /// Ges the allow drop type(s) from the given dragged paths and input allowed drop types
    /// </summary>
    /// <param name="paths">The paths being dragged (non-null and has at least 1 entry)</param>
    /// <param name="dropType">Drop type the user wants to perform</param>
    /// <returns></returns>
    EnumDropType GetFileDropType(string[] paths, EnumDropType dropType);

    Task OnFilesDropped(string[] paths, EnumDropType dropType);
}