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
        private readonly List<IPropertyEditorObject> propertyObjectList;
        private readonly HashSet<BasePropertyGroupViewModel> ignoreHandlerHierarchy;

        public override IReadOnlyList<IPropertyEditorObject> PropertyObjects => this.propertyObjectList;

        public FixedPropertyGroupViewModel(Type applicableType) : base(applicableType) {
            this.propertyObjectList = new List<IPropertyEditorObject>();
            this.idToGroupMap = new Dictionary<string, BasePropertyGroupViewModel>();
            this.idToEditorMap = new Dictionary<string, BasePropertyEditorViewModel>();
            this.ignoreHandlerHierarchy = new HashSet<BasePropertyGroupViewModel>();
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
        /// <param name="id">The unique ID for this group, relative to this group</param>
        /// <param name="isExpandedByDefault">Whether or not the group is expanded by default</param>
        /// <param name="canSetupHierarchy">
        /// Whether or not the group's hierarchy will be setup when the current instance's
        /// hierarchy is also setup (during the recursive setup). True by default
        /// </param>
        /// <param name="isSelectable">Whether or not this group is selectable in the UI. Default value is false</param>
        /// <returns>A new fixed group, which is added to the current instance's internal group collection</returns>
        public FixedPropertyGroupViewModel CreateFixedSubGroup(Type applicableType, string id, bool isExpandedByDefault = true, bool canSetupHierarchy = true, bool isSelectable = false) {
            if (canSetupHierarchy) {
                this.ValidateApplicableType(applicableType);
            }

            this.ValidateId(id);
            FixedPropertyGroupViewModel group = new FixedPropertyGroupViewModel(applicableType) {
                IsExpanded = isExpandedByDefault,
                DisplayName = id,
                IsSelectable = isSelectable
            };

            this.AddGroupInternal(group, id, canSetupHierarchy);
            return group;
        }

        /// <summary>
        /// Adds the given group to this group
        /// </summary>
        /// <param name="group">The group to add</param>
        /// <param name="id">A unique ID for this group</param>
        /// <param name="useSetupHandlers">
        /// Whether or not the group's hierarchy will be setup when the current instance's hierarchy is
        /// also setup (during the recursive setup). True by default. Set to false to use the new group
        /// primarily just to make the group structure look better or if you plan on using different types
        /// of handlers for this new group (e.g. a collection of effect handlers for a clip)
        /// </param>
        public void AddSubGroup(BasePropertyGroupViewModel group, string id, bool useSetupHandlers = true) {
            if (useSetupHandlers && group.ApplicableType != null) {
                this.ValidateApplicableType(group.ApplicableType);
            }

            this.ValidateId(id);
            this.AddGroupInternal(group, id, useSetupHandlers);
        }

        private void AddGroupInternal(BasePropertyGroupViewModel group, string id, bool useSetupHandlers) {
            if (this.Parent?.IsRoot ?? true) {
                group.IsHeaderBold = true;
            }

            group.Parent = this;
            group.RecalculateHierarchyDepth();
            this.idToGroupMap[id] = group;
            this.propertyObjectList.Add(group);
            if (!useSetupHandlers) {
                this.ignoreHandlerHierarchy.Add(group);
            }

            group.SetPropertyEditor(this.PropertyEditor);
        }

        public void AddPropertyEditor(string id, BasePropertyEditorViewModel editor) {
            if (this.idToEditorMap.ContainsKey(id))
                throw new Exception($"Editor already exists with the name: {id}");

            editor.Parent = this;
            editor.RecalculateHierarchyDepth();
            this.idToEditorMap[id] = editor;
            this.propertyObjectList.Add(editor);
            editor.SetPropertyEditor(this.PropertyEditor);
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
            IPropertyEditorObject lastEntry = null;
            List<IPropertyEditorObject> list = this.propertyObjectList;
            for (int i = 0, end = list.Count - 1; i <= end; i++) {
                IPropertyEditorObject obj = this.propertyObjectList[i];
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
                        if (!this.ignoreHandlerHierarchy.Contains(group)) {
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
            IPropertyEditorObject lastEntry = null;
            List<IPropertyEditorObject> list = this.propertyObjectList;
            for (int i = 0, end = list.Count - 1; i <= end; i++) {
                IPropertyEditorObject obj = this.propertyObjectList[i];
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

        public void AddSeparator(bool isEditorSeparator) {
            this.propertyObjectList.Add(new PropertyObjectSeparator(this, isEditorSeparator));
        }

        public bool IsDisconnectedFromHandlerHierarchy(FixedPropertyGroupViewModel group) {
            return this.ignoreHandlerHierarchy.Contains(group);
        }
    }
}