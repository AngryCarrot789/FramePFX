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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    /// <summary>
    /// A list box which stores the <see cref="TrackControlSurfaceListBoxItem"/> items
    /// </summary>
    public class TrackControlSurfaceListBox : ListBox {
        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(TrackControlSurfaceListBox), new PropertyMetadata(null, (d, e) => ((TrackControlSurfaceListBox) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));

        public Timeline Timeline {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        public TimelineControl TimelineControl { get; set; }

        private readonly Stack<TrackControlSurfaceListBoxItem> cachedItems;
        private readonly Dictionary<Type, Stack<TrackControlSurface>> itemContentCacheMap;

        public TrackControlSurfaceListBox() {
            this.cachedItems = new Stack<TrackControlSurfaceListBoxItem>();
            this.itemContentCacheMap = new Dictionary<Type, Stack<TrackControlSurface>>();
            this.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(TrackControlSurfaceListBoxPanel)));
            this.SelectionMode = SelectionMode.Extended;
            // The scroll viewer that contains this listbox has this set, since that spans the timeline height but this doesn't
            // AdvancedContextMenu.SetContextGenerator(this, TrackContextRegistry.Instance);
        }

        static TrackControlSurfaceListBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TrackControlSurfaceListBox), new FrameworkPropertyMetadata(typeof(TrackControlSurfaceListBox)));
        }

        // I override the measuere/arrange functions to help with debugging sometimes

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return base.ArrangeOverride(arrangeBounds);
        }

        private void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            if (oldTimeline == newTimeline)
                return;
            if (oldTimeline != null) {
                oldTimeline.TrackAdded -= this.OnTrackAdded;
                oldTimeline.TrackRemoved -= this.OnTrackRemoved;
                oldTimeline.TrackMoved -= this.OnTrackMoved;
                for (int i = this.Items.Count - 1; i >= 0; i--) {
                    this.RemoveTrackInternal(i);
                }
            }

            if (newTimeline != null) {
                newTimeline.TrackAdded += this.OnTrackAdded;
                newTimeline.TrackRemoved += this.OnTrackRemoved;
                newTimeline.TrackMoved += this.OnTrackMoved;

                DataManager.SetContextData(this, new ContextData().Set(DataKeys.TimelineKey, newTimeline));

                int i = 0;
                foreach (Track track in newTimeline.Tracks) {
                    this.InsertTrackInternal(track, i++);
                }
            }
            else {
                DataManager.ClearContextData(this);
            }
        }

        private void OnTrackAdded(Timeline timeline, Track track, int index) {
            this.InsertTrackInternal(track, index);
        }

        private void OnTrackRemoved(Timeline timeline, Track track, int index) {
            this.RemoveTrackInternal(index);
        }

        private void InsertTrackInternal(Track track, int index) {
            TrackControlSurfaceListBoxItem control = this.cachedItems.Count > 0 ? this.cachedItems.Pop() : new TrackControlSurfaceListBoxItem();
            control.OnAddingToList(this, track);
            this.Items.Insert(index, control);
            // UpdateLayout must be called explicitly, so that the visual tree
            // can be measured, allowing templates to be applied
            control.InvalidateMeasure();
            control.UpdateLayout();
            control.OnAddedToList();
            this.TimelineControl.UpdateTrackAutomationVisibility(control);
        }

        private void RemoveTrackInternal(int index) {
            TrackControlSurfaceListBoxItem control = (TrackControlSurfaceListBoxItem) this.Items[index];
            control.OnRemovingFromList();
            this.Items.RemoveAt(index);
            control.OnRemovedFromList();
            if (this.cachedItems.Count < 8)
                this.cachedItems.Push(control);
        }

        private void OnTrackMoved(Timeline timeline, Track track, int oldIndex, int newIndex) {
            TrackControlSurfaceListBoxItem control = (TrackControlSurfaceListBoxItem) this.Items[oldIndex];
            control.OnIndexMoving(oldIndex, newIndex);
            this.Items.RemoveAt(oldIndex);
            this.Items.Insert(newIndex, control);
            control.OnIndexMoved(oldIndex, newIndex);
            this.InvalidateMeasure();
        }

        public TrackControlSurface GetContentObject(Type trackType) {
            TrackControlSurface content;
            if (this.itemContentCacheMap.TryGetValue(trackType, out Stack<TrackControlSurface> stack) && stack.Count > 0) {
                content = stack.Pop();
            }
            else {
                content = TrackControlSurface.NewInstance(trackType);
            }

            return content;
        }

        public bool ReleaseContentObject(Type trackType, TrackControlSurface contentControl) {
            if (!this.itemContentCacheMap.TryGetValue(trackType, out Stack<TrackControlSurface> stack)) {
                this.itemContentCacheMap[trackType] = stack = new Stack<TrackControlSurface>();
            }
            else if (stack.Count == 4) {
                return false;
            }

            stack.Push(contentControl);
            return true;
        }

        public IEnumerable<TrackControlSurfaceListBoxItem> GetTracks() => this.Items.Cast<TrackControlSurfaceListBoxItem>();
    }
}