using System;
using System.Collections.Generic;
using FramePFX.Core.RBC;
using FramePFX.Core.ResourceManaging;

namespace FramePFX.Core.Editor.Timeline {
    public class ClipModel : IRBESerialisable {
        public TimelineLayerModel Layer { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this clip is currently in the process of being removed from it's owning layer
        /// </summary>
        public bool IsBeingRemoved { get; set; }

        public string TypeId => ClipRegistry.Instance.GetTypeIdForModel(this.GetType());

        public ClipModel() {

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
    }
}