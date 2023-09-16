using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// An implementation of a property group that stores singleton instances of child groups and editors.
    /// This is useful for grouping together editors of non-dynamic data sources (e.g. editing handlers' properties)
    /// <para>
    /// A case where this may not be useful is when trying to edit the properties of objects stored in a collection within a handler, where
    /// that collection may change; that requires multiple instances of the same editor, which is what <see cref="DynamicPropertyGroupViewModel"/> does
    /// </para>
    /// </summary>
    public sealed class FixedPropertyGroupViewModel : BasePropertyGroupViewModel {
        private readonly Dictionary<string, BasePropertyGroupViewModel> idToGroupMap;
        private readonly Dictionary<string, BasePropertyEditorViewModel> idToEditorMap;
        private readonly List<BasePropertyObjectViewModel> propertyObjectList;

        public override IReadOnlyList<BasePropertyObjectViewModel> PropertyObjects => this.propertyObjectList;

        public FixedPropertyGroupViewModel(Type applicableType) : base(applicableType) {
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
        public FixedPropertyGroupViewModel CreateFixedSubGroup(Type applicableType, string id, bool isExpandedByDefault = true) {
            this.ValidateApplicableType(applicableType);
            this.ValidateId(id);
            FixedPropertyGroupViewModel group = new FixedPropertyGroupViewModel(applicableType) {
                IsExpanded = isExpandedByDefault,
                DisplayName = id
            };

            this.AddGroupInternal(group, id);
            return group;
        }

        /// <summary>
        /// Creates and adds a new child group object to this group
        /// </summary>
        /// <param name="applicableType">The applicable type. Must be assignable to the current group's applicable type</param>
        /// <param name="id"></param>
        /// <param name="isExpandedByDefault"></param>
        /// <returns></returns>
        public DynamicPropertyGroupViewModel CreateDynamicSubGroup(Type applicableType, string id, bool isExpandedByDefault = true) {
            this.ValidateApplicableType(applicableType);
            this.ValidateId(id);
            DynamicPropertyGroupViewModel group = new DynamicPropertyGroupViewModel(applicableType) {
                IsExpanded = isExpandedByDefault,
                DisplayName = id
            };

            this.AddGroupInternal(group, id);
            return group;
        }

        private void AddGroupInternal(BasePropertyGroupViewModel group, string id) {
            group.Parent = this;
            group.RecalculateHierarchyDepth();
            this.idToGroupMap[id] = group;
            this.propertyObjectList.Add(group);
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

            this.IsCurrentlyApplicable = true;
            // TODO: maybe calculate every possible type from the given input (scanning each object's hierarchy
            // and adding each type to a HashSet), and then using that to check for applicability.
            // It would probably be slower for single selections, which is most likely what will be used...
            // but the performance difference for multi select would make it worth it tbh

            foreach (BasePropertyObjectViewModel obj in this.propertyObjectList) {
                switch (obj) {
                    case BasePropertyGroupViewModel group: {
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
    }
}