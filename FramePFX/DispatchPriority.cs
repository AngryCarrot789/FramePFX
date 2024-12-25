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

using System.ComponentModel;

namespace FramePFX;

public enum DispatchPriority : int {
    /// <summary>The lowest foreground dispatcher priority</summary>
    Default = 0,

    // INTERNAL_MinimumForegroundPriority = Default,
    /// <summary>
    /// The job will be processed with the same priority as input.
    /// </summary>
    Input = Default - 1,

    /// <summary>
    /// The job will be processed after other non-idle operations have completed.
    /// </summary>
    Background = Input - 1,

    /// <summary>
    /// The job will be processed after background operations have completed.
    /// </summary>
    ContextIdle = Background - 1,

    /// <summary>
    /// The job will be processed when the application is idle.
    /// </summary>
    ApplicationIdle = ContextIdle - 1,

    /// <summary>The job will be processed when the system is idle.</summary>
    SystemIdle = ApplicationIdle - 1,

    /// <summary>
    /// Minimum possible priority that's actually dispatched, default value
    /// </summary>
    INTERNAL_MinimumActiveValue = SystemIdle,

    /// <summary>
    /// A dispatcher priority for jobs that shouldn't be executed yet
    /// </summary>
    Inactive = INTERNAL_MinimumActiveValue - 1,

    /// <summary>Minimum valid priority</summary>
    INTERNAL_MinValue = Inactive,

    /// <summary>Used internally in dispatcher code</summary>
    Invalid = INTERNAL_MinimumActiveValue - 2,

    /// <summary>
    /// The job will be processed after layout and render but before input.
    /// </summary>
    Loaded = Default + 1,

    /// <summary>
    /// A special priority for platforms with UI render timer or for forced full rasterization requests
    /// </summary>
    INTERNAL_UiThreadRender = Loaded + 1,

    /// <summary>
    /// A special priority to synchronize native control host positions, IME, etc
    /// We should probably have a better API for that, so the priority is internal
    /// </summary>
    INTERNAL_AfterRender = INTERNAL_UiThreadRender + 1,

    /// <summary>
    /// The job will be processed with the same priority as render.
    /// </summary>
    Render = INTERNAL_AfterRender + 1,

    /// <summary>
    /// A special platform hook for jobs to be executed before the normal render cycle
    /// </summary>
    INTERNAL_BeforeRender = Render + 1,

    /// <summary>
    /// A special priority for platforms that resize the render target in asynchronous-ish matter,
    /// should be changed into event grouping in the platform backend render
    /// </summary>
    INTERNAL_AsyncRenderTargetResize = INTERNAL_BeforeRender + 1,

    /// <summary>
    /// The job will be processed with the same priority as data binding.
    /// </summary>
    [Obsolete("WPF compatibility")] [EditorBrowsable(EditorBrowsableState.Never)]
    DataBind = Render,

    /// <summary>The job will be processed with normal priority.</summary>
    Normal = Render + 1,

    /// <summary>
    /// The job will be processed before other asynchronous operations.
    /// </summary>
    Send = Normal + 1,
}