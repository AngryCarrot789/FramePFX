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
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    public class TrackControlSurfaceAudio : TrackControlSurface {
        public NumberDragger VolumeDragger { get; private set; }

        public ToggleButton MutedButton { get; private set; }

        public AudioTrack MyTrack { get; private set; }

        private readonly AutomationBinder<AudioTrack> volumeBinder = new AutomationBinder<AudioTrack>(AudioTrack.VolumeParameter);
        private readonly AutomationBinder<AudioTrack> isMutedBinder = new AutomationBinder<AudioTrack>(AudioTrack.IsMutedParameter);

        public TrackControlSurfaceAudio() {
            this.volumeBinder.UpdateModel += UpdateVolumeForModel;
            this.volumeBinder.UpdateControl += UpdateVolumeForControl;
            this.isMutedBinder.UpdateModel += UpdateVisibilityForModel;
            this.isMutedBinder.UpdateControl += UpdateVisibilityForControl;
        }

        private static void UpdateVolumeForModel(AutomationBinder<AudioTrack> binder) {
            AutomatedUtils.SetDefaultKeyFrameOrAddNew(binder.Model, binder.Parameter, (float) ((TrackControlSurfaceAudio) binder.Control).VolumeDragger.Value);
        }

        private static void UpdateVolumeForControl(AutomationBinder<AudioTrack> binder) {
            ((TrackControlSurfaceAudio) binder.Control).VolumeDragger.Value = AudioTrack.VolumeParameter.GetCurrentValue(binder.Model);
        }

        private static void UpdateVisibilityForModel(AutomationBinder<AudioTrack> binder) {
            AutomatedUtils.SetDefaultKeyFrameOrAddNew(binder.Model, binder.Parameter, (((TrackControlSurfaceAudio) binder.Control).MutedButton.IsChecked ?? false).Box());
        }

        private static void UpdateVisibilityForControl(AutomationBinder<AudioTrack> binder) {
            ((TrackControlSurfaceAudio) binder.Control).MutedButton.IsChecked = AudioTrack.IsMutedParameter.GetCurrentValue(binder.Model);
        }

        protected override void OnConnected() {
            base.OnConnected();
            this.MyTrack = (AudioTrack) this.Owner.Track;
            this.volumeBinder.Attach(this, this.MyTrack);
            this.isMutedBinder.Attach(this, this.MyTrack);
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            this.MyTrack = null;
            this.volumeBinder.Detatch();
            this.isMutedBinder.Detatch();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.GetTemplateChild("PART_VolumeSlider", out NumberDragger dragger);
            this.GetTemplateChild("PART_MutedButton", out ToggleButton visibilityButton);

            this.VolumeDragger = dragger;
            this.VolumeDragger.ValueChanged += (sender, args) => this.volumeBinder.OnControlValueChanged();

            this.MutedButton = visibilityButton;
            this.MutedButton.Checked += this.VisibilityCheckedChanged;
            this.MutedButton.Unchecked += this.VisibilityCheckedChanged;
        }

        private void VisibilityCheckedChanged(object sender, RoutedEventArgs e) {
            this.isMutedBinder.OnControlValueChanged();
        }
    }
}