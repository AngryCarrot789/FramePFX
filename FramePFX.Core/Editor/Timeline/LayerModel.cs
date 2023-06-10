using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.Timeline {
    /// <summary>
    /// A model that represents a timeline layer, which can contain clips
    /// </summary>
    public abstract class LayerModel : IRBESerialisable {
        public TimelineModel Timeline { get; }

        public List<ClipModel> Clips { get; }

        public string RegistryId => LayerRegistry.Instance.GetTypeIdForModel(this.GetType());

        public string Name { get; set; }
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
        public double Height { get; set; }
        public string LayerColour { get; set; }

        public LayerModel(TimelineModel timeline) {
            this.Timeline = timeline;
            this.Clips = new List<ClipModel>();
            this.MinHeight = 40;
            this.MaxHeight = 200;
            this.Height = 60;
            this.LayerColour = LayerColours.GetRandomColour();
        }

        public IEnumerable<ClipModel> GetClipsAtFrame(long frame) {
            return this.Clips.Where(clip => clip.IntersectsFrameAt(frame));
        }

        public void AddClip(ClipModel model, bool setLayer = true) {
            this.InsertClip(this.Clips.Count, model, setLayer);
        }

        public void InsertClip(int index, ClipModel model, bool setLayer = true) {
            this.Clips.Insert(index, model);
            if (setLayer) {
                ClipModel.SetLayer(model, this);
            }
        }

        public bool RemoveClip(ClipModel model, bool clearLayer = true) {
            int index = this.Clips.IndexOf(model);
            if (index >= 0) {
                this.RemoveClipAt(index, clearLayer);
                return true;
            }

            return false;
        }

        public void RemoveClipAt(int index, bool clearLayer = true) {
            ClipModel clip = this.Clips[index];
            if (!ReferenceEquals(this, clip.Layer)) {
                throw new Exception("Expected model (to remove)'s layer to equal this instance");
            }

            this.Clips.RemoveAt(index);
            if (clearLayer) {
                ClipModel.SetLayer(clip, null);
            }
        }

        public abstract LayerModel CloneCore();

        public void WriteToRBE(RBEDictionary data) {
            data.SetString(nameof(this.Name), this.Name);
            data.SetDouble(nameof(this.MinHeight), this.MinHeight);
            data.SetDouble(nameof(this.MaxHeight), this.MaxHeight);
            data.SetDouble(nameof(this.Height), this.Height);
            data.SetString(nameof(this.LayerColour), this.LayerColour);
            RBEList list = data.CreateList(nameof(this.Clips));
            foreach (ClipModel clip in this.Clips) {
                if (!(clip.FactoryId is string id))
                    throw new Exception("Unknown clip type: " + clip.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(ClipModel.FactoryId), id);
                clip.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Name = data.GetString(nameof(this.Name), null);
            this.MinHeight = data.GetDouble(nameof(this.MinHeight), 40);
            this.MaxHeight = data.GetDouble(nameof(this.MaxHeight), 200);
            this.Height = data.GetDouble(nameof(this.Height), 60);
            this.LayerColour = data.GetString(nameof(this.LayerColour), LayerColours.GetRandomColour());
            foreach (RBEBase entry in data.GetList(nameof(this.Clips)).List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                string id = dictionary.GetString(nameof(ClipModel.FactoryId));
                ClipModel clip = ClipRegistry.Instance.CreateLayerModel(id);
                clip.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddClip(clip);
            }
        }
    }
}