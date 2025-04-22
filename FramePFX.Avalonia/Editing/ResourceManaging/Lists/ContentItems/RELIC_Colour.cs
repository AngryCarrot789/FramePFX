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
using Avalonia.Media;
using FramePFX.BaseFrontEnd.ResourceManaging;
using PFXToolKitUI.Avalonia.Bindings;
using FramePFX.Editing.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists.ContentItems;

public class RELIC_Colour : ResourceExplorerListItemContent {
    public static readonly StyledProperty<SolidColorBrush?> BrushProperty = AvaloniaProperty.Register<RELIC_Colour, SolidColorBrush?>(nameof(Brush));

    public SolidColorBrush? Brush {
        get => this.GetValue(BrushProperty);
        set => this.SetValue(BrushProperty, value);
    }

    public new ResourceColour? Resource => (ResourceColour?) base.Resource;

    private readonly AvaloniaPropertyToEventPropertyBinder<ResourceColour> colourBinder = new AvaloniaPropertyToEventPropertyBinder<ResourceColour>(BrushProperty, nameof(ResourceColour.ColourChanged), binder => {
        RELIC_Colour element = (RELIC_Colour) binder.Control;
        SKColor c = binder.Model.Colour;
        element.Brush!.Color = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
    }, binder => {
        RELIC_Colour element = (RELIC_Colour) binder.Control;
        Color c = element.Brush!.Color;
        binder.Model.Colour = new SKColor(c.R, c.G, c.B, c.A);
    });

    public RELIC_Colour() {
        this.Brush = new SolidColorBrush();
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.colourBinder.Attach(this, this.Resource!);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.colourBinder.Detach();
    }
}