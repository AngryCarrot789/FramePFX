using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.ResourceManaging;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.Controls {
    public class VideoTimelineControl : MultiSelector, ILayerHandle {
        public static readonly DependencyProperty UnitZoomProperty =
            TimelineControl.UnitZoomProperty.AddOwner(
                typeof(VideoTimelineControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((VideoTimelineControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty ResourceDropNotifierProperty = DependencyProperty.Register("ResourceDropNotifier", typeof(IResourceDropNotifier), typeof(VideoTimelineControl), new PropertyMetadata(default(IResourceDropNotifier)));

        /// <summary>
        /// The zoom level of this timeline layer
        /// <para>
        /// This is a value used for converting frames into pixels
        /// </para>
        /// </summary>
        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        //           Width
        // ---------------------------
        // UnitZoom * MaxFrameDuration

        /// <summary>
        /// Gets or sets the maximum duration (in frames) of this timeline layer based on it's visual/actual pixel width
        /// <para>
        /// Setting this will modify the <see cref="UnitZoom"/> property as ActualWidth / MaxFrameDuration
        /// </para>
        /// </summary>
        public double MaxFrameDuration {
            get => this.ActualWidth / this.UnitZoom;
            set => this.UnitZoom = this.ActualWidth / value;
        }

        /// <summary>
        /// The timeline that owns/contains this timeline layer
        /// </summary>
        public TimelineControl Timeline {
            get => this.timeline;
            set => this.timeline = value;
        }

        public IResourceDropNotifier ResourceDropNotifier {
            get => (IResourceDropNotifier) this.GetValue(ResourceDropNotifierProperty);
            set => this.SetValue(ResourceDropNotifierProperty, value);
        }

        private bool isUpdatingUnitZoom;
        private bool isUpdatingLayerType;
        private TimelineControl timeline;
        private TimelineVideoClipControl lastSelectedItem;

        public TimelineLayer ViewModel => this.DataContext as TimelineLayer;

        public VideoTimelineControl() {
            this.CanSelectMultipleItems = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is TimelineLayer vm) {
                    vm.Control = this;
                }
            };
            this.AllowDrop = true;
            this.Drop += this.OnDrop;
        }

        private async void OnDrop(object sender, DragEventArgs e) {
            if (this.ResourceDropNotifier == null) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent("ResourceItem")) {
                object obj = e.Data.GetData("ResourceItem");
                if (obj is ResourceItem resourceItemViewModel) {
                    await this.ResourceDropNotifier.OnResourceDropped(resourceItemViewModel);
                }
            }
        }

        /// <summary>
        /// Creates an index map of this layer's clips. The order of this Layer's Items changes when a clip
        /// is selected, and also when clips are dragged around. This functions will create a sorted list of the items,
        /// and cache the unordered and ordered indices for fast access in dictionaries
        /// </summary>
        /// <returns></returns>
        public IndexMap<TimelineVideoClipControl> CreateIndexMap() {
            // Dictionary<TimelineClipControl, long> clipToFrame = new Dictionary<TimelineClipControl, long>(this.Items.Count);
            // Dictionary<long, TimelineClipControl> frameToClip = new Dictionary<long, TimelineClipControl>(this.Items.Count);
            int count = this.Items.Count;
            Dictionary<TimelineVideoClipControl, int> clipToRealIndex = new Dictionary<TimelineVideoClipControl, int>(count);
            Dictionary<int, TimelineVideoClipControl> realIndexToClip = new Dictionary<int, TimelineVideoClipControl>(count);
            Dictionary<TimelineVideoClipControl, int> clipToFakeIndex = new Dictionary<TimelineVideoClipControl, int>(count);
            // only named it clipToFakeIndex because it lines up with the other names :3 it should be clipToOrderedIndex
            List<TimelineVideoClipControl> clips = new List<TimelineVideoClipControl>(count);
            IndexMap<TimelineVideoClipControl> map = new IndexMap<TimelineVideoClipControl>(clipToRealIndex, realIndexToClip, clipToFakeIndex, clips);
            int i = 0;
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out TimelineVideoClipControl clip)) {
                    clipToRealIndex[clip] = i;
                    realIndexToClip[i++] = clip;
                    clips.Add(clip);
                }
                else {
                    // !!! wot to do here??? this shouldn't be reachable but...
                    return map;
                }
            }

            clips.Sort((a, b) => a.FrameBegin.CompareTo(b.FrameBegin));
            for (int j = 0; j < clips.Count; j++)
                clipToFakeIndex[clips[j]] = j;
            return map;
        }

        public bool GetClipControl(object item, out TimelineVideoClipControl clip) {
            return (clip = ICGenUtils.GetContainerForItem<TimelineVideoClip, TimelineVideoClipControl>(item, this.ItemContainerGenerator, x => x.Handle as TimelineVideoClipControl)) != null;
        }

        public bool GetClipViewModel(object item, out TimelineVideoClip videoClip) {
            return ICGenUtils.GetItemForContainer<TimelineVideoClipControl, TimelineVideoClip>(item, this.ItemContainerGenerator, x => x.ViewModel, out videoClip);
        }

        public IEnumerable<TimelineVideoClipControl> GetClipControls() {
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out TimelineVideoClipControl clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineVideoClip> GetClipViewModels() {
            foreach (object item in this.Items) {
                if (this.GetClipViewModel(item, out TimelineVideoClip clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineVideoClipControl> GetSelectedClipControls() {
            foreach (object item in this.SelectedItems) {
                if (this.GetClipControl(item, out TimelineVideoClipControl clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineVideoClip> GetSelectedClipViewModels() {
            foreach (object item in this.SelectedItems) {
                if (this.GetClipViewModel(item, out TimelineVideoClip clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineVideoClipControl> GetClipsInArea(FrameSpan span) {
            List<TimelineVideoClipControl> list = new List<TimelineVideoClipControl>();
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out TimelineVideoClipControl clip)) {
                    if (clip.Span.Intersects(span)) {
                        list.Add(clip);
                    }
                }
            }

            return list;
        }

        public bool GetViewModel(out TimelineLayer timelineLayer) {
            return (timelineLayer = this.ViewModel) != null;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TimelineVideoClipControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is TimelineVideoClipControl;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is TimelineVideoClipControl clip) {
                if (item is TimelineVideoClip viewModel) {
                    viewModel.Handle = clip;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            List<TimelineVideoClipControl> clips = this.timeline.GetAllSelectedClipControls().ToList();
            if (clips.Any(clip => clip.VideoTimeline == this && clip.IsMouseOver)) {
                return;
            }

            clips.ForEach(x => {
                if (x.VideoTimeline.SelectedItems.Count > 0) {
                    x.VideoTimeline.UnselectAll();
                }
            });
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.isUpdatingUnitZoom) {
                if (!TimelineUtils.IsUnitEqual(oldZoom, newZoom)) {
                    throw new Exception("Recursive update of FrameOffset. Old = " + oldZoom + ", New = " + newZoom);
                }

                return;
            }

            this.isUpdatingUnitZoom = true;
            if (Math.Abs(oldZoom - newZoom) > TimelineUtils.MinUnitZoom) {
                foreach (TimelineVideoClipControl element in this.GetElements()) {
                    element.UnitZoom = newZoom;
                }
            }

            this.isUpdatingUnitZoom = false;
        }

        private void OnLayerTypeChanged(string oldType, string newType) {
            if (this.isUpdatingLayerType || oldType == newType)
                return;

            this.isUpdatingLayerType = true;

            // Maybe invalidate all of the elements?
            foreach (TimelineVideoClipControl element in this.GetElements()) {
                element.OnLayerTypeChanged(oldType, newType);
            }

            this.isUpdatingLayerType = false;
        }

        public double GetRenderX(TimelineVideoClipControl control) {
            return control.Margin.Left;
        }

        public void SetRenderX(TimelineVideoClipControl control, double value) {
            Thickness margin = control.Margin;
            margin.Left = value;
            control.Margin = margin;
        }

        public double GetRenderY(TimelineVideoClipControl control) {
            return control.Margin.Top;
        }

        public void SetRenderY(TimelineVideoClipControl control, double value) {
            Thickness margin = control.Margin;
            margin.Top = value;
            control.Margin = margin;
        }

        public IEnumerable<TimelineVideoClipControl> GetElements() {
            foreach (object item in this.Items) {
                if (item is TimelineVideoClipControl clip) {
                    yield return clip;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is TimelineVideoClipControl clip2) {
                    yield return clip2;
                }
            }
        }

        public TimelineVideoClipControl CreateClonedElement(TimelineVideoClipControl clipToCopy) {
            throw new Exception();
        }

        public TimelineVideoClipControl CreateElement(int startFrame, int durationFrames) {
            throw new Exception();
        }

        /// <summary>
        /// Removes the given clip from this timeline layer
        /// </summary>
        /// <param name="clip"></param>
        public bool RemoveClip(TimelineVideoClipControl clip) {
            int index = this.Items.IndexOf(clip);
            if (index == -1) {
                return false;
            }

            this.Items.RemoveAt(index);
            // TODO: post process clip removal, maybe check if there are no more references to a resource
            // that the clip used, and then delete the resource?
            return true;
        }

        public void OnClipMouseButton(TimelineVideoClipControl clip, MouseButtonEventArgs e, bool wasDragging = false) {
            if (e.ChangedButton == MouseButton.Left) {
                if (e.ButtonState == MouseButtonState.Pressed) {
                    if (AreModifiersPressed(ModifierKeys.Control)) {
                        this.SetItemSelectedProperty(clip, true);
                    }
                    else if (AreModifiersPressed(ModifierKeys.Shift) && this.lastSelectedItem != null && this.SelectedItems.Count > 0) {
                        this.MakeRangedSelection(this.lastSelectedItem, clip);
                    }
                    else if (this.Timeline.GetAllSelectedClipControls().ToList().Count > 1) {
                        if (!clip.IsSelected) {
                            this.Timeline.SetPrimarySelection(this, clip);
                        }
                    }
                    else {
                        this.Timeline.SetPrimarySelection(this, clip);
                    }
                }
                else if (!wasDragging && clip.IsSelected && !AreModifiersPressed(ModifierKeys.Control) && !AreModifiersPressed(ModifierKeys.Shift) && this.Timeline.GetAllSelectedClipControls().ToList().Count > 1) {
                    this.Timeline.SetPrimarySelection(this, clip);
                }
            }

            // if (e.ChangedButton == MouseButton.Left) {
            //     if (e.ButtonState == MouseButtonState.Released) {
            //         this.SelectedItems.Clear();
            //         element.IsSelected = !element.IsSelected;
            //         this.Timeline.OnElementSelectionChanged(this, element);
            // 
            //         // if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
            //         //     element.IsSelected = !element.IsSelected;
            //         //     this.Timeline.OnElementSelectionChanged(this, element);
            //         // }
            //         // else {
            //         //     this.SelectedItems.Clear();
            //         //     element.IsSelected = !element.IsSelected;
            //         //     this.Timeline.OnElementSelectionChanged(this, element);
            //         // }
            //     }
            // }
        }

        public void MakeTopElement(TimelineVideoClipControl control) {
            TimelineLayer timelineLayer = this.ViewModel;
            TimelineVideoClip container;
            if (timelineLayer != null && (container = control.ViewModel) != null) {
                timelineLayer.MakeTopMost(container);
            }
        }

        // TODO: Implement cross-layer selection. The timeline can cache the last layer that make a single selection (aka the anchor)
        // public void MakeRangedSelection(FrameSpan span) {
        //     IndexMap<TimelineClipControl> map = this.CreateIndexMap();
        //
        // }

        public void MakeRangedSelection(TimelineVideoClipControl a, TimelineVideoClipControl b) {
            if (a == b) {
                this.MakeSingleSelection(a);
            }
            else {
                IndexMap<TimelineVideoClipControl> map = this.CreateIndexMap();
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

        public void MakeSingleSelection(TimelineVideoClipControl item) {
            this.UnselectAll();
            this.SetItemSelectedProperty(item, true);
            this.lastSelectedItem = item;
        }

        public void SetItemSelectedProperty(TimelineVideoClipControl item, bool selected) {
            item.IsSelected = selected;
            object x = this.ItemContainerGenerator.ItemFromContainer(item);
            if (x == null || x == DependencyProperty.UnsetValue)
                x = item;

            if (selected) {
                this.SelectedItems.Add(x);
            }
            else {
                this.SelectedItems.Remove(x);
            }

            this.Timeline.ViewModel.MainSelectedClip = item.ViewModel;
            this.Timeline.ViewModel.OnUpdateSelection(this.GetSelectedClipViewModels());
        }

        public bool SetItemSelectedPropertyAtIndex(int index, bool selected) {
            if (index < 0 || index >= this.Items.Count) {
                return false;
            }

            if (this.ItemContainerGenerator.ContainerFromIndex(index) is TimelineVideoClipControl resource) {
                this.SetItemSelectedProperty(resource, true);
                return true;
            }
            else {
                return false;
            }
        }

        public static bool AreModifiersPressed(ModifierKeys key1) {
            return (Keyboard.Modifiers & key1) == key1;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2) {
            return (Keyboard.Modifiers & (key1 | key2)) == key1;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2, ModifierKeys key3) {
            return (Keyboard.Modifiers & (key1 | key2 | key3)) == key1;
        }

        public static bool AreModifiersPressed(params ModifierKeys[] keys) {
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (ModifierKeys modifier in keys)
                modifiers |= modifier;
            return (Keyboard.Modifiers & modifiers) == modifiers;
        }
    }
}
