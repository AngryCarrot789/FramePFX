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

using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Effects {
    /// <summary>
    /// A group which manages a collection of effects
    /// </summary>
    public class EffectListPropertyEditorGroup : BasePropertyEditorGroup {
        public IHaveEffects EffectOwner { get; private set; }

        public EffectListPropertyEditorGroup() : base(typeof(object)) {
            this.DisplayName = "Effects";
        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public void ClearHierarchy() {
            if (this.EffectOwner != null) {
                this.EffectOwner.EffectAdded -= this.OnEffectAdded;
                this.EffectOwner.EffectRemoved -= this.OnEffectRemoved;
                this.EffectOwner.EffectMoved -= this.OnEffectMoved;
            }

            for (int i = this.PropertyObjects.Count - 1; i >= 0; i--) {
                ((EffectPropertyEditorGroup) this.PropertyObjects[i]).ClearHierarchy();
                this.RemoveItemAt(i);
            }

            this.IsCurrentlyApplicable = false;
            this.EffectOwner = null;
        }

        /// <summary>
        /// Clears the hierarchy and then sets up this group's hierarchy for the given input list. If
        /// the <see cref="BasePropertyObjectViewModel.HandlerCountMode"/> for this group is unacceptable,
        /// then nothing else happens. If none of the input objects are applicable, then nothing happens. Otherwise,
        /// <see cref="BasePropertyObjectViewModel.IsCurrentlyApplicable"/> is set to true and the hierarchy is loaded
        /// </summary>
        /// <param name="input">Input list of objects</param>
        public virtual void SetupHierarchyState(IHaveEffects owner) {
            this.ClearHierarchy();
            if (owner == null) {
                return;
            }

            this.EffectOwner = owner;

            int i = 0;
            foreach (BaseEffect effect in owner.Effects) {
                this.InsertEffectInternal(effect, i++);
            }

            this.IsCurrentlyApplicable = this.PropertyObjects.Count > 0;
            if (this.IsCurrentlyApplicable) {
                owner.EffectAdded += this.OnEffectAdded;
                owner.EffectRemoved += this.OnEffectRemoved;
                owner.EffectMoved += this.OnEffectMoved;
            }
        }

        private void OnEffectAdded(IHaveEffects effects, BaseEffect effect, int index) {
            this.InsertEffectInternal(effect, index);
        }

        private void OnEffectRemoved(IHaveEffects effects, BaseEffect effect, int index) {
            this.RemoveItemAt(index);
        }

        private void OnEffectMoved(IHaveEffects effects, BaseEffect effect, int oldindex, int newindex) {
            this.MoveItem(oldindex, newindex);
        }

        private void InsertEffectInternal(BaseEffect effect, int index) {
            EffectPropertyEditorGroup fxEditor = EffectPropertyEditorGroup.NewInstanceFromEffect(effect);
            this.InsertItem(index, fxEditor);
            fxEditor.SetupEffect(effect);
        }

        public override bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj) {
            return obj is EffectPropertyEditorGroup;
        }
    }
}