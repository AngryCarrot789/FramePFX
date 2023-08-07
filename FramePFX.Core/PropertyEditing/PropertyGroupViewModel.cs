using System;
using System.Collections.Generic;

namespace FramePFX.Core.PropertyEditing
{
    /// <summary>
    /// A class which contains a collection of child groups and editors
    /// </summary>
    public class PropertyGroupViewModel : BasePropertyObjectViewModel
    {
        private readonly Dictionary<string, PropertyGroupViewModel> idToGroupMap;
        private readonly Dictionary<string, BasePropertyEditorViewModel> idToEditorMap;
        private readonly List<BasePropertyObjectViewModel> propertyObjectList;
        private bool isExpanded;

        public IReadOnlyList<BasePropertyObjectViewModel> PropertyObjects => this.propertyObjectList;

        public string Id { get; }

        /// <summary>
        /// Whether or not this group is expanded, showing the child groups and editors
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set => this.RaisePropertyChanged(ref this.isExpanded, value);
        }

        public PropertyGroupViewModel(Type applicableType, string id) : base(applicableType)
        {
            this.Id = id;
            this.propertyObjectList = new List<BasePropertyObjectViewModel>();
            this.idToGroupMap = new Dictionary<string, PropertyGroupViewModel>();
            this.idToEditorMap = new Dictionary<string, BasePropertyEditorViewModel>();
        }

        /// <summary>
        /// Creates and adds a new child group object to this group
        /// </summary>
        /// <param name="applicableType">The applicable type. Must be assignable to the current group's applicable type</param>
        /// <param name="id"></param>
        /// <param name="isExpandedByDefault"></param>
        /// <returns></returns>
        public PropertyGroupViewModel CreateSubGroup(Type applicableType, string id, bool isExpandedByDefault = true)
        {
            //                                  i think this is the right way around...
            if (this.ApplicableType != null && !applicableType.IsAssignableFrom(this.ApplicableType))
            {
                throw new Exception($"The target type is not assignable to the current applicable type: {applicableType} # {this.ApplicableType}");
            }

            if (this.idToGroupMap.ContainsKey(id))
                throw new Exception($"Group already exists with the ID: {id}");

            PropertyGroupViewModel group = new PropertyGroupViewModel(applicableType, id) {
                isExpanded = isExpandedByDefault
            };

            this.idToGroupMap[id] = group;
            this.propertyObjectList.Add(@group);
            return group;
        }

        public BasePropertyEditorViewModel GetEditorByName(string name)
        {
            return this.idToEditorMap.TryGetValue(name, out BasePropertyEditorViewModel editor) ? editor : null;
        }

        public PropertyGroupViewModel GetGroupByName(string name)
        {
            return this.idToGroupMap.TryGetValue(name, out PropertyGroupViewModel g) ? g : null;
        }

        public void AddPropertyEditor(string id, BasePropertyEditorViewModel editor)
        {
            if (this.idToEditorMap.ContainsKey(id))
                throw new Exception($"Editor already exists with the name: {id}");

            this.idToEditorMap[id] = editor;
            this.propertyObjectList.Add(editor);
        }

        public void ClearHandlersRecursive()
        {
            foreach (BasePropertyObjectViewModel obj in this.propertyObjectList)
            {
                switch (obj)
                {
                    case BasePropertyEditorViewModel editor:
                        editor.ClearHandlers();
                        break;
                    case PropertyGroupViewModel group:
                        group.ClearHandlersRecursive();
                        break;
                }
            }

            this.IsCurrentlyApplicable = false;
        }

        public void SetupHierarchyState(IReadOnlyList<object> input)
        {
            if (input.Count < 1)
            {
                throw new Exception("Cannot setup hierarchy with an empty list");
            }

            // TODO: maybe calculate every possible type from the given input (scanning each object's hierarchy
            // and adding each type to a HashSet), and then using that to check for applicability.
            // It would probably be slower for single selections, which is most likely what will be used...
            // but the performance difference for multi select would make it worth it tbh

            List<BasePropertyObjectViewModel> list = this.propertyObjectList;
            foreach (BasePropertyObjectViewModel obj in list)
            {
                if (!obj.IsHandlerCountAcceptable(input.Count))
                {
                    continue;
                }

                switch (obj)
                {
                    case PropertyGroupViewModel group:
                    {
                        group.IsCurrentlyApplicable = AreAnyApplicable(group, input);
                        if (group.IsCurrentlyApplicable)
                        {
                            group.SetupHierarchyState(input);
                        }

                        break;
                    }
                    case BasePropertyEditorViewModel editor:
                    {
                        // TODO: maybe only load handlers for applicable objects, and ignore the other ones?
                        editor.IsCurrentlyApplicable = AreAllApplicable(editor, input);
                        if (editor.IsCurrentlyApplicable)
                        {
                            editor.SetHandlers(input);
                        }

                        break;
                    }
                }
            }
        }

        // These are more optimised versions of the enumerable versions. Hopefully they're faster

        private static bool AreAnyApplicable(BasePropertyObjectViewModel group, IReadOnlyList<object> sources)
        {
            // return sources.Any(x => group.IsApplicable(x));
            for (int i = 0, c = sources.Count; i < c; i++)
                if (group.IsApplicable(sources[i]))
                    return true;
            return false;
        }

        private static bool AreAllApplicable(BasePropertyObjectViewModel editor, IReadOnlyList<object> sources)
        {
            // return sources.All(x => editor.IsApplicable(x));
            for (int i = 0, c = sources.Count; i < c; i++)
                if (!editor.IsApplicable(sources[i]))
                    return false;
            return true;
        }
    }
}