using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Track;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public abstract class TimelineTrackControl : MultiSelector, ITrackHandle {
        /// <summary>
        /// The timeline that contains this track
        /// </summary>
        public TimelineControl Timeline => ItemsControlFromItemContainer(this) as TimelineControl;

        /// <summary>
        /// The zoom level of the associated timeline, or 1, if no timeline is present
        /// </summary>
        public double UnitZoom => this.Timeline?.UnitZoom ?? 1D;

        public TrackViewModel ViewModel => this.DataContext as TrackViewModel;

        public IResourceItemDropHandler ResourceItemDropHandler => this.DataContext as IResourceItemDropHandler;


        protected bool isProcessingDrop;
        public TimelineClipControl lastSelectedItem;

        protected TimelineTrackControl() {
            this.CanSelectMultipleItems = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is TrackViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(ITrackHandle), this);
                }
            };

            this.AllowDrop = true;
            this.Drop += this.OnDrop;
            this.DragEnter += this.OnDragDropEnter;
            this.DragOver += this.OnDragOver;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            if (e.Key == Key.System && e.OriginalSource is TimelineClipControl) {
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        public IEnumerable<TimelineClipControl> GetClipContainers() {
            return this.GetClipContainers<TimelineClipControl>(this.Items);
        }

        public IEnumerable<T> GetClipContainers<T>() where T : TimelineClipControl {
            return this.GetClipContainers<T>(this.Items);
        }

        public IEnumerable<TimelineClipControl> GetSelectedClipContainers() {
            return this.GetClipContainers<TimelineClipControl>(this.SelectedItems);
        }

        public IEnumerable<T> GetSelectedClipContainers<T>() where T : TimelineClipControl {
            return this.GetClipContainers<T>(this.SelectedItems);
        }

        public IEnumerable<T> GetClipContainers<T>(IEnumerable items, bool canUseIcgIndex = true) where T : TimelineClipControl {
            int i = 0;
            foreach (object item in items) {
                if (item is T a) {
                    yield return a;
                }
                else if (canUseIcgIndex) {
                    if (this.ItemContainerGenerator.ContainerFromIndex(i) is T b) {
                        yield return b;
                    }
                }
                else {
                    if (this.ItemContainerGenerator.ContainerFromItem(item) is T b) {
                        yield return b;
                    }
                }

                i++;
            }
        }

        public IEnumerable<TimelineClipControl> GetClipsThatIntersect(FrameSpan span) {
            return this.GetClipContainers<TimelineClipControl>().Where(x => x.Span.Intersects(span));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.Timeline.GetSelectedClipContainers().Any(clip => ReferenceEquals(clip.Track, this) && clip.IsMouseOver)) {
                return;
            }

            foreach (TimelineTrackControl track in this.Timeline.GetTrackContainers()) {
                if (track.SelectedItems.Count > 0) {
                    track.UnselectAll();
                }
            }

            this.Focus();
        }

        private void OnDragDropEnter(object sender, DragEventArgs e) {
            if (this.isProcessingDrop || this.ResourceItemDropHandler == null) {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void OnDragOver(object sender, DragEventArgs e) {
            IResourceItemDropHandler handler;
            if (!this.isProcessingDrop && (handler = this.ResourceItemDropHandler) != null) {
                if (e.Data.GetDataPresent(nameof(BaseResourceObjectViewModel))) {
                    object value = e.Data.GetData(nameof(BaseResourceObjectViewModel));
                    if (value is ResourceItemViewModel resource) {
                        if (handler.CanDropResource(resource)) {
                            return;
                        }
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e) {
            IResourceItemDropHandler handler;
            if (!this.isProcessingDrop && (handler = this.ResourceItemDropHandler) != null) {
                if (e.Data.GetDataPresent(nameof(BaseResourceObjectViewModel))) {
                    object value = e.Data.GetData(nameof(BaseResourceObjectViewModel));
                    if (value is ResourceItemViewModel resource) {
                        this.isProcessingDrop = true;
                        if (handler.CanDropResource(resource)) {
                            this.OnDropResource(handler, resource, e.GetPosition(this));
                            return;
                        }
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        protected async void OnDropResource(IResourceItemDropHandler handler, ResourceItemViewModel item, Point mouse) {
            long frame = TimelineUtils.PixelToFrame(mouse.X, this.UnitZoom);
            frame = Maths.Clamp(frame, 0, this.Timeline?.MaxDuration ?? 0);
            await handler.OnResourceDropped(item, frame);
            this.isProcessingDrop = false;
        }

        public void OnUnitZoomChanged() {
            foreach (TimelineClipControl element in this.GetClipContainers()) {
                element.OnUnitZoomChanged();
            }
        }

        public void MakeTopElement(TimelineClipControl control) {
            TrackViewModel track = this.ViewModel;
            if (track != null && control.DataContext is ClipViewModel clip) {
                track.MakeTopMost(clip);
            }
        }

        /// <summary>
        /// Creates an index map of this track's clips. The order of this Track's Items changes when a clip
        /// is selected, and also when clips are dragged around. This functions will create a sorted list of the items,
        /// and cache the unordered and ordered indices for fast access in dictionaries
        /// </summary>
        /// <returns></returns>
        public IndexMap<TimelineClipControl> CreateIndexMap() {
            // Dictionary<TimelineClipControl, long> clipToFrame = new Dictionary<TimelineClipControl, long>(this.Items.Count);
            // Dictionary<long, TimelineClipControl> frameToClip = new Dictionary<long, TimelineClipControl>(this.Items.Count);
            int count = this.Items.Count;
            Dictionary<TimelineClipControl, int> clipToRealIndex = new Dictionary<TimelineClipControl, int>(count);
            Dictionary<int, TimelineClipControl> realIndexToClip = new Dictionary<int, TimelineClipControl>(count);
            Dictionary<TimelineClipControl, int> clipToFakeIndex = new Dictionary<TimelineClipControl, int>(count);
            // only named it clipToFakeIndex because it lines up with the other names :3 it should be clipToOrderedIndex
            List<TimelineClipControl> clips = new List<TimelineClipControl>(count);
            IndexMap<TimelineClipControl> map = new IndexMap<TimelineClipControl>(clipToRealIndex, realIndexToClip, clipToFakeIndex, clips);
            int i = 0;

            foreach (TimelineClipControl clip in this.GetClipContainers()) {
                clipToRealIndex[clip] = i;
                realIndexToClip[i++] = clip;
                clips.Add(clip);
            }

            clips.Sort((a, b) => a.FrameBegin.CompareTo(b.FrameBegin));
            for (int j = 0; j < clips.Count; j++)
                clipToFakeIndex[clips[j]] = j;
            return map;
        }

        public void MakeRangedSelection(TimelineClipControl a, TimelineClipControl b) {
            if (a == b) {
                this.MakeSingleSelection(a);
            }
            else {
                IndexMap<TimelineClipControl> map = this.CreateIndexMap();
                int indexA = map.OrderedIndexOf(a);
                if (indexA == -1)
                    return;
                int indexB = map.OrderedIndexOf(b);
                if (indexB == -1)
                    return;

                if (indexA < indexB) {
                    this.UnselectAll();
                    for (int i = indexA; i <= indexB; i++) {
                        int index = map.OrderedIndexToRealIndex(i);
                        if (index != -1) {
                            // using RealIndexToClip may be faster than using ItemContainerGenerator indexing... maybe
                            this.SetItemSelectedProperty(map.RealIndexToValue[index], true);
                            // this.SetItemSelectedPropertyAtIndex(index, true);
                        }
                    }
                }
                else if (indexA > indexB) {
                    this.UnselectAll();
                    for (int i = indexB; i <= indexA; i++) {
                        int index = map.OrderedIndexToRealIndex(i);
                        if (index != -1) {
                            this.SetItemSelectedProperty(map.RealIndexToValue[index], true);
                            // this.SetItemSelectedPropertyAtIndex(index, true);
                        }
                    }
                }
                else {
                    this.MakeSingleSelection(a);
                }
            }
        }

        public void MakeSingleSelection(TimelineClipControl container) {
            this.UnselectAll();
            this.SetItemSelectedProperty(container, true);
            this.lastSelectedItem = container;
        }

        public void SetItemSelectedProperty(TimelineClipControl container, bool selected) {
            container.IsSelected = selected;
            object x = this.ItemContainerGenerator.ItemFromContainer(container);
            if (x == null || x == DependencyProperty.UnsetValue) {
                x = container;
            }

            if (selected) {
                this.SelectedItems.Add(x);
            }
            else {
                this.SelectedItems.Remove(x);
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            base.MeasureOverride(constraint);
            TimelineControl timeline = this.Timeline;
            return timeline == null ? constraint : new Size(timeline.MaxDuration * this.UnitZoom, constraint.Height);
        }

        public bool CanAcceptClip(TimelineClipControl clip) {
            if (this is VideoTrackControl) {
                return clip is VideoClipControl;
            }
            else if (this is AudioTrackControl) {

            }

            return false;
        }

        private object currentItem;

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.currentItem;
            this.currentItem = null;
            return this.GetContainerForItem(item);
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (this is VideoTrackControl) {
                return item is VideoClipControl;
            }

            this.currentItem = item;
            return false;
        }

        protected abstract TimelineClipControl GetContainerForItem(object item);
    }
}