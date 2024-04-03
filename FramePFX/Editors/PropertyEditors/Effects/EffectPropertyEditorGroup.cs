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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FramePFX.Editors.PropertyEditors.Effects.Pixelate;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Effects
{
    /// <summary>
    /// A property editor group that encapsulates a single instance of an effect.
    /// For now, the effect property editor only supports single selection
    /// </summary>
    public class EffectPropertyEditorGroup : BasePropertyEditorGroup
    {
        private static readonly Dictionary<Type, Func<EffectPropertyEditorGroup>> Constructors;

        public BaseEffect Effect { get; private set; }

        public EffectPropertyEditorGroup(Type applicableType) : base(applicableType)
        {
            this.DisplayName = applicableType.Name;
        }

        static EffectPropertyEditorGroup()
        {
            Constructors = new Dictionary<Type, Func<EffectPropertyEditorGroup>>();
            RegisterEffect<CPUPixelateEffect>(() => new CPUPixelateEffectPropertyEditorGroup());
        }

        public static void RegisterEffect<T>(Func<EffectPropertyEditorGroup> constructor) where T : BaseEffect
        {
            Constructors.Add(typeof(T), constructor ?? throw new ArgumentNullException(nameof(constructor)));
        }

        public void SetupEffect(BaseEffect effect)
        {
            if (!(this.Parent is EffectListPropertyEditorGroup parent))
            {
                throw new InvalidOperationException("This group is not placed in a " + nameof(EffectListPropertyEditorGroup));
            }

            if (parent.EffectOwner == null)
            {
                throw new InvalidOperationException("This group's parent's effect owner is null");
            }

            this.ClearHierarchy();

            IReadOnlyList<object> handlers = new List<object>() { effect };

            bool isApplicable = false;
            for (int i = 0, end = this.PropertyObjects.Count - 1; i <= end; i++)
            {
                BasePropertyEditorObject obj = this.PropertyObjects[i];
                if (obj is SimplePropertyEditorGroup group)
                {
                    group.SetupHierarchyState(handlers);
                    isApplicable |= group.IsCurrentlyApplicable;
                }
                else if (obj is PropertyEditorSlot editor)
                {
                    editor.SetHandlers(handlers);
                    isApplicable |= editor.IsCurrentlyApplicable;
                }
            }

            if (isApplicable)
            {
                this.Effect = effect;
            }

            this.IsCurrentlyApplicable = isApplicable;
        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public void ClearHierarchy()
        {
            if (!this.IsCurrentlyApplicable && !this.IsRoot)
            {
                return;
            }

            foreach (BasePropertyEditorObject obj in this.PropertyObjects)
            {
                switch (obj)
                {
                    case PropertyEditorSlot editor:
                        editor.ClearHandlers();
                        break;
                    case SimplePropertyEditorGroup group:
                        group.ClearHierarchy();
                        break;
                }
            }

            this.IsCurrentlyApplicable = false;
            this.Effect = null;
        }

        public override bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj)
        {
            return obj is PropertyEditorSlot || obj is BasePropertyEditorGroup;
        }

        public static EffectPropertyEditorGroup NewInstanceFromEffect(BaseEffect effect)
        {
            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            bool hasLogged = false;
            for (Type type = effect.GetType(); type != null; type = type.BaseType)
            {
                if (Constructors.TryGetValue(type, out Func<EffectPropertyEditorGroup> func))
                {
                    return func();
                }

                if (!hasLogged)
                {
                    hasLogged = true;
                    Debugger.Break();
                    Debug.WriteLine("Could not find effect editor group constructor for effect type on first try. Scanning base types");
                }
            }

            throw new Exception("No such editor group registered for effect: " + effect.GetType());
        }
    }
}