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

using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    public class TrackControlSurfaceAudio : TrackControlSurface {
        public AudioTrack MyTrack { get; private set; }

        public TrackControlSurfaceAudio() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        protected override void OnConnected() {
            base.OnConnected();
            this.MyTrack = (AudioTrack) this.Owner.Track;
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            this.MyTrack = null;
        }
    }
}