using System;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// An implementation of a property group that stores singleton instances of child groups and editors.
    /// This is useful for grouping together editors of non-dynamic data sources (e.g. editing the properties of handler objects)
    /// <para>
    /// A case where this may not be useful is when you want to have each handler have their own editor hierarchy, instead of one
    /// group being shared across multiple handlers. This is what <see cref="DynamicPropertyGroupViewModel"/> does, while also providing
    /// some optimisations
    /// </para>
    /// </summary>
    public class FixedPropertyGroupViewModel : BasePropertyGroupViewModel {
        private readonly Dictionary<string, BasePropertyGroupViewModel> idToGroupMap;
        private readonly Dictionary<string, BasePropertyEditorViewModel> idToEditorMap;
        private readonly List<IPropertyObject> propertyObjectList;
        private readonly HashSet<BasePropertyGroupViewModel> nonHierarchialGroups;

        public override IReadOnlyList<IPropertyObject> PropertyObjects => this.propertyObjectList;

        public FixedPropertyGroupViewModel(Type applicableType) : base(applicableType) {
            this.propertyObjectList = new List<IPropertyObject>();
            this.idToGroupMap = new Dictionary<string, BasePropertyGroupViewModel>();
            this.idToEditorMap = new Dictionary<string, BasePropertyEditorViewModel>();
            this.nonHierarchialGroups = new HashSet<BasePropertyGroupViewModel>();
        }

        public override void RecalculateHierarchyDepth() {
            base.RecalculateHierarchyDepth();
            foreach (BasePropertyObjectViewModel obj in this.propertyObjectList.OfType<BasePropertyObjectViewModel>()) {
                obj.RecalculateHierarchyDepth();
            }
        }

        private void ValidateApplicableType(Type type) {
            if (this.ApplicableType != null && !this.ApplicableType.IsAssignableFrom(type)) {
                throw new Exception($"The target type is not assignable to the current applicable type: {type} != {this.ApplicableType}");
            }
        }

        private void ValidateId(string id) {
            if (this.idToGroupMap.ContainsKey(id)) {
                throw new Exception($"Group already exists with the ID: {id}");
            }
        }

        /// <summary>
        /// Creates and adds a new child fixed group object to this group
        /// </summary>
        /// <param name="applicableType">The applicable type. Must be assignable to the current group's applicable type</param>
        /// <param name="id"></param>
        /// <param name="isExpandedByDefault"></param>
        /// <returns></returns>
        public FixedPropertyGroupViewModel CreateFixedSubGroup(Type applicableType, string id, bool isExpandedByDefault = true, bool isHierarchial = true) {
            if (isHierarchial) {
                this.ValidateApplicableType(applicableType);
            }

            this.ValidateId(id);
            FixedPropertyGroupViewModel group = new FixedPropertyGroupViewModel(applicableType) {
                IsExpanded = isExpandedByDefault,
                DisplayName = id
            };

            this.AddGroupInternal(group, id, isHierarchial);
            return group;
        }

        /// <summary>
        /// Creates and adds a new child group object to this group
        /// </summary>
        /// <param name="applicableType">The applicable type. Must be assignable to the current group's applicable type</param>
        /// <param name="id"></param>
        /// <param name="isExpandedByDefault"></param>
        /// <returns></returns>
        public DynamicPropertyGroupViewModel CreateDynamicSubGroup(Type applicableType, string id, bool isExpandedByDefault = true, bool isHierarchial = true) {
            if (isHierarchial) {
                this.ValidateApplicableType(applicableType);
            }

            this.ValidateId(id);
            DynamicPropertyGroupViewModel group = new DynamicPropertyGroupViewModel(applicableType) {
                IsExpanded = isExpandedByDefault,
                DisplayName = id
            };

            this.AddGroupInternal(group, id, isHierarchial);
            return group;
        }

        public void AddSubGroup(BasePropertyGroupViewModel group, string id, bool isHierarchial = true) {
            if (isHierarchial && group.ApplicableType != null) {
                this.ValidateApplicableType(group.ApplicableType);
            }

            this.ValidateId(id);
            this.AddGroupInternal(group, id, isHierarchial);
        }

        private void AddGroupInternal(BasePropertyGroupViewModel group, string id, bool isHierarchial) {
            if (this.Parent?.IsRoot ?? true) {
                group.IsHeaderBold = true;
            }

            group.Parent = this;
            group.RecalculateHierarchyDepth();
            this.idToGroupMap[id] = group;
            this.propertyObjectList.Add(group);
            if (!isHierarchial) {
                this.nonHierarchialGroups.Add(group);
            }
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
        /// Clears the hierarchy and then sets up this group's hierarchy for the given input list. If
        /// the <see cref="BasePropertyObjectViewModel.HandlerCountMode"/> for this group is unacceptable,
        /// then nothing else happens. If none of the input objects are applicable, then nothing happens. Otherwise,
        /// <see cref="BasePropertyObjectViewModel.IsCurrentlyApplicable"/> is set to true and the hierarchy is loaded
        /// </summary>
        /// <param name="input">Input list of objects</param>
        public override void SetupHierarchyState(IReadOnlyList<object> input) {
            this.ClearHierarchyState();
            if (!this.IsHandlerCountAcceptable(input.Count)) {
                return;
            }

            if (!AreAnyApplicable(this, input)) {
                return;
            }

            // TODO: maybe calculate every possible type from the given input (scanning each object's hierarchy
            // and adding each type to a HashSet), and then using that to check for applicability.
            // It would probably be slower for single selections, which is most likely what will be used...
            // but the performance difference for multi select would make it worth it tbh

            this.Handlers = input;
            bool isApplicable = false;
            IPropertyObject lastEntry = null;
            List<IPropertyObject> list = this.propertyObjectList;
            for (int i = 0, end = list.Count - 1; i <= end; i++) {
                IPropertyObject obj = this.propertyObjectList[i];
                if (obj is PropertyObjectSeparator separator) {
                    if (i == 0 || i == end || lastEntry is PropertyObjectSeparator || (lastEntry is BasePropertyObjectViewModel p && !p.IsCurrentlyApplicable)) {
                        separator.IsVisible = false;
                    }
                    else {
                        separator.IsVisible = true;
                    }

                    lastEntry = obj;
                    continue;
                }
                else {
                    if (obj is BasePropertyGroupViewModel group) {
                        if (!this.nonHierarchialGroups.Contains(group)) {
                            group.SetupHierarchyState(input);
                            isApplicable |= group.IsCurrentlyApplicable;
                        }
                    }
                    else if (obj is BasePropertyEditorViewModel editor) {
                        editor.SetHandlers(input);
                        isApplicable |= editor.IsCurrentlyApplicable;
                    }

                    if (lastEntry is PropertyObjectSeparator && obj is BasePropertyObjectViewModel p1 && !p1.IsCurrentlyApplicable) {
                        ((PropertyObjectSeparator) lastEntry).IsVisible = false;
                    }
                }

                lastEntry = obj;
            }

            this.IsCurrentlyApplicable = isApplicable;
        }

        public void CleanSeparators() {
            IPropertyObject lastEntry = null;
            List<IPropertyObject> list = this.propertyObjectList;
            for (int i = 0, end = list.Count - 1; i <= end; i++) {
                IPropertyObject obj = this.propertyObjectList[i];
                if (obj is PropertyObjectSeparator separator) {
                    if (i == 0 || i == end || lastEntry is PropertyObjectSeparator || (lastEntry is BasePropertyObjectViewModel prop && !prop.IsCurrentlyApplicable)) {
                        separator.IsVisible = false;
                    }
                    else {
                        separator.IsVisible = true;
                    }
                }
                else if (obj is BasePropertyObjectViewModel prop && lastEntry is PropertyObjectSeparator && !prop.IsCurrentlyApplicable) {
                    ((PropertyObjectSeparator) lastEntry).IsVisible = false;
                }

                lastEntry = obj;
            }
        }

        public void AddSeparator() {
            this.propertyObjectList.Add(new PropertyObjectSeparator());
        }

        public bool IsDisconnectedFromHierarchy(FixedPropertyGroupViewModel group) {
            return this.nonHierarchialGroups.Contains(group);
        }
    }
}