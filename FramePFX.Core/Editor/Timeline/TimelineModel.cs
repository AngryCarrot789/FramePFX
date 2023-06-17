using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Automation;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline {
    public class TimelineModel : IAutomatable, IRBESerialisable {
        public ProjectModel Project { get; }

        public long PlayHead { get; set; }

        public long MaxDuration { get; set; }

        public List<LayerModel> Layers { get; }

        /// <summary>
        /// This timeline's automation data
        /// </summary>
        public AutomationData AutomationData { get; }

        public TimelineModel(ProjectModel project) {
            this.Project = project;
            this.Layers = new List<LayerModel>();
            this.AutomationData = new AutomationData(this);
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetLong(nameof(this.PlayHead), this.PlayHead);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            RBEList list = data.CreateList(nameof(this.Layers));
            foreach (LayerModel layer in this.Layers) {
                if (!(layer.RegistryId is string registryId))
                    throw new Exception("Unknown layer type: " + layer.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(LayerModel.RegistryId), registryId);
                layer.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.PlayHead = data.GetLong(nameof(this.PlayHead));
            this.MaxDuration = data.GetLong(nameof(this.MaxDuration));
            foreach (RBEBase entry in data.GetList(nameof(this.Layers)).List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                string registryId = dictionary.GetString(nameof(LayerModel.RegistryId));
                LayerModel layer = LayerRegistry.Instance.CreateLayerModel(this, registryId);
                layer.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddLayer(layer);
            }

            this.MaxDuration = Math.Max(this.MaxDuration, this.Layers.Count < 1 ? 0 : this.Layers.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        public void AddLayer(LayerModel layer) {
            Validate.Exception(ReferenceEquals(layer.Timeline, this), "Expected layer's timeline and the current timeline instance to be equal");
            this.Layers.Add(layer);
        }

        public bool RemoveLayer(LayerModel layer) {
            Validate.Exception(ReferenceEquals(layer.Timeline, this), "Expected layer's timeline and the current timeline instance to be equal");
            int index = this.Layers.IndexOf(layer);
            if (index < 0) {
                return false;
            }

            this.Layers.RemoveAt(index);
            return true;
        }

        public void RemoveLayer(int index) {
            LayerModel layer = this.Layers[index];
            Validate.Exception(ReferenceEquals(layer.Timeline, this), "Expected layer's timeline and the current timeline instance to be equal");
            this.Layers.RemoveAt(index);
        }

        public void ClearLayers() {
            this.Layers.Clear();
        }

        public void Render(RenderContext render) {
            this.Render(render, this.PlayHead);
        }

        public void Render(RenderContext render, long frame) {
            for (int i = this.Layers.Count - 1; i >= 0; i--) {
                if (!(this.Layers[i] is VideoLayerModel layer)) {
                    continue;
                }

                List<ClipModel> clips = layer.Clips.Where(x => x.IntersectsFrameAt(frame)).ToList();
                if (clips.Count > 0) {
                    SKColorFilter filter = SKColorFilter.CreateBlendMode(new SKColor(0, 0, 0, (byte) Maths.Clamp(layer.Opacity * 255F, 0, 255F)), SKBlendMode.DstIn);
                    foreach (ClipModel clip in clips) {
                        render.Canvas.Save();
                        ((VideoClipModel) clip).Render(render, frame, filter);
                        render.Canvas.Restore();
                    }
                }

                // SKRect clipping = render.Canvas.LocalClipBounds;
                // int save = render.Canvas.SaveLayer(clipping, new SKPaint());
                // foreach (ClipModel clip in clipsOverFrame) {
                //     if (clip.IntersectsFrameAt(frame)) {
                //         render.Canvas.Save();
                //         ((VideoClipModel) clip).Render(render, frame);
                //         render.Canvas.Restore();
                //     }
                // }
                // byte alpha = (byte) Maths.Clamp(layer.Opacity * 255f, 0F, 255F);
                // SKColorFilter blend = SKColorFilter.CreateBlendMode(new SKColor(0, 0, 0, alpha), SKBlendMode.DstOut);
                // SKPaint paint = new SKPaint() {
                //     Style = SKPaintStyle.Fill,
                //     ColorFilter = blend
                // };
                // render.Canvas.DrawRect(clipping, paint);
                // render.Canvas.RestoreToCount(save);
            }
        }

        public IEnumerable<ClipModel> GetClipsAtFrame(long frame) {
            return Enumerable.Reverse(this.Layers).SelectMany(layer => layer.GetClipsAtFrame(frame));
        }
    }
}