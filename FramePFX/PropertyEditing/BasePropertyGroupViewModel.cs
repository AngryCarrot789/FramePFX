using System;
using System.Collections.Generic;
using FramePFX.AdvancedContextService;
using FramePFX.Commands;

namespace FramePFX.PropertyEditing {
    public abstract class BasePropertyGroupViewModel : BasePropertyObjectViewModel, IContextProvider {
        private IReadOnlyList<object> handlers;
        private bool isExpanded;
        private bool isSelected;

        /// <summary>
        /// Whether or not this group is expanded, showing the child groups and editors
        /// </summary>
        public bool IsExpanded {
            get => this.isExpanded;
            set {
                this.RaisePropertyChanged(ref this.isExpanded, value);
                this.ExpandCommand.RaiseCanExecuteChanged();
                this.CollapseCommand.RaiseCanExecuteChanged();
            }
        }

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
        public abstract IReadOnlyList<IPropertyObject> PropertyObjects { get; }

        /// <summary>
        /// Gets a list of handlers that are currently active (updated during a call to <see cref="SetupHierarchyState"/>)
        /// </summary>
        public IReadOnlyList<object> Handlers {
            get => this.handlers;
            protected set => this.RaisePropertyChanged(ref this.handlers, value);
        }

        public string DisplayName { get; set; } = "Group";

        public RelayCommand ExpandCommand { get; }
        public RelayCommand CollapseCommand { get; }

        public RelayCommand ExpandHierarchyCommand { get; }
        public RelayCommand CollapseHierarchyCommand { get; }

        protected BasePropertyGroupViewModel(Type applicableType) : base(applicableType) {
            this.ExpandCommand = new RelayCommand(() => this.IsExpanded = true, () => !this.IsExpanded);
            this.CollapseCommand = new RelayCommand(() => this.IsExpanded = false, () => this.IsExpanded);
            this.ExpandHierarchyCommand = new RelayCommand(this.ExpandHierarchy);
            this.CollapseHierarchyCommand = new RelayCommand(this.CollapseHierarchy);
        }

        protected void ExpandHierarchy() {
            this.IsExpanded = true;
            foreach (IPropertyObject obj in this.PropertyObjects) {
                if (obj is BasePropertyGroupViewModel group) {
                    group.ExpandHierarchy();
                }
            }
        }

        protected void CollapseHierarchy() {
            // probably more performant to expand the top first, so that closing child ones won't cause rendering
            this.IsExpanded = false;
            foreach (IPropertyObject obj in this.PropertyObjects) {
                if (obj is BasePropertyGroupViewModel group) {
                    group.CollapseHierarchy();
                }
            }
        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public virtual void ClearHierarchyState() {
            if (!this.IsCurrentlyApplicable && !this.IsRoot) {
                return;
            }

            foreach (IPropertyObject obj in this.PropertyObjects) {
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
            this.Handlers = null;
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

        public virtual void GetContext(List<IContextEntry> list) {
            list.Add(!this.IsExpanded ? new CommandContextEntry("Expand", this.ExpandCommand) : new CommandContextEntry("Collapse", this.CollapseCommand));
            list.Add(new CommandContextEntry("Expand Hierarchy", this.ExpandHierarchyCommand));
            list.Add(new CommandContextEntry("Collapse Hierarchy", this.CollapseHierarchyCommand));
        }
    }
}