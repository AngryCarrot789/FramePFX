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

using Avalonia.Controls.Primitives;
using FramePFX.Editing.Video;
using PFXToolKitUI.Avalonia.AvControls.Dragger;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;

namespace FramePFX.Avalonia.Editor.TrackSettings;

public class TrackSettingsControlVideo : TrackSettingsControl {
    private ToggleButton? PART_VisibilityButton;
    private NumberDragger? PART_OpacitySlider;

    private readonly IBinder<VideoTrack> isVisibleBinder = new AvaloniaPropertyToDataParameterBinder<VideoTrack>(VideoTrack.IsVisibleParameter, ToggleButton.IsCheckedProperty, b => ((ToggleButton) b.Control).IsChecked = b.Model.IsVisible, b => b.Model.IsVisible = ((ToggleButton) b.Control).IsChecked == true);
    private readonly IBinder<VideoTrack> opacityBinder = new AvaloniaPropertyToDataParameterBinder<VideoTrack>(VideoTrack.OpacityParameter, NumberDragger.ValueProperty, b => ((NumberDragger) b.Control).Value = b.Model.Opacity, b => b.Model.Opacity = ((NumberDragger) b.Control).Value);

    public VideoTrack? Track => (VideoTrack?) base.TrackViewState?.Track;
    
    public TrackSettingsControlVideo() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.isVisibleBinder.AttachControl(this.PART_VisibilityButton = e.NameScope.GetTemplateChild<ToggleButton>("PART_VisibilityButton"));
        this.opacityBinder.AttachControl(this.PART_OpacitySlider = e.NameScope.GetTemplateChild<NumberDragger>("PART_OpacitySlider"));
    }

    protected override void OnConnected() {
        base.OnConnected();
        
        this.isVisibleBinder.AttachModel(this.Track!);
        this.opacityBinder.AttachModel(this.Track!);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        
        this.isVisibleBinder.DetachModel();
        this.opacityBinder.DetachModel();
    }
}