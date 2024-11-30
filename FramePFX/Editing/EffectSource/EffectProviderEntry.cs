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

using FramePFX.Editing.Factories;
using FramePFX.Editing.Timelines.Effects;

namespace FramePFX.Editing.EffectSource;

public class EffectProviderEntry {
    public Type EffectType { get; }

    public string EffectId { get; }

    public string DisplayName { get; }

    public Action<BaseEffect> PostProcessor { get; }

    public EffectProviderEntry(string effectId, string displayName, Action<BaseEffect> postProcessor) {
        this.EffectType = EffectFactory.Instance.GetType(effectId);
        this.EffectId = effectId;
        this.DisplayName = displayName;
        this.PostProcessor = postProcessor;
    }

    public BaseEffect CreateEffect() {
        BaseEffect effect = EffectFactory.Instance.NewEffect(this.EffectId);
        this.PostProcessor?.Invoke(effect);
        return effect;
    }
}