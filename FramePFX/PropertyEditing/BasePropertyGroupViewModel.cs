using System;
using System.Collections.Generic;

namespace FramePFX.PropertyEditing {
    public abstract class BasePropertyGroupViewModel : BasePropertyObjectViewModel {
        private bool isExpanded;

        /// <summary>
        /// Whether or not this group is expanded, showing the child groups and editors
        /// </summary>
        public bool IsExpanded {
            get => this.isExpanded;
            set => this.RaisePropertyChanged(ref this.isExpanded, value);
        }

        private bool isSelected;
        public bool IsSelected {
            get => this.isSelected;
            set => this.RaisePropertyChanged(ref this.isSelected, value);
        }

        /// <summary>
        /// Whether or not the current group is the root property group object. Only one root group should exist per instance of <see cref="PropertyEditorRegistry"/>
        /// </summary>
        public bool IsRoot => this.Parent == null;

        /// <summary>
        /// Gets a read-only list of all base property objects currently stored in this group. This
        /// typically remains un-changed for <see cref="FixedPropertyGroupViewModel"/>
        /// </summary>
        public abstract IReadOnlyList<BasePropertyObjectViewModel> PropertyObjects { get; }

        public string DisplayName { get; set; } = "Group";

        protected BasePropertyGroupViewModel(Type applicableType) : base(applicableType) {

        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public virtual void ClearHierarchyState() {
            if (!this.IsCurrentlyApplicable) {
                return;
            }

            foreach (BasePropertyObjectViewModel obj in this.PropertyObjects) {
                switch (obj) {
                    case BasePropertyEditorViewModel editor:
                        editor.ClearHandlers();
                        break;
                    case BasePropertyGroupViewModel group:
                        group.ClearHierarchyState();
                        break;
                }
            }

            this.IsCurrentlyApplicable = false;
        }

        /// <summary>
        /// Clears the hierarchy and then sets up this group's hierarchy for the given input list. If
        /// the <see cref="BasePropertyObjectViewModel.HandlerCountMode"/> for this group is unacceptable,
        /// then nothing else happens. If none of the input objects are applicable, then nothing happens. Otherwise,
        /// <see cref="BasePropertyObjectViewModel.IsCurrentlyApplicable"/> is set to true and the hierarchy is loaded
        /// </summary>
        /// <param name="input">Input list of objects</param>
        public abstract void SetupHierarchyState(IReadOnlyList<object> input);

        protected static bool AreAnyApplicable(BasePropertyObjectViewModel group, IReadOnlyList<object> sources) {
            // return sources.Any(x => group.IsApplicable(x));
            for (int i = 0, c = sources.Count; i < c; i++)
                if (group.IsApplicable(sources[i]))
                    return true;
            return false;
        }
    }
}