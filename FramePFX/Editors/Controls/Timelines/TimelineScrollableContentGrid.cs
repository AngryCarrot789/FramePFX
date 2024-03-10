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
using System.Windows.Input;
using FramePFX.AttachedProperties;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Controls.Timelines
{
    public class TimelineScrollableContentGrid : Grid
    {
        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(TimelineScrollableContentGrid), new PropertyMetadata(null, OnTimelineChanged));

        public Timeline Timeline
        {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        public TimelineControl TimelineControl { get; set; }

        public bool HandleBringIntoView
        {
            get => HandleRequestBringIntoView.GetIsEnabled(this);
            set => HandleRequestBringIntoView.SetIsEnabled(this, value);
        }

        public TimelineScrollableContentGrid()
        {
            this.HandleBringIntoView = true;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!e.Handled && e.LeftButton == MouseButtonState.Pressed && this.TimelineControl != null)
            {
                Point point = e.GetPosition(this);
                bool isClickSequence = point.Y > this.TimelineControl.TimelineRuler.ActualHeight;
                this.TimelineControl.SetPlayHeadToMouseCursor(point.X, isClickSequence);
                if (isClickSequence)
                {
                    this.TimelineControl.Timeline?.ClearClipSelection();
                    this.TimelineControl.UpdatePropertyEditorClipSelection();
                }
            }
        }

        private static void OnTimelineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineScrollableContentGrid grid = (TimelineScrollableContentGrid) d;
            if (e.OldValue is Timeline oldTimeline)
            {
                oldTimeline.PlayHeadChanged -= grid.OnPlayHeadChanged;
                oldTimeline.ZoomTimeline -= grid.OnTimelineZoomed;
            }

            if (e.NewValue is Timeline newTimeline)
            {
                newTimeline.PlayHeadChanged += grid.OnPlayHeadChanged;
                newTimeline.ZoomTimeline += grid.OnTimelineZoomed;
            }
        }

        private void OnPlayHeadChanged(Timeline timeline, long oldvalue, long newvalue)
        {
            this.InvalidateMeasure();
        }

        private void OnTimelineZoomed(Timeline timeline, double oldzoom, double newzoom, ZoomType zoomtype)
        {
            this.InvalidateMeasure();
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.TimelineControl != null && this.TimelineControl.Timeline == null)
                arrangeSize.Width = this.TimelineControl.ActualWidth;
            return base.ArrangeOverride(arrangeSize);
        }
    }
}