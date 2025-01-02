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

namespace FramePFX.BaseFrontEnd.Windows;

/// <summary>
/// Manages windows and popup instances to support multiple windows and popups on any cross-platform,
/// even those that only support a single view (such as a raspberry PI server)
/// </summary>
public abstract class WindowManager {
    private readonly List<IWindow> mainWindows;
    private readonly List<IDialog> dialogs;
    private readonly Avalonia.Application app;
    
    public WindowManager() {
        this.app = Avalonia.Application.Current ?? throw new InvalidOperationException("No application initialised");
        this.mainWindows = new List<IWindow>();
    }

    /// <summary>
    /// Creates a window template
    /// </summary>
    /// <param name="window"></param>
    public abstract IWindow CreateWindow();
    
    /// <summary>
    /// Creates a dialog template
    /// </summary>
    /// <returns></returns>
    public abstract IDialog CreateDialog();
}