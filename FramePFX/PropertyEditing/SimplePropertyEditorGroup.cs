using System;
using System.Collections.Generic;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// A general property editor group, which supports a single list of handler objects
    /// </summary>
    public class PropertyEditorGroup : BasePropertyEditorGroup {
        public IReadOnlyList<object> Handlers { get; private set; }

        public PropertyEditorGroup(Type applicableType) : base(applicableType) {

        }

        public override bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj) {
            return obj is PropertyEditorSlot || obj is BasePropertyEditorGroup;
        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public void ClearHierarchy() {
            if (!this.IsCurrentlyApplicable && !this.IsRoot) {
                return;
            }

            foreach (BasePropertyEditorObject obj in this.PropertyObjects) {
                switch (obj) {
                    case PropertyEditorSlot editor:
                        editor.ClearHandlers();
                        break;
                    case PropertyEditorGroup group:
                        group.ClearHierarchy();
                        break;
                }
            }

            this.IsCurrentlyApplicable = false;
            this.Handlers = null;
        }

        /// <summary>
        /// Clears the hierarchy and then sets up this group's hierarchy for the given input list. If
        /// the <see cref="BasePropertyObjectViewModel.HandlerCountMode"/> for this group is unacceptable,
        /// then nothing else happens. If none of the input objects are applicable, then nothing happens. Otherwise,
        /// <see cref="BasePropertyObjectViewModel.IsCurrentlyApplicable"/> is set to true and the hierarchy is loaded
        /// </summary>
        /// <param name="input">Input list of objects</param>
        public virtual void SetupHierarchyState(IReadOnlyList<object> input) {
            this.ClearHierarchy();
            if (!this.IsHandlerCountAcceptable(input.Count)) {
                return;
            }

            if (!AreAnyApplicable(this, input)) {
                return;
            }

            // maybe calculate every possible type from the given input (scanning each object's hierarchy
            // and adding each type to a HashSet), and then using that to check for applicability.
            // It would probably be slower for single selections, which is most likely what will be used...
            // but the performance difference for multi select would make it worth it tbh

            this.Handlers = input;
            bool isApplicable = false;
            for (int i = 0, end = this.PropertyObjects.Count - 1; i <= end; i++) {
                BasePropertyEditorObject obj = this.PropertyObjects[i];
                if (obj is PropertyEditorGroup group) {
                    group.SetupHierarchyState(input);
                    isApplicable |= group.IsCurrentlyApplicable;
                }
                else if (obj is PropertyEditorSlot editor) {
                    editor.SetHandlers(input);
                    isApplicable |= editor.IsCurrentlyApplicable;
                }
            }

            this.IsCurrentlyApplicable = isApplicable;
        }

        protected static bool AreAnyApplicable(PropertyEditorGroup group, IReadOnlyList<object> sources) {
            // return sources.Any(x => group.IsApplicable(x));
            for (int i = 0, c = sources.Count; i < c; i++)
                if (group.IsObjectApplicable(sources[i]))
                    return true;
            return false;
        }
    }
}