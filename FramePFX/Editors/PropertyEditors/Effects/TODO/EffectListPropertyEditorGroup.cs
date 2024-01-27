using System;
using System.Collections.Generic;
using FramePFX.Editors.Timelines;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Effects.TODO {
    /// <summary>
    /// A property editor group that contains dynamically created <see cref="EffectPropertyEditorGroup"/> objects (that
    /// represent the effects themselves) based on an input list of objects that can have effects applied (e.g. clips or tracks)
    /// </summary>
    public class EffectListPropertyEditorGroup : BasePropertyEditorGroup {
        private readonly Dictionary<Type, Registration> effectToRegistration;

        public EffectListPropertyEditorGroup(Type applicableType) : base(applicableType) {
            this.effectToRegistration = new Dictionary<Type, Registration>();
        }

        /// <summary>
        /// Registers an effect type to be presentable in this property editor group. The given type should extend
        /// <see cref="EffectPropertyEditorGroup"/>. It is used in combination with the <see cref="HandlerCountMode"/>
        /// to dynamically create an instance of the <see cref="EffectPropertyEditorGroup"/>, but a cached instance may
        /// also be used, so it is important that the objects's constructor always creates a similar object for the handler
        /// count mode each time it is called, otherwise horrible UI bugs and crashes may occur
        /// </summary>
        /// <param name="effectType">The type of effect</param>
        /// <param name="handlerCount">The targeted handler count mode</param>
        /// <param name="typeOfEffectGroup">The type of object to target with the specific handler count mode</param>
        public void RegisterType(Type effectType, HandlerCountMode handlerCount, Type typeOfEffectGroup) {
            if (!this.effectToRegistration.TryGetValue(effectType, out var registration)) {
                this.effectToRegistration[effectType] = registration = new Registration(this, effectType);
            }

            switch (handlerCount) {
                case HandlerCountMode.Any:  registration.anyCountEffectGroupType = typeOfEffectGroup; break;
                case HandlerCountMode.Single:  registration.singleEffectGroupType = typeOfEffectGroup; break;
                case HandlerCountMode.Multi:  registration.multiEffectGroupType = typeOfEffectGroup; break;
                default: throw new ArgumentOutOfRangeException(nameof(handlerCount), handlerCount, null);
            }
        }

        public void ClearHierarchyState() {
        }

        public void SetupHierarchy(List<IHaveEffects> objects) {
            this.ClearHierarchyState();
        }

        public override bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj) {
            return obj is PropertyEditorSlot || obj is BasePropertyEditorGroup;
        }

        private class Registration {
            private readonly EffectListPropertyEditorGroup effectList;
            public readonly Type effectType;
            public Type anyCountEffectGroupType;
            public Type singleEffectGroupType;
            public Type multiEffectGroupType;

            public Registration(EffectListPropertyEditorGroup effectList, Type effectType) {
                this.effectList = effectList;
                this.effectType = effectType;
            }
        }
    }
}