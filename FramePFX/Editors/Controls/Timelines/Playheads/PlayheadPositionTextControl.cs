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
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Controls.Timelines.Playheads {
    public class PlayheadPositionTextControl : Control {
        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(PlayheadPositionTextControl), new PropertyMetadata(null, (d, e) => ((PlayheadPositionTextControl) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));
        public static readonly DependencyProperty PlayHeadPositionProperty = DependencyProperty.Register("PlayHeadPosition", typeof(long), typeof(PlayheadPositionTextControl), new FrameworkPropertyMetadata(0L));
        public static readonly DependencyProperty TotalFrameDurationProperty = DependencyProperty.Register("TotalFrameDuration", typeof(long), typeof(PlayheadPositionTextControl), new FrameworkPropertyMetadata(0L));
        public static readonly DependencyProperty LargestFrameInUseProperty = DependencyProperty.Register("LargestFrameInUse", typeof(long), typeof(PlayheadPositionTextControl), new PropertyMetadata(0L));

        public Timeline Timeline {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        public long PlayHeadPosition {
            get => (long) this.GetValue(PlayHeadPositionProperty);
            set => this.SetValue(PlayHeadPositionProperty, value);
        }

        public long TotalFrameDuration {
            get => (long) this.GetValue(TotalFrameDurationProperty);
            set => this.SetValue(TotalFrameDurationProperty, value);
        }

        public long LargestFrameInUse {
            get => (long) this.GetValue(LargestFrameInUseProperty);
            set => this.SetValue(LargestFrameInUseProperty, value);
        }

        private readonly GetSetAutoEventPropertyBinder<Timeline> playHeadBinder = new GetSetAutoEventPropertyBinder<Timeline>(PlayHeadPositionProperty, nameof(PlayheadPositionTextControl.Timeline.PlayHeadChanged), (b) => b.Model.PlayHeadPosition, (b, v) => b.Model.PlayHeadPosition = (long) v);
        private readonly GetSetAutoEventPropertyBinder<Timeline> totalFramesBinder = new GetSetAutoEventPropertyBinder<Timeline>(TotalFrameDurationProperty, nameof(PlayheadPositionTextControl.Timeline.MaxDurationChanged), (b) => b.Model.MaxDuration, (b, v) => b.Model.MaxDuration = (long) v);
        private readonly UpdaterAutoEventPropertyBinder<Timeline> largestFrameInUseBinder = new UpdaterAutoEventPropertyBinder<Timeline>(LargestFrameInUseProperty, nameof(PlayheadPositionTextControl.Timeline.LargestFrameInUseChanged), obj => obj.Control.SetValue(LargestFrameInUseProperty, obj.Model.LargestFrameInUse), null);

        public PlayheadPositionTextControl() {
        }

        static PlayheadPositionTextControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayheadPositionTextControl), new FrameworkPropertyMetadata(typeof(PlayheadPositionTextControl)));
        }

        private void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            if (oldTimeline != null) {
                this.totalFramesBinder.Detatch();
                this.playHeadBinder.Detatch();
                this.largestFrameInUseBinder.Detatch();
            }

            if (newTimeline != null) {
                this.totalFramesBinder.Attach(this, newTimeline);
                this.playHeadBinder.Attach(this, newTimeline);
                this.largestFrameInUseBinder.Attach(this, newTimeline);
            }
            else {
                this.PlayHeadPosition = 0;
                this.TotalFrameDuration = 0;
                this.LargestFrameInUse = 0;
            }
        }
    }
}