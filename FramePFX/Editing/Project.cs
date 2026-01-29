// 
// Copyright (c) 2026-2026 REghZy
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

using System.Numerics;
using PFXToolKitUI.Utils.Events;
using Rationals;

namespace FramePFX.Editing;

/// <summary>
/// Represents a FramePFX project
/// </summary>
public sealed class Project {
    public VideoEditor? VideoEditor {
        get => field;
        internal set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.VideoEditorChanged);
    }

    /// <summary>
    /// Gets or sets the path of the project. Projects are directory-based
    /// </summary>
    public string? ProjectDirectory {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ProjectDirectoryChanged);
    }

    /// <summary>
    /// Gets or sets the project frame rate
    /// </summary>
    public Rational FrameRate {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.FrameRateChanged);
    } = new Rational(new BigInteger(60), new BigInteger(1));
    
    /// <summary>
    /// Gets the main timeline associated with this project
    /// </summary>
    public Timeline MainTimeline { get; }

    public event EventHandler<ValueChangedEventArgs<VideoEditor?>>? VideoEditorChanged;
    public event EventHandler? ProjectDirectoryChanged;
    public event EventHandler? FrameRateChanged;

    public Project() {
        this.MainTimeline = new Timeline() { Project = this };
    }
}