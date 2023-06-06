using System;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline {
    /// <summary>
    /// A model that represents a timeline layer clip, such as a video or audio clip
    /// </summary>
    public abstract class ClipModel : IRBESerialisable, IDisposable {
        /// <summary>
        /// Returns the layer that this clip is currently in
        /// </summary>
        public LayerModel Layer { get; private set; }

        /// <summary>
        /// Returns the timeline associated with this clip. This is fetched from the <see cref="Layer"/> property, so this returns null if that it null
        /// </summary>
        public TimelineModel Timeline => this.Layer?.Timeline;

        /// <summary>
        /// Returns the project associated with this clip. This is fetched from the <see cref="Layer"/> property, so this returns null if that it null
        /// </summary>
        public ProjectModel Project => this.Layer?.Timeline.Project;

        /// <summary>
        /// Returns the resource manager associated with this clip. This is fetched from the <see cref="Layer"/> property, so this returns null if that it null
        /// </summary>
        public ResourceManager ResourceManager => this.Layer?.Timeline.Project.ResourceManager;

        public string DisplayName { get; set; }

        public string TypeId => ClipRegistry.Instance.GetTypeIdForModel(this.GetType());

        public bool IsDisposing { get; private set; }

        protected ClipModel() {

        }

        /// <summary>
        /// Sets the given clip's layer
        /// </summary>
        /// <param name="model">Model to set the layer of</param>
        /// <param name="layer">New layer</param>
        /// <param name="fireLayerChangedEvent">Whether to invoke the model's <see cref="OnAddedToLayer"/> function. If false, it must be called manually</param>
        public static void SetLayer(ClipModel model, LayerModel layer, bool fireLayerChangedEvent = true) {
            if (fireLayerChangedEvent) {
                LayerModel oldLayer = model.Layer;
                model.Layer = layer;
                model.OnAddedToLayer(oldLayer, layer);
            }
            else {
                model.Layer = layer;
            }
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            string typeId = this.TypeId;
            if (!data.TryGetString(nameof(this.TypeId), out string id) || id != typeId) {
                if (typeId == null) {
                    throw new Exception($"Model Type is not registered: {this.GetType()}");
                }
                else {
                    throw new Exception($"Model Type Id mis match. Data contained '{id}' but the registered type is {typeId}");
                }
            }

            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!(this.TypeId is string id))
                throw new Exception($"Model Type is not registered: {this.GetType()}");
            data.SetString(nameof(this.TypeId), id);
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
        }

        public abstract bool IntersectsFrameAt(long frame);

        protected virtual void OnAddedToLayer(LayerModel oldLayer, LayerModel newLayer) {

        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    this.DisporeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception($"{nameof(this.DisporeCore)} threw an unexpected exception", e));
                }
            }
        }

        public virtual void OnBeginDispose() {
            this.IsDisposing = true;
        }

        protected virtual void DisporeCore(ExceptionStack stack) {

        }

        public virtual void OnEndDispose() {
            this.IsDisposing = false;
        }
    }
}