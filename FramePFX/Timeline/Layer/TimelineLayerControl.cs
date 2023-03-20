using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Timeline.Layer.Clips;

namespace FramePFX.Timeline.Layer {
    public class TimelineLayerControl : MultiSelector {
        public static readonly DependencyProperty UnitZoomProperty =
            TimelineControl.UnitZoomProperty.AddOwner(
                typeof(TimelineLayerControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineLayerControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnitZoom(v)));

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

        internal Selector ParentSelector => ItemsControlFromItemContainer(this) as Selector;

        private bool isUpdatingUnitZoom;
        private bool isUpdatingLayerType;
        private TimelineControl timeline;

        public TimelineLayerControl() {
            this.LayerType = "Any";
            this.CanSelectMultipleItems = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is LayerViewModel vm) {
                    vm.Control = this;
                }
            };
        }

        public IEnumerable<TimelineClipControl> GetSelectedClipControls() {
            foreach (object item in this.SelectedItems) {
                if (ICGenUtils.GetContainerForItem<ClipViewModel, TimelineClipControl>(item, this.ItemContainerGenerator, x => x.Control) is TimelineClipControl clip) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ClipViewModel> GetSelectedClipModels() {
            foreach (object item in this.SelectedItems) {
                if (ICGenUtils.GetContainerForItem<TimelineClipControl, ClipViewModel>(item, this.ItemContainerGenerator, x => x.DataContext as ClipViewModel) is ClipViewModel clip) {
                    yield return clip;
                }
            }
        }

        public bool GetViewModel(out LayerViewModel layer) {
            return (layer = this.DataContext as LayerViewModel) != null;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TimelineClipControl() { TimelineLayer = this };
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is TimelineClipControl;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is TimelineClipControl clip) {
                if (item is ClipViewModel viewModel) {
                    viewModel.Control = clip;
                }
                // else {
                //     throw new Exception($"Expected item of type {nameof(ClipViewModel)}, got {item?.GetType()}");
                // }

                clip.TimelineLayer = this;
            }
            // else {
            //     throw new Exception($"Expected element of type {nameof(TimelineClipControl)}, got {element?.GetType()}");
            // }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            List<TimelineClipControl> clips = this.timeline.GetAllSelectedClipControls().ToList();
            if (clips.Any(clip => clip.TimelineLayer == this && clip.IsMouseOver)) {
                return;
            }

            clips.ForEach(x => {
                x.TimelineLayer.SelectedItems.Clear();
                x.IsSelected = false;
            });
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.isUpdatingUnitZoom) {
                if (!TimelineUtils.IsZoomEqual(oldZoom, newZoom)) {
                    throw new Exception("Recursive update of FrameOffset. Old = " + oldZoom + ", New = " + newZoom);
                }

                return;
            }

            this.isUpdatingUnitZoom = true;
            if (Math.Abs(oldZoom - newZoom) > TimelineUtils.MinUnitZoom) {
                foreach (TimelineClipControl element in this.GetElements()) {
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
            foreach (TimelineClipControl element in this.GetElements()) {
                element.OnLayerTypeChanged(oldType, newType);
            }

            this.isUpdatingLayerType = false;
        }

        public double GetRenderX(TimelineClipControl control) {
            return control.Margin.Left;
        }

        public void SetRenderX(TimelineClipControl control, double value) {
            Thickness margin = control.Margin;
            margin.Left = value;
            control.Margin = margin;
        }

        public double GetRenderY(TimelineClipControl control) {
            return control.Margin.Top;
        }

        public void SetRenderY(TimelineClipControl control, double value) {
            Thickness margin = control.Margin;
            margin.Top = value;
            control.Margin = margin;
        }

        public IEnumerable<TimelineClipControl> GetElements() {
            foreach (object item in this.Items) {
                if (item is TimelineClipControl clip) {
                    yield return clip;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is TimelineClipControl clip2) {
                    yield return clip2;
                }
            }
        }

        public TimelineClipControl CreateClonedElement(TimelineClipControl clipToCopy) {
            throw new Exception();
        }

        public TimelineClipControl CreateElement(int startFrame, int durationFrames) {
            throw new Exception();
        }

        /// <summary>
        /// Removes the given clip from this timeline layer
        /// </summary>
        /// <param name="clip"></param>
        public bool RemoveElement(TimelineClipControl clip) {
            int index = this.Items.IndexOf(clip);
            if (index == -1) {
                return false;
            }

            this.Items.RemoveAt(index);
            return true;
        }

        public void OnClipMouseAction(TimelineClipControl clip, MouseButtonEventArgs e, bool wasDragging = false) {
            if (e.ChangedButton == MouseButton.Left && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                clip.IsSelected = true;
                this.SelectedItem = clip;
                return;
            }

            if (e.ChangedButton == MouseButton.Left) {
                if (e.ButtonState == MouseButtonState.Pressed) {
                    if (this.Timeline.GetAllSelectedClipControls().ToList().Count >= 2) {
                        if (!clip.IsSelected) {
                            this.Timeline.SetPrimarySelection(this, clip);
                        }
                    }
                    else {
                        this.Timeline.SetPrimarySelection(this, clip);
                    }
                }
                else if (!wasDragging && this.Timeline.GetAllSelectedClipControls().ToList().Count > 1) {
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

        public void MakeTopElement(TimelineClipControl control) {
            if (this.GetViewModel(out LayerViewModel layer) && control.GetViewModel(out ClipViewModel clip)) {
                layer.MakeTopMost(clip);
            }
        }
    }
}
