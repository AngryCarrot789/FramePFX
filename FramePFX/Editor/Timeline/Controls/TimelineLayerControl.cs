using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Core;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public class BaseTimelineLayerControl : MultiSelector, ILayerHandle {
        public static readonly DependencyProperty ResourceDropHandlerProperty =
            DependencyProperty.Register(
                "ResourceDropHandler",
                typeof(IResourceDropHandler),
                typeof(BaseTimelineLayerControl),
                new PropertyMetadata(null));

        public IResourceDropHandler ResourceDropHandler {
            get => (IResourceDropHandler) this.GetValue(ResourceDropHandlerProperty);
            set => this.SetValue(ResourceDropHandlerProperty, value);
        }

        /// <summary>
        /// The zoom level of the associated timeline, or 1, if no timeline is present
        /// </summary>
        public double UnitZoom => this.Timeline?.UnitZoom ?? 1D;

        /// <summary>
        /// The timeline that contains this layer
        /// </summary>
        public TimelineControl Timeline => ItemsControlFromItemContainer(this) as TimelineControl;

        public LayerViewModel ViewModel => this.DataContext as LayerViewModel;

        protected bool isProcessingDrop;

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

        public IEnumerable<T> GetItemContainers<T>() where T : TimelineClipControl {
            return this.GetClipContainers<T>(this.Items);
        }

        public IEnumerable<T> GetSelectedItemContainers<T>() where T : TimelineClipControl {
            return this.GetClipContainers<T>(this.Items);
        }

        public IEnumerable<T> GetClipContainers<T>(IEnumerable items) where T : TimelineClipControl {
            int i = 0;
            foreach (object item in items) {
                if (item is T a) {
                    yield return a;
                }
                else if (this.ItemContainerGenerator.ContainerFromIndex(i) is T b) {
                    yield return b;
                }

                i++;
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e) {
            if (this.ResourceDropHandler == null) {
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
                long frame = TimelineUtils.PixelToFrame(mouse.X, layer.UnitZoom);
                frame = Maths.Clamp(frame, 0, this.Timeline?.MaxDuration ?? 0);
                await this.ResourceDropHandler.OnResourceDropped(item, frame);
            }

            this.isProcessingDrop = false;
        }

        public void OnUnitZoomChanged() {
            foreach (TimelineClipControl element in this.GetClipContainers<TimelineClipControl>()) {
                element.OnUnitZoomChanged();
            }
        }
    }
}