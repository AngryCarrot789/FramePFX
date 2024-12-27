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

using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Editing.Automation;
using FramePFX.BaseFrontEnd.AvControls.Dragger;
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.Timelines.Tracks;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;

public class TrackControlSurfaceVideo : TrackControlSurface {
    public NumberDragger OpacityDragger { get; private set; }

    public ToggleButton VisibilityButton { get; private set; }

    public VideoTrack MyTrack { get; private set; }

    private readonly AutomationBinder<VideoTrack> opacityBinder = new AutomationBinder<VideoTrack>(VideoTrack.OpacityParameter);
    private readonly AutomationBinder<VideoTrack> visibilityBinder = new AutomationBinder<VideoTrack>(VideoTrack.IsEnabledParameter);

    public TrackControlSurfaceVideo() {
        this.opacityBinder.UpdateModel += UpdateOpacityForModel;
        this.opacityBinder.UpdateControl += UpdateOpacityForControl;
        this.visibilityBinder.UpdateModel += UpdateVisibilityForModel;
        this.visibilityBinder.UpdateControl += UpdateVisibilityForControl;
    }

    public override void OnConnected() {
        base.OnConnected();
        this.MyTrack = (VideoTrack) this.Owner!.Track!;
        this.opacityBinder.Attach(this, this.MyTrack);
        this.visibilityBinder.Attach(this, this.MyTrack);
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.opacityBinder.Detach();
        this.visibilityBinder.Detach();
        this.MyTrack = null;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_OpacitySlider", out NumberDragger dragger);
        e.NameScope.GetTemplateChild("PART_VisibilityButton", out ToggleButton visibilityButton);

        this.OpacityDragger = dragger;
        this.OpacityDragger.ValueChanged += (sender, args) => this.opacityBinder.OnControlValueChanged();

        this.VisibilityButton = visibilityButton;
        this.VisibilityButton.IsCheckedChanged += this.VisibilityCheckedChanged;
    }

    private void VisibilityCheckedChanged(object? sender, RoutedEventArgs e) {
        this.visibilityBinder.OnControlValueChanged();
    }

    private static void UpdateOpacityForModel(AutomationBinder<VideoTrack> binder) {
        AvAutomationUtils.SetDefaultKeyFrameOrAddNew(binder.Model, ((TrackControlSurfaceVideo) binder.Control).OpacityDragger, binder.Parameter, RangeBase.ValueProperty);
        binder.Model.InvalidateRender();
    }

    private static void UpdateOpacityForControl(AutomationBinder<VideoTrack> binder) {
        TrackControlSurfaceVideo control = (TrackControlSurfaceVideo) binder.Control;
        control.OpacityDragger.Value = VideoTrack.OpacityParameter.GetCurrentValue(binder.Model);
    }

    private static void UpdateVisibilityForModel(AutomationBinder<VideoTrack> binder) {
        AvAutomationUtils.SetDefaultKeyFrameOrAddNew(binder.Model, ((TrackControlSurfaceVideo) binder.Control).VisibilityButton, binder.Parameter, ToggleButton.IsCheckedProperty);
        binder.Model.InvalidateRender();
    }

    private static void UpdateVisibilityForControl(AutomationBinder<VideoTrack> binder) {
        TrackControlSurfaceVideo control = (TrackControlSurfaceVideo) binder.Control;
        control.VisibilityButton.IsChecked = VideoTrack.IsEnabledParameter.GetCurrentValue(binder.Model);
    }
}