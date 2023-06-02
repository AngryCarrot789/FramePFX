using System;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.ResourceManaging {
    public abstract class ResourceItem : IRBESerialisable, IDisposable {
        public string TypeId => ResourceTypeRegistry.Instance.GetTypeIdForModel(this.GetType());

        /// <summary>
        /// This resource item's unique identifier
        /// </summary>
        public string Id { get; set; }

        public ResourceItem() {

        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!(this.TypeId is string id))
                throw new Exception($"Model Type is not registered: {this.GetType()}");
            data.SetString(nameof(this.TypeId), id);
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
        }

        /// <summary>
        /// Disposes this IO model. This should NOT be called from the destructor/finalizer
        /// </summary>
        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception($"Unexpected exception while invoking {nameof(this.DisposeCore)}", e));
                }
            }
        }

        /// <summary>
        /// The core method for disposing of sources and outputs. This method really should not throw,
        /// and instead, exceptions should be added to the given <see cref="ExceptionStack"/>
        /// </summary>
        /// <param name="stack">The exception stack in which exception should be added into when encountered during disposal</param>
        /// <param name="isDisposing"></param>
        protected virtual void DisposeCore(ExceptionStack stack) {

        }
    }
}