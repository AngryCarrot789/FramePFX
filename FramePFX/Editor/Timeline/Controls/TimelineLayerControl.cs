using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public abstract class TimelineLayerControl : MultiSelector, ILayerHandle {
        public static readonly DependencyProperty ResourceDropHandlerProperty =
            DependencyProperty.Register(
                "ResourceDropHandler",
                typeof(IResourceDropHandler),
                typeof(TimelineLayerControl),
                new PropertyMetadata(null));

        public IResourceDropHandler ResourceDropHandler {
            get => (IResourceDropHandler) this.GetValue(ResourceDropHandlerProperty);
            set => this.SetValue(ResourceDropHandlerProperty, value);
        }

        /// <summary>
        /// The timeline that contains this layer
        /// </summary>
        public TimelineControl Timeline => ItemsControlFromItemContainer(this) as TimelineControl;

        /// <summary>
        /// The zoom level of the associated timeline, or 1, if no timeline is present
        /// </summary>
        public double UnitZoom => this.Timeline?.UnitZoom ?? 1D;

        public LayerViewModel ViewModel => this.DataContext as LayerViewModel;

        protected bool isProcessingDrop;
        public TimelineClipControl lastSelectedItem;

        protected TimelineLayerControl() {
            this.CanSelectMultipleItems = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is LayerViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(ILayerHandle), this);
                }
            };

            this.AllowDrop = true;
            this.Drop += this.OnDrop;
            this.DragEnter += this.OnDragDropEnter;
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

        public abstract IEnumerable<TimelineClipControl> GetClipsThatIntersect(FrameSpan span);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.Timeline.GetSelectedClipContainers().Any(clip => ReferenceEquals(clip.Layer, this) && clip.IsMouseOver)) {
                return;
            }

            foreach (TimelineLayerControl layer in this.Timeline.GetLayerContainers()) {
                if (layer.SelectedItems.Count > 0) {
                    layer.UnselectAll();
                }
            }
        }

        private void OnDragDropEnter(object sender, DragEventArgs e) {
            if (this.isProcessingDrop || this.ResourceDropHandler == null) {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (this.isProcessingDrop || this.ResourceDropHandler == null) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent(nameof(ResourceItem))) {
                if (e.Data.GetData(nameof(ResourceItem)) is ResourceItem resource) {
                    this.isProcessingDrop = true;
                    this.OnDropResource(resource, e.GetPosition(this));
                }
            }
        }

        protected async void OnDropResource(ResourceItem item, Point mouse) {
            if (item.IsRegistered) {
                long frame = TimelineUtils.PixelToFrame(mouse.X, this.UnitZoom);
                frame = Maths.Clamp(frame, 0, this.Timeline?.MaxDuration ?? 0);
                await this.ResourceDropHandler.OnResourceDropped(item, frame);
            }

            this.isProcessingDrop = false;
        }

        public void OnUnitZoomChanged() {
            foreach (TimelineClipControl element in this.GetClipContainers()) {
                element.OnUnitZoomChanged();
            }
        }

        public void MakeTopElement(TimelineClipControl control) {
            LayerViewModel layer = this.ViewModel;
            if (layer != null && control.DataContext is ClipViewModel clip) {
                layer.MakeTopMost(clip);
            }
        }

        /// <summary>
        /// Creates an index map of this layer's clips. The order of this Layer's Items changes when a clip
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
    }
}