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

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Controls.Timelines.Playheads
{
    public abstract class BasePlayHeadControl : Control
    {
        protected static readonly FieldInfo IsDraggingPropertyKeyField = typeof(Thumb).GetField("IsDraggingPropertyKey", BindingFlags.NonPublic | BindingFlags.Static);
        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(BasePlayHeadControl), new PropertyMetadata(null, (d, e) => ((BasePlayHeadControl) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));

        public Timeline Timeline
        {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        protected BasePlayHeadControl()
        {
        }

        public abstract long GetFrame(Timeline timeline);

        protected virtual void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline)
        {
            if (oldTimeline != null)
            {
                oldTimeline.ZoomTimeline -= this.OnTimelineZoomed;
            }

            if (newTimeline != null)
            {
                newTimeline.ZoomTimeline += this.OnTimelineZoomed;
                this.Visibility = Visibility.Visible;
                this.SetPixelFromFrameAndZoom(this.GetFrame(newTimeline), newTimeline.Zoom);
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void OnTimelineZoomed(Timeline timeline, double oldzoom, double newzoom, ZoomType zoomtype)
        {
            this.SetPixelFromFrameAndZoom(this.GetFrame(timeline), newzoom);
        }

        protected void SetPixelFromFrame(long frame)
        {
            this.SetPixelFromFrameAndZoom(frame, this.Timeline?.Zoom ?? 1.0d);
        }

        protected void SetPixelFromFrameAndZoom(long frame, double zoom)
        {
            Thickness m = this.Margin;
            this.Margin = new Thickness(frame * zoom, m.Top, m.Right, m.Bottom);
        }
    }
}