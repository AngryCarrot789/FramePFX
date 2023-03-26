using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Timeline.Layer.Clips;

namespace FramePFX.Timeline.Layer {
    public class TimelineLayerControl : MultiSelector, ILayerHandle {
        public static readonly DependencyProperty UnitZoomProperty =
            TimelineControl.UnitZoomProperty.AddOwner(
                typeof(TimelineLayerControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineLayerControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty LayerTypeProperty =
            DependencyProperty.Register(
                "LayerType",
                typeof(string),
                typeof(TimelineLayerControl),
                new FrameworkPropertyMetadata(
                    "Any",
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((TimelineLayerControl) d).OnLayerTypeChanged((string) e.OldValue, (string) e.NewValue)));

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

        /// <summary>
        /// The type of elements that this layer contains. Elements will contain 
        /// the same layer type as this, at least, that's the intention
        /// </summary>
        public string LayerType {
            get => (string) this.GetValue(LayerTypeProperty);
            set => this.SetValue(LayerTypeProperty, value);
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

        private bool isUpdatingUnitZoom;
        private bool isUpdatingLayerType;
        private TimelineControl timeline;
        private TimelineClipContainerControl lastSelectedItem;

        public LayerViewModel ViewModel => this.DataContext as LayerViewModel;

        public TimelineLayerControl() {
            this.LayerType = "Any";
            this.CanSelectMultipleItems = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is LayerViewModel vm) {
                    vm.Control = this;
                }
            };
        }

        /// <summary>
        /// Creates an index map of this layer's clips. The order of this Layer's Items changes when a clip
        /// is selected, and also when clips are dragged around. This functions will create a sorted list of the items,
        /// and cache the unordered and ordered indices for fast access in dictionaries
        /// </summary>
        /// <returns></returns>
        public IndexMap<TimelineClipContainerControl> CreateIndexMap() {
            // Dictionary<TimelineClipControl, long> clipToFrame = new Dictionary<TimelineClipControl, long>(this.Items.Count);
            // Dictionary<long, TimelineClipControl> frameToClip = new Dictionary<long, TimelineClipControl>(this.Items.Count);
            int count = this.Items.Count;
            Dictionary<TimelineClipContainerControl, int> clipToRealIndex = new Dictionary<TimelineClipContainerControl, int>(count);
            Dictionary<int, TimelineClipContainerControl> realIndexToClip = new Dictionary<int, TimelineClipContainerControl>(count);
            Dictionary<TimelineClipContainerControl, int> clipToFakeIndex = new Dictionary<TimelineClipContainerControl, int>(count);
            // only named it clipToFakeIndex because it lines up with the other names :3
            List<TimelineClipContainerControl> clips = new List<TimelineClipContainerControl>(count);
            IndexMap<TimelineClipContainerControl> map = new IndexMap<TimelineClipContainerControl>(clipToRealIndex, realIndexToClip, clipToFakeIndex, clips);
            int i = 0;
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out TimelineClipContainerControl clip)) {
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

        public bool GetClipControl(object item, out TimelineClipContainerControl clip) {
            return (clip = ICGenUtils.GetContainerForItem<ClipContainerViewModel, TimelineClipContainerControl>(item, this.ItemContainerGenerator, x => x.ContainerHandle as TimelineClipContainerControl)) != null;
        }

        public bool GetClipViewModel(object item, out ClipContainerViewModel clip) {
            return ICGenUtils.GetItemForContainer<TimelineClipContainerControl, ClipContainerViewModel>(item, this.ItemContainerGenerator, x => x.ViewModel, out clip);
        }

        public IEnumerable<TimelineClipContainerControl> GetClipControls() {
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out TimelineClipContainerControl clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ClipContainerViewModel> GetClipViewModels() {
            foreach (object item in this.Items) {
                if (this.GetClipViewModel(item, out ClipContainerViewModel clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineClipContainerControl> GetSelectedClipControls() {
            foreach (object item in this.SelectedItems) {
                if (this.GetClipControl(item, out TimelineClipContainerControl clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ClipContainerViewModel> GetSelectedClipViewModels() {
            foreach (object item in this.SelectedItems) {
                if (this.GetClipViewModel(item, out ClipContainerViewModel clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineClipContainerControl> GetClipsInArea(FrameSpan span) {
            List<TimelineClipContainerControl> list = new List<TimelineClipContainerControl>();
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out TimelineClipContainerControl clip)) {
                    if (clip.Span.Intersects(span)) {
                        list.Add(clip);
                    }
                }
            }

            return list;
        }

        public bool GetViewModel(out LayerViewModel layer) {
            return (layer = this.ViewModel) != null;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TimelineClipContainerControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is TimelineClipContainerControl;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is TimelineClipContainerControl clip) {
                if (item is ClipContainerViewModel viewModel) {
                    viewModel.ContainerHandle = clip;
                }
                // else {
                //     throw new Exception($"Expected item of type {nameof(ClipViewModel)}, got {item?.GetType()}");
                // }

                // clip.TimelineLayer = this;
            }
            // else {
            //     throw new Exception($"Expected element of type {nameof(TimelineClipControl)}, got {element?.GetType()}");
            // }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            List<TimelineClipContainerControl> clips = this.timeline.GetAllSelectedClipControls().ToList();
            if (clips.Any(clip => clip.TimelineLayer == this && clip.IsMouseOver)) {
                return;
            }

            clips.ForEach(x => {
                if (x.TimelineLayer.SelectedItems.Count > 0) {
                    x.TimelineLayer.UnselectAll();
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
                foreach (TimelineClipContainerControl element in this.GetElements()) {
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
            foreach (TimelineClipContainerControl element in this.GetElements()) {
                element.OnLayerTypeChanged(oldType, newType);
            }

            this.isUpdatingLayerType = false;
        }

        public double GetRenderX(TimelineClipContainerControl control) {
            return control.Margin.Left;
        }

        public void SetRenderX(TimelineClipContainerControl control, double value) {
            Thickness margin = control.Margin;
            margin.Left = value;
            control.Margin = margin;
        }

        public double GetRenderY(TimelineClipContainerControl control) {
            return control.Margin.Top;
        }

        public void SetRenderY(TimelineClipContainerControl control, double value) {
            Thickness margin = control.Margin;
            margin.Top = value;
            control.Margin = margin;
        }

        public IEnumerable<TimelineClipContainerControl> GetElements() {
            foreach (object item in this.Items) {
                if (item is TimelineClipContainerControl clip) {
                    yield return clip;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is TimelineClipContainerControl clip2) {
                    yield return clip2;
                }
            }
        }

        public TimelineClipContainerControl CreateClonedElement(TimelineClipContainerControl clipToCopy) {
            throw new Exception();
        }

        public TimelineClipContainerControl CreateElement(int startFrame, int durationFrames) {
            throw new Exception();
        }

        /// <summary>
        /// Removes the given clip from this timeline layer
        /// </summary>
        /// <param name="clip"></param>
        public bool RemoveClip(TimelineClipContainerControl clip) {
            int index = this.Items.IndexOf(clip);
            if (index == -1) {
                return false;
            }

            this.Items.RemoveAt(index);
            // TODO: post process clip removal, maybe check if there are no more references to a resource
            // that the clip used, and then delete the resource?
            return true;
        }

        public void OnClipMouseButton(TimelineClipContainerControl clip, MouseButtonEventArgs e, bool wasDragging = false) {
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

        public void MakeTopElement(TimelineClipContainerControl control) {
            if (this.GetViewModel(out LayerViewModel layer) && control.GetViewModel(out ClipContainerViewModel clip)) {
                layer.MakeTopMost(clip);
            }
        }

        // TODO: Implement cross-layer selection. The timeline can cache the last layer that make a single selection (aka the anchor)
        // public void MakeRangedSelection(FrameSpan span) {
        //     IndexMap<TimelineClipControl> map = this.CreateIndexMap();
        //
        // }

        public void MakeRangedSelection(TimelineClipContainerControl a, TimelineClipContainerControl b) {
            if (a == b) {
                this.MakeSingleSelection(a);
            }
            else {
                IndexMap<TimelineClipContainerControl> map = this.CreateIndexMap();
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

        public void MakeSingleSelection(TimelineClipContainerControl item) {
            this.UnselectAll();
            this.SetItemSelectedProperty(item, true);
            this.lastSelectedItem = item;
        }

        public void SetItemSelectedProperty(TimelineClipContainerControl item, bool selected) {
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

            this.Timeline.ViewModel.MainSelectedClip = item.ViewModel.ClipContent;
        }

        public bool SetItemSelectedPropertyAtIndex(int index, bool selected) {
            if (index < 0 || index >= this.Items.Count) {
                return false;
            }

            if (this.ItemContainerGenerator.ContainerFromIndex(index) is TimelineClipContainerControl resource) {
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
