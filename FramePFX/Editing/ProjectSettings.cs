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

using FramePFX.Editing.Utils;
using FramePFX.Utils.RBC;
using SkiaSharp;

namespace FramePFX.Editing;

public delegate void ProjectSettingsEventHandler(ProjectSettings settings);

public class ProjectSettings {
    private SKSizeI resolution;
    private Rational frameRate;

    public SKSizeI Resolution {
        get => this.resolution;
        set {
            if (this.resolution == value)
                return;
            this.resolution = value;
            this.ResolutionChanged?.Invoke(this);
        }
    }

    public int Width => this.resolution.Width;

    public int Height => this.resolution.Height;

    public Rational FrameRate {
        get => this.frameRate;
        set {
            if (this.frameRate == value)
                return;
            this.frameRate = value;
            this.FrameRateChanged?.Invoke(this);
        }
    }

    public int SampleRate = 44100;
    public int BufferSize = 2048;

    public event ProjectSettingsEventHandler? ResolutionChanged;
    public event ProjectSettingsEventHandler? FrameRateChanged;

    /// <summary>
    /// Gets the project associated with these settings
    /// </summary>
    public Project Project { get; }

    public ProjectSettings(Project project, int width, int height, Rational frameRate) : this(project) {
        this.resolution = new SKSizeI(width, height);
        this.frameRate = frameRate;
    }

    public ProjectSettings(Project project) {
        this.Project = project ?? throw new ArgumentNullException(nameof(project));
    }

    public static ProjectSettings CreateDefault(Project project) {
        return new ProjectSettings(project, 1920, 1080, new Rational(60, 1));
    }

    public ProjectSettings Clone(Project project = null) {
        ProjectSettings settings = new ProjectSettings(project ?? this.Project);
        RBEDictionary dictionary = new RBEDictionary();
        this.WriteToRBE(dictionary);
        settings.ReadFromRBE(dictionary);
        return settings;
    }

    public void WriteToRBE(RBEDictionary dictionary) {
        dictionary.SetULong(nameof(this.FrameRate), (ulong) this.frameRate);
        dictionary.SetULong(nameof(this.Resolution), this.resolution.ToLong());
    }

    public void ReadFromRBE(RBEDictionary dictionary) {
        this.frameRate = (Rational) dictionary.GetULong(nameof(this.FrameRate));
        this.resolution = SKUtils.Long2SizeI(dictionary.GetULong(nameof(this.Resolution)));
    }

    public void WriteInto(ProjectSettings settings) {
        settings.Resolution = this.Resolution;
        settings.FrameRate = this.FrameRate;
    }
}