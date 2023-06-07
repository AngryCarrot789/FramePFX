using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public class BaseTimelineLayerControl : MultiSelector, ILayerHandle {
        public static readonly DependencyProperty DropNotifierProperty =
            DependencyProperty.Register(
                "DropNotifier",
                typeof(ILayerDropable),
                typeof(BaseTimelineLayerControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty UnitZoomProperty =
            TimelineControl.UnitZoomProperty.AddOwner(
                typeof(BaseTimelineLayerControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((BaseTimelineLayerControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public ILayerDropable DropNotifier {
            get => (ILayerDropable) this.GetValue(DropNotifierProperty);
            set => this.SetValue(DropNotifierProperty, value);
        }

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
        /// The timeline that owns/contains this timeline layer
        /// </summary>
        public TimelineControl Timeline {
            get => this.timeline;
            set => this.timeline = value;
        }

        public LayerViewModel ViewModel => this.DataContext as LayerViewModel;

        protected bool isUpdatingUnitZoom;
        protected TimelineControl timeline;

        public BaseTimelineLayerControl() {
            this.CanSelectMultipleItems = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is LayerViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(ILayerHandle), this);
                }
            };

            this.AllowDrop = true;
            this.Drop += this.OnDrop;
            this.DragEnter += this.OnDragEnter;
        }

        public long GetFrameFromPixel(double pixel) {
            return TimelineUtils.PixelToFrame(pixel, this.UnitZoom);
        }

        public virtual double GetRenderX(TimelineVideoClipControl control) {
            return control.Margin.Left;
        }

        public virtual void SetRenderX(TimelineVideoClipControl control, double value) {
            Thickness margin = control.Margin;
            margin.Left = value;
            control.Margin = margin;
        }

        public IEnumerable<BaseTimelineClipControl> GetElements() {
            foreach (object item in this.Items) {
                if (item is BaseTimelineClipControl clip) {
                    yield return clip;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is BaseTimelineClipControl clip2) {
                    yield return clip2;
                }
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e) {
            if (this.DropNotifier == null) {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        protected bool isProcessingDrop;

        private void OnDrop(object sender, DragEventArgs e) {
            if (this.isProcessingDrop || this.DropNotifier == null) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent("ResourceItem")) {
                object obj = e.Data.GetData("ResourceItem");
                if (obj is ResourceItem resource) {
                    this.isProcessingDrop = true;
                    this.OnDropResource(resource, e.GetPosition(this));
                }
            }
        }

        protected async void OnDropResource(ResourceItem item, Point mouse) {
            // lazy
            if (item.IsRegistered && this is VideoTimelineLayerControl layer) {
                long frame = layer.GetFrameFromPixel(mouse.X);
                frame = Maths.Clamp(frame, 0, this.Timeline?.MaxDuration ?? 0);
                await this.DropNotifier.OnResourceDropped(item, frame);
            }

            this.isProcessingDrop = false;
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
                foreach (BaseTimelineClipControl element in this.GetElements()) {
                    element.UnitZoom = newZoom;
                }
            }

            this.isUpdatingUnitZoom = false;
        }
    }
}