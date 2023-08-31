using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// A class which contains a collection of child groups and editors
    /// </summary>
    public sealed class PropertyGroupViewModel : BasePropertyObjectViewModel {
        private readonly Dictionary<string, PropertyGroupViewModel> idToGroupMap;
        private readonly Dictionary<string, BasePropertyEditorViewModel> idToEditorMap;
        private readonly List<BasePropertyObjectViewModel> propertyObjectList;
        private bool isExpanded;

        public IReadOnlyList<BasePropertyObjectViewModel> PropertyObjects => this.propertyObjectList;

        public string Id { get; }

        /// <summary>
        /// Whether or not this group is expanded, showing the child groups and editors
        /// </summary>
        public bool IsExpanded {
            get => this.isExpanded;
            set => this.RaisePropertyChanged(ref this.isExpanded, value);
        }

        /// <summary>
        /// Whether or not the current group is the root property group object. Only one root group should exist per instance of <see cref="PropertyEditorRegistry"/>
        /// </summary>
        public bool IsRoot => this.Parent == null;

        public PropertyGroupViewModel(Type applicableType, string id) : base(applicableType) {
            this.Id = id;
            this.propertyObjectList = new List<BasePropertyObjectViewModel>();
            this.idToGroupMap = new Dictionary<string, PropertyGroupViewModel>();
            this.idToEditorMap = new Dictionary<string, BasePropertyEditorViewModel>();
        }

        public override void RecalculateHierarchyDepth() {
            base.RecalculateHierarchyDepth();
            foreach (BasePropertyObjectViewModel obj in this.propertyObjectList) {
                obj.RecalculateHierarchyDepth();
            }
        }

        /// <summary>
        /// Creates and adds a new child group object to this group
        /// </summary>
        /// <param name="applicableType">The applicable type. Must be assignable to the current group's applicable type</param>
        /// <param name="id"></param>
        /// <param name="isExpandedByDefault"></param>
        /// <returns></returns>
        public PropertyGroupViewModel CreateSubGroup(Type applicableType, string id, bool isExpandedByDefault = true) {
            if (this.ApplicableType != null && !this.ApplicableType.IsAssignableFrom(applicableType)) {
                throw new Exception($"The target type is not assignable to the current applicable type: {applicableType} # {this.ApplicableType}");
            }

            if (this.idToGroupMap.ContainsKey(id))
                throw new Exception($"Group already exists with the ID: {id}");

            PropertyGroupViewModel group = new PropertyGroupViewModel(applicableType, id) {
                isExpanded = isExpandedByDefault
            };

            group.Parent = this;
            group.RecalculateHierarchyDepth();
            this.idToGroupMap[id] = group;
            this.propertyObjectList.Add(group);
            return group;
        }

        public BasePropertyEditorViewModel GetEditorByName(string name) {
            return this.idToEditorMap.TryGetValue(name, out BasePropertyEditorViewModel editor) ? editor : null;
        }

        public PropertyGroupViewModel GetGroupByName(string name) {
            return this.idToGroupMap.TryGetValue(name, out PropertyGroupViewModel g) ? g : null;
        }

        public void AddPropertyEditor(string id, BasePropertyEditorViewModel editor) {
            if (this.idToEditorMap.ContainsKey(id))
                throw new Exception($"Editor already exists with the name: {id}");

            editor.Parent = this;
            editor.RecalculateHierarchyDepth();
            this.idToEditorMap[id] = editor;
            this.propertyObjectList.Add(editor);
        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public void ClearHierarchyState() {
            if (!this.IsCurrentlyApplicable) {
                return;
            }

            foreach (BasePropertyObjectViewModel obj in this.propertyObjectList) {
                switch (obj) {
                    case BasePropertyEditorViewModel editor:
                        editor.ClearHandlers();
                        break;
                    case PropertyGroupViewModel group:
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
        public void SetupHierarchyState(IReadOnlyList<object> input) {
            this.ClearHierarchyState();
            if (!this.IsHandlerCountAcceptable(input.Count)) {
                return;
            }

            if (!AreAnyApplicable(this, input)) {
                return;
            }

            this.IsCurrentlyApplicable = true;
            // TODO: maybe calculate every possible type from the given input (scanning each object's hierarchy
            // and adding each type to a HashSet), and then using that to check for applicability.
            // It would probably be slower for single selections, which is most likely what will be used...
            // but the performance difference for multi select would make it worth it tbh

            List<BasePropertyObjectViewModel> list = this.propertyObjectList;
            foreach (BasePropertyObjectViewModel obj in list) {
                switch (obj) {
                    case PropertyGroupViewModel group: {
                        group.SetupHierarchyState(input);
                        break;
                    }
                    case BasePropertyEditorViewModel editor: {
                        editor.SetHandlers(input);
                        break;
                    }
                }
            }
        }

        // These are more optimised versions of the enumerable versions. Hopefully they're faster

        private static bool AreAnyApplicable(BasePropertyObjectViewModel group, IReadOnlyList<object> sources) {
            // return sources.Any(x => group.IsApplicable(x));
            for (int i = 0, c = sources.Count; i < c; i++)
                if (group.IsApplicable(sources[i]))
                    return true;
            return false;
        }

        internal static bool AreAllApplicable(BasePropertyObjectViewModel editor, IReadOnlyList<object> sources) {
            // return sources.All(x => editor.IsApplicable(x));
            for (int i = 0, c = sources.Count; i < c; i++)
                if (!editor.IsApplicable(sources[i]))
                    return false;
            return true;
        }
    }
}