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

using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia;

public interface IFrontEndApplication {
    public static IFrontEndApplication Instance => (IFrontEndApplication) Application.Instance;

    // TODO: abstract this away
    // this is used for UserInputDialog and a few other things, but we should instead
    // write an abstraction around a popup dialog that just shows content on top of the main view

    /// <summary>
    /// Tries to get the window that is currently activated (focused), falling back to the main window if allowed
    /// </summary>
    /// <param name="window">The found window</param>
    /// <param name="fallbackToMainWindow">True to use the main window if no focused window is found</param>
    /// <returns>True if a window was found</returns>
    bool TryGetActiveWindow([NotNullWhen(true)] out Window? window, bool fallbackToMainWindow = true);
}