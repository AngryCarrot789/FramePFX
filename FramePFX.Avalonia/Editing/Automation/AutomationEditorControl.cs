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

using Avalonia;
using Avalonia.Controls;
using FramePFX.Editing.Automation.Keyframes;

namespace FramePFX.Avalonia.Editing.Automation;

public class AutomationEditorControl : Control {
    public const double EllipseRadius = 2.5d;
    public const double EllipseThickness = 1d;
    public const double EllipseHitRadius = 12d;
    public const double LineThickness = 2d;
    public const double LineHitThickness = 12d;
    public const double MaximumFloatingPointRange = 10000;
    
    public static readonly StyledProperty<double> HorizontalZoomProperty = AvaloniaProperty.Register<AutomationEditorControl, double>(nameof(HorizontalZoom));
    public static readonly StyledProperty<AutomationSequence?> AutomationSequenceProperty = AvaloniaProperty.Register<AutomationEditorControl, AutomationSequence?>(nameof(AutomationSequence));

    public double HorizontalZoom {
        get => this.GetValue(HorizontalZoomProperty);
        set => this.SetValue(HorizontalZoomProperty, value);
    }

    public AutomationSequence? AutomationSequence {
        get => this.GetValue(AutomationSequenceProperty);
        set => this.SetValue(AutomationSequenceProperty, value);
    }

    public AutomationEditorControl() {
        this.IsHitTestVisible = true;
    }
}