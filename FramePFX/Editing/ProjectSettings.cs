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

using PFXToolKitUI.Utils.Events;
using Rationals;
using SkiaSharp;

namespace FramePFX.Editing;

public sealed class ProjectSettings {
    public Project Project { get; }

    public SKSizeI Resolution {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ResolutionChanged);
    }

    public int Width {
        get => this.Resolution.Width;
        set => this.Resolution = new SKSizeI(value, this.Resolution.Height);
    }
    
    public int Height {
        get => this.Resolution.Height;
        set => this.Resolution = new SKSizeI(this.Resolution.Width, value);
    }
    
    public Rational FrameRate {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnFrameRateChanged(o, n));
    }

    public double FrameRateDouble { get; private set; }

    public event EventHandler<ValueChangedEventArgs<SKSizeI>>? ResolutionChanged;
    public event EventHandler<ValueChangedEventArgs<Rational>>? FrameRateChanged;

    public ProjectSettings(Project project) {
        this.Project = project;
        this.Width = 1280;
        this.Height = 720;
        this.FrameRate = new Rational(30, 1);
    }
    
    private void OnFrameRateChanged(Rational oldVal, Rational newVal) {
        this.FrameRateDouble = (double) newVal.Numerator / (double) newVal.Denominator;
        this.FrameRateChanged?.Invoke(this, new ValueChangedEventArgs<Rational>(oldVal, newVal));
    }
}