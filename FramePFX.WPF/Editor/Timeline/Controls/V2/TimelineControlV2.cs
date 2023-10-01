using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.WPF.Editor.Timeline.Utils;

namespace FramePFX.WPF.Editor.Timeline.Controls.V2
{
    public class TimelineControlV2 : Control
    {
        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(TimelineControlV2),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => ((TimelineControlV2) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty MaxDurationProperty =
            DependencyProperty.Register(
                "MaxDuration",
                typeof(long),
                typeof(TimelineControlV2),
                new FrameworkPropertyMetadata(
                    10000L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => ((TimelineControlV2) d).OnMaxDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? TimelineUtils.ZeroLongBox : v));

        public static readonly DependencyProperty PlayHeadPositionProperty =
            DependencyProperty.Register(
                "PlayHeadPosition",
                typeof(long),
                typeof(TimelineControlV2),
                new FrameworkPropertyMetadata(TimelineUtils.ZeroLongBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty TracksProperty =
            DependencyProperty.Register(
                "Tracks",
                typeof(IEnumerable<TrackViewModel>),
                typeof(TimelineControlV2),
                new FrameworkPropertyMetadata(null, (d, e) => ((TimelineControlV2) d).OnTracksPropertyChanged((IEnumerable<TrackViewModel>) e.OldValue, (IEnumerable<TrackViewModel>) e.NewValue)));

        /// <summary>
        /// The horizontal zoom multiplier of this timeline, which affects the size of all tracks
        /// and therefore clips. This is a value used for converting frames into pixels
        /// </summary>
        public double UnitZoom
        {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public long MaxDuration
        {
            get => (long) this.GetValue(MaxDurationProperty);
            set => this.SetValue(MaxDurationProperty, value);
        }

        public long PlayHeadPosition
        {
            get => (long) this.GetValue(PlayHeadPositionProperty);
            set => this.SetValue(PlayHeadPositionProperty, value);
        }

        public IEnumerable<TrackViewModel> Tracks
        {
            get => (IEnumerable<TrackViewModel>) this.GetValue(TracksProperty);
            set => this.SetValue(TracksProperty, value);
        }

        public TimelineControlV2()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = base.MeasureOverride(constraint);
            double width = this.MaxDuration * this.UnitZoom;
            if (double.IsNaN(size.Width) || double.IsPositiveInfinity(size.Width))
            {
                return new Size(width, size.Height);
            }

            return new Size(Math.Max(width, size.Width), size.Height);
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom)
        {
        }

        private void OnMaxDurationChanged(long oldDuration, long newDuration)
        {
        }

        private void OnTracksPropertyChanged(IEnumerable<TrackViewModel> oldValue, IEnumerable<TrackViewModel> newValue)
        {
            if (oldValue is INotifyCollectionChanged a)
            {
                a.CollectionChanged -= this.OnTrackCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged b)
            {
                b.CollectionChanged += this.OnTrackCollectionChanged;
            }
        }

        private void OnTrackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }
    }
}