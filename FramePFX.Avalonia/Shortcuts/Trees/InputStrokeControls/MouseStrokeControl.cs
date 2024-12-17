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
using FramePFX.Avalonia.Shortcuts.Converters;
using FramePFX.Avalonia.Utils;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Avalonia.Shortcuts.Trees.InputStrokeControls;

public class MouseStrokeControl : TemplatedControl
{
    public static readonly StyledProperty<MouseStroke?> MouseStrokeProperty = AvaloniaProperty.Register<MouseStrokeControl, MouseStroke?>(nameof(MouseStroke));

    public MouseStroke? MouseStroke
    {
        get => this.GetValue(MouseStrokeProperty);
        set => this.SetValue(MouseStrokeProperty, value);
    }

    private TextBlock? PART_TextBlock;
    
    public MouseStrokeControl()
    {
    }

    static MouseStrokeControl()
    {
        MouseStrokeProperty.Changed.AddClassHandler<MouseStrokeControl, MouseStroke?>((d, e) => d.OnMouseStrokeChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.PART_TextBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_TextBlock");
        this.UpdateText();
    }

    private void OnMouseStrokeChanged(MouseStroke? oldValue, MouseStroke? newValue)
    {
        this.UpdateText();
    }

    private void UpdateText()
    {
        if (this.PART_TextBlock == null)
            return;

        if (!(this.MouseStroke is MouseStroke stroke))
            return;

        this.PART_TextBlock.Text = MouseStrokeStringConverter.ToStringFunction(stroke.MouseButton, stroke.Modifiers, stroke.ClickCount);
    }
}