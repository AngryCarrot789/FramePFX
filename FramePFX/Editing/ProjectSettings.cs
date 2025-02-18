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

using Fractions;
using FramePFX.Editing.Utils;
using FramePFX.Utils.BTE;
using SkiaSharp;

namespace FramePFX.Editing;

public delegate void ProjectSettingsEventHandler(ProjectSettings settings);

public class ProjectSettings {
    private SKSizeI resolution;
    private Fraction myFrameRate;

    private Fraction InternalFrameRate {
        get => this.myFrameRate;
        set {
            this.frameRateDouble = null;
            this.myFrameRate = value;
        }
    }

    private double? frameRateDouble;

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

    public Fraction FrameRate {
        get => this.myFrameRate;
        set {
            if (this.myFrameRate != value) {
                this.InternalFrameRate = value;
                this.FrameRateChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets a cached double-value of the frame rate
    /// </summary>
    public double FrameRateDouble => this.frameRateDouble ??= this.FrameRate.ToDouble();

    public int SampleRate = 44100;
    public int BufferSize = 2048;

    public event ProjectSettingsEventHandler? ResolutionChanged;
    public event ProjectSettingsEventHandler? FrameRateChanged;

    /// <summary>
    /// Gets the project associated with these settings
    /// </summary>
    public Project Project { get; }

    public ProjectSettings(Project project, int width, int height, Fraction myFrameRate) : this(project) {
        this.resolution = new SKSizeI(width, height);
        this.myFrameRate = myFrameRate;
    }

    public ProjectSettings(Project project) {
        this.Project = project ?? throw new ArgumentNullException(nameof(project));
    }

    public static ProjectSettings CreateDefault(Project project) {
        return new ProjectSettings(project, 1920, 1080, new Fraction(60, 1));
    }

    public void WriteToBTE(BTEDictionary dictionary) {
        this.FrameRate.Serialise(nameof(this.FrameRate), dictionary);
        dictionary.SetULong(nameof(this.Resolution), this.resolution.ToLong());
    }

    public void ReadFromBTE(BTEDictionary dictionary) {
        this.InternalFrameRate = FractionUtils.DeserialiseFraction(dictionary, nameof(this.FrameRate));
        this.resolution = SKUtils.Long2SizeI(dictionary.GetULong(nameof(this.Resolution)));
    }

    public void WriteInto(ProjectSettings settings) {
        settings.Resolution = this.Resolution;
        settings.FrameRate = this.FrameRate;
    }
}