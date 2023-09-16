using System;
using System.Collections.Generic;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// An implementation of a property group that stores singleton instances of child groups and
    /// editors. This is useful for grouping together editors of non-dynamic data sources
    /// </summary>
    public class SingletonPropertyGroupViewModel : BasePropertyGroupViewModel {
        private readonly Dictionary<string, BasePropertyGroupViewModel> idToGroupMap;
        private readonly Dictionary<string, BasePropertyEditorViewModel> idToEditorMap;
        private readonly List<BasePropertyObjectViewModel> propertyObjectList;

        public IReadOnlyList<BasePropertyObjectViewModel> PropertyObjects => this.propertyObjectList;

        /// <summary>
        /// Whether or not the current group is the root property group object. Only one root group should exist per instance of <see cref="PropertyEditorRegistry"/>
        /// </summary>
        public bool IsRoot => this.Parent == null;

        public SingletonPropertyGroupViewModel(Type applicableType, string id) : base(applicableType, id) {
            this.propertyObjectList = new List<BasePropertyObjectViewModel>();
            this.idToGroupMap = new Dictionary<string, BasePropertyGroupViewModel>();
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
        public SingletonPropertyGroupViewModel CreateSubSingletonGroup(Type applicableType, string id, bool isExpandedByDefault = true) {
            if (this.ApplicableType != null && !this.ApplicableType.IsAssignableFrom(applicableType)) {
                throw new Exception($"The target type is not assignable to the current applicable type: {applicableType} # {this.ApplicableType}");
            }

            if (this.idToGroupMap.ContainsKey(id))
                throw new Exception($"Group already exists with the ID: {id}");

            SingletonPropertyGroupViewModel group = new SingletonPropertyGroupViewModel(applicableType, id) {
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

        public BasePropertyGroupViewModel GetGroupByName(string name) {
            return this.idToGroupMap.TryGetValue(name, out BasePropertyGroupViewModel g) ? g : null;
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
                    case SingletonPropertyGroupViewModel group:
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
                    case SingletonPropertyGroupViewModel group: {
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
    }
}