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

namespace FramePFX;

public enum ApplicationStartupPhase {
    /// <summary>
    /// The application is in its default state; the instance just exists but nothing else has happened
    /// </summary>
    Default,
    /// <summary>
    /// The application is in the pre-init stage, which is where services are being created
    /// </summary>
    PreInitialization,
    /// <summary>
    /// The application is initialising core components and is loading plugins
    /// </summary>
    Initializing,
    /// <summary>
    /// The application is fully initialised and is about to enter the running state
    /// </summary>
    FullyInitialized,
    /// <summary>
    /// The application is in its running state. No windows may be open once this state is reached
    /// </summary>
    Running,
    /// <summary>
    /// The application is in the process of shutting down (e.g. the last editor window was closed)
    /// </summary>
    Stopping,
    /// <summary>
    /// The application is stopped and the process will exit soon
    /// </summary>
    Stopped
}