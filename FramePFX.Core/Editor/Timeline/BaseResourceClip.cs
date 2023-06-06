using System;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline {
    /// <summary>
    /// A base video clip that references a single resource
    /// </summary>
    /// <typeparam name="T">The resource item type</typeparam>
    public abstract class BaseResourceClip<T> : VideoClipModel where T : ResourceItem {
        public delegate void ClipResourceModifiedEventHandler(T resource, string property);
        public delegate void ClipResourceChangedEventHandler(T oldItem, T newItem);

        private readonly ResourceModifiedEventHandler dataModifiedHandler;
        private readonly ResourcePath<T>.ResourceChangedEventHandler resourceChangedHandler;

        public ResourcePath<T> ResourcePath { get; private set; }

        public event ClipResourceChangedEventHandler ClipResourceChanged;
        public event ClipResourceModifiedEventHandler ClipResourceDataModified;

        protected BaseResourceClip() {
            this.dataModifiedHandler = this.OnResourceDataModifiedInternal;
            this.resourceChangedHandler = this.OnResourceChangedInternal;
        }

        protected override void OnAddedToLayer(LayerModel oldLayer, LayerModel newLayer) {
            base.OnAddedToLayer(oldLayer, newLayer);
            this.ResourcePath?.SetManager(newLayer?.Timeline.Project.ResourceManager);
        }

        public void SetTargetResourceId(string id) {
            if (this.ResourcePath != null) {
                this.ResourcePath.ResourceChanged -= this.resourceChangedHandler;
                this.ResourcePath.Dispose();
            }

            this.ResourcePath = new ResourcePath<T>(this.Layer?.Timeline.Project.ResourceManager, id);
            this.ResourcePath.ResourceChanged += this.resourceChangedHandler;
        }

        private void OnResourceChangedInternal(T oldItem, T newItem) {
            if (oldItem != null)
                oldItem.DataModified -= this.dataModifiedHandler;
            if (newItem != null)
                newItem.DataModified += this.dataModifiedHandler;
            this.OnResourceChanged(oldItem, newItem);
            this.ClipResourceChanged?.Invoke(oldItem, newItem);
        }

        private void OnResourceDataModifiedInternal(ResourceItem sender, string property) {
            if (this.ResourcePath == null)
                throw new InvalidOperationException("Expected resource path to be non-null");
            if (!this.ResourcePath.IsCachedItemEqualTo(sender))
                throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
            this.OnResourceDataModified(property);
            this.ClipResourceDataModified?.Invoke((T) sender, property);
        }

        protected virtual void OnResourceChanged(T oldItem, T newItem) {
            this.InvalidateRender();
        }

        protected virtual void OnResourceDataModified(string property) {
            this.InvalidateRender();
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (this.ResourcePath != null)
                ResourcePath<T>.WriteToRBE(this.ResourcePath, data.GetOrCreateDictionaryElement(nameof(this.ResourcePath)));
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetElement(nameof(this.ResourcePath), out RBEDictionary resource))
                this.ResourcePath = ResourcePath<T>.ReadFromRBE(this.Layer?.Timeline.Project.ResourceManager, resource);
        }

        public bool TryGetResource(out T resource) {
            if (this.ResourcePath == null) {
                resource = null;
                return false;
            }

            return this.ResourcePath.TryGetResource(out resource);
        }

        protected override void DisporeCore(ExceptionStack stack) {
            base.DisporeCore(stack);
            if (this.ResourcePath != null && !this.ResourcePath.IsDisposed) {
                try {
                    // this shouldn't throw unless it was already disposed for some reason. Might as well handle that case
                    this.ResourcePath?.Dispose();
                }
                catch (Exception e) {
                    stack.Push(e);
                }
            }
        }
    }
}