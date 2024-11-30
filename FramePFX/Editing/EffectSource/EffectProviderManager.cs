//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Collections.ObjectModel;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Factories;
using FramePFX.Editing.Timelines.Effects;

namespace FramePFX.Editing.EffectSource;

public class EffectProviderManager {
    public static EffectProviderManager Instance { get; } = new EffectProviderManager();

    private readonly List<EffectProviderEntry> entries;

    public ReadOnlyCollection<EffectProviderEntry> Entries { get; }

    private EffectProviderManager() {
        this.entries = new List<EffectProviderEntry>();
        this.Entries = this.entries.AsReadOnly();

        this.RegisterEffect<CPUPixelateEffect>("CPU Pixelate Effect", (p) => p.SetDefaultValue(CPUPixelateEffect.BlockSizeParameter, 16));
    }

    public void RegisterEffect<T>(string displayName, Action<T> postProcessor) where T : BaseEffect {
        string id = EffectFactory.Instance.GetId(typeof(T));
        Action<BaseEffect> postProc = null;
        if (postProcessor != null)
            postProc = fx => postProcessor((T) fx);
        this.entries.Add(new EffectProviderEntry(id, displayName, postProc));
    }
}