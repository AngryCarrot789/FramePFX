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

using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    public class TrackControlSurfaceVideo : TrackControlSurface {
        public NumberDragger OpacityDragger { get; private set; }

        public ToggleButton VisibilityButton { get; private set; }

        public VideoTrack MyTrack { get; private set; }

        private readonly AutomationBinder<VideoTrack> opacityBinder = new AutomationBinder<VideoTrack>(VideoTrack.OpacityParameter);
        private readonly AutomationBinder<VideoTrack> visibilityBinder = new AutomationBinder<VideoTrack>(VideoTrack.VisibleParameter);

        public TrackControlSurfaceVideo() {
            this.opacityBinder.UpdateModel += UpdateOpacityForModel;
            this.opacityBinder.UpdateControl += UpdateOpacityForControl;
            this.visibilityBinder.UpdateModel += UpdateVisibilityForModel;
            this.visibilityBinder.UpdateControl += UpdateVisibilityForControl;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.GetTemplateChild("PART_OpacitySlider", out NumberDragger dragger);
            this.GetTemplateChild("PART_VisibilityButton", out ToggleButton visibilityButton);

            this.OpacityDragger = dragger;
            this.OpacityDragger.ValueChanged += (sender, args) => this.opacityBinder.OnControlValueChanged();

            this.VisibilityButton = visibilityButton;
            this.VisibilityButton.Checked += this.VisibilityCheckedChanged;
            this.VisibilityButton.Unchecked += this.VisibilityCheckedChanged;
        }

        private void VisibilityCheckedChanged(object sender, RoutedEventArgs e) {
            this.visibilityBinder.OnControlValueChanged();
        }

        private static void UpdateOpacityForModel(AutomationBinder<VideoTrack> binder) {
            AutomatedUtils.SetDefaultKeyFrameOrAddNew(binder.Model, ((TrackControlSurfaceVideo) binder.Control).OpacityDragger, binder.Parameter, RangeBase.ValueProperty);
            binder.Model.InvalidateRender();
        }

        private static void UpdateOpacityForControl(AutomationBinder<VideoTrack> binder) {
            TrackControlSurfaceVideo control = (TrackControlSurfaceVideo) binder.Control;
            control.OpacityDragger.Value = VideoTrack.OpacityParameter.GetCurrentValue(binder.Model);
        }

        private static void UpdateVisibilityForModel(AutomationBinder<VideoTrack> binder) {
            AutomatedUtils.SetDefaultKeyFrameOrAddNew(binder.Model, ((TrackControlSurfaceVideo) binder.Control).VisibilityButton, binder.Parameter, ToggleButton.IsCheckedProperty);
            binder.Model.InvalidateRender();
        }

        private static void UpdateVisibilityForControl(AutomationBinder<VideoTrack> binder) {
            TrackControlSurfaceVideo control = (TrackControlSurfaceVideo) binder.Control;
            control.VisibilityButton.IsChecked = VideoTrack.VisibleParameter.GetCurrentValue(binder.Model);
        }

        protected override void OnConnected() {
            base.OnConnected();
            this.MyTrack = (VideoTrack) this.Owner.Track;
            this.opacityBinder.Attach(this, this.MyTrack);
            this.visibilityBinder.Attach(this, this.MyTrack);
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            this.MyTrack = null;
            this.opacityBinder.Detatch();
            this.visibilityBinder.Detatch();
        }
    }
}