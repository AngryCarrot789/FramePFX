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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FramePFX.Avalonia.AvControls;
using FramePFX.Avalonia.Utils;

namespace FramePFX.Avalonia.Interactivity;

/// <summary>
/// A control that contains a panel named 'PART_Panel' which contains multiple paths
/// </summary>
public class PanelSvgIconControl : TemplatedControl {
    private Panel? PART_Panel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Panel = e.NameScope.GetTemplateChild<Panel>(nameof(this.PART_Panel));
    }

    protected override Size ArrangeOverride(Size finalSize) {
        return PathHelper.Arrange(this, this.PART_Panel, finalSize, out Size arrange) ? arrange : base.ArrangeOverride(finalSize);
    }
}
