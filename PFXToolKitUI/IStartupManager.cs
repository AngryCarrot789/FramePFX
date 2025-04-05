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

namespace PFXToolKitUI;

/// <summary>
/// The startup handler for the application
/// </summary>
public interface IStartupManager {
    public static IStartupManager Instance => ApplicationPFX.Instance.ServiceManager.GetService<IStartupManager>();
    
    /// <summary>
    /// Invoked once the application is fully loaded, all UI components are ready.
    /// This method takes the command line arguments which may contain a file path if
    /// the app was opened via a file. It will not include application .exe path
    /// </summary>
    /// <param name="args">Command line args, excluding app exe</param>
    Task OnApplicationStartupWithArgs(string[] args);
}