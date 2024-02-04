using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines.Effects;

namespace FramePFX.Editors.EffectSource {
    public class EffectProviderManager {
        public static EffectProviderManager Instance { get; } = new EffectProviderManager();

        private readonly List<EffectProviderEntry> entries;

        public ReadOnlyCollection<EffectProviderEntry> Entries { get; }

        private EffectProviderManager() {
            this.entries = new List<EffectProviderEntry>();
            this.Entries = this.entries.AsReadOnly();

            this.RegisterEffect<MotionEffect>("Motion Effect", null);
            this.RegisterEffect<PixelateEffect>("Pixelate Effect", (p) => p.BlockSize = 16);
        }

        public void RegisterEffect<T>(string displayName, Action<T> postProcessor) where T : BaseEffect {
            string id = EffectFactory.Instance.GetId(typeof(T));
            Action<BaseEffect> postProc = null;
            if (postProcessor != null)
                postProc = fx => postProcessor((T) fx);
            this.entries.Add(new EffectProviderEntry(id, displayName, postProc));
        }
    }
}