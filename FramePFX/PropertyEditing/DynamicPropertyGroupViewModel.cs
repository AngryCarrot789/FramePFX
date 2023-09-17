using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Utils;
using OpenTK.Input;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// An implementation of a property group that dynamically creates instances of child groups (which may or may not
    /// be dynamic too, most likely not though) when the hierarchy is being setup. This is useful when trying to edit
    /// the properties of objects stored in a collection within a handler, where that collection may change
    /// <para>
    /// This class will attempt to assign multiple handlers to the groups, but it may not be possible depending on the
    /// present handlers (e.g. multiple handlers of the same type; you probably don't want to share the same data for each)
    /// </para>
    /// <para>
    /// This type of group has a slight performance penalty over <see cref="FixedPropertyGroupViewModel"/> in that this class
    /// will need to create a new instance of the child hierarchies each time <see cref="SetupHierarchyState"/> is invoked
    /// </para>
    /// </summary>
    public class DynamicPropertyGroupViewModel : BasePropertyGroupViewModel {
        private readonly ObservableCollection<BasePropertyObjectViewModel> activeObjects;
        private readonly Dictionary<Type, TypeRegistration> registrations;

        public override IReadOnlyList<IPropertyObject> PropertyObjects => this.activeObjects;

        /// <summary>
        /// Whether or not this group should only use 1 handler per child group. When false,
        /// this group will attempt to merge multiple of the same type of handler into a single group.
        /// <para>
        /// This is true by default
        /// </para>
        /// <para>
        /// This property is only used for the standard <see cref="SetupHierarchyState"/>
        /// </para>
        /// </summary>
        public bool UseSingleHandlerPerGroup { get; set; }

        /// <summary>
        /// Whether or not there must be the same number of a specific handler type in each list
        /// of handlers when using <see cref="SetupHierarchyStateExtended"/>.
        /// <para>
        /// Default value is true, meaning that if, for example, you have 3 input lists where A contains the
        /// handlers [1,2,3,3], B contains [1,5,5,5,3,4] and C contains [1,5,4], only the handler 1 will actually be applicable.
        /// When this is false, in the above example, 1, 2, and 4 will be the only handlers applicable
        /// </para>
        /// </summary>
        public bool RequireAllHandlerListsHaveCommonHandlerCountForExtendedMode { get; set; }

        public DynamicPropertyGroupViewModel(Type applicableType) : base(applicableType) {
            this.registrations = new Dictionary<Type, TypeRegistration>();
            this.activeObjects = new ObservableCollection<BasePropertyObjectViewModel>();
            this.RequireAllHandlerListsHaveCommonHandlerCountForExtendedMode = true;
            this.UseSingleHandlerPerGroup = true;
        }


        private void AddGroupInternal(BasePropertyGroupViewModel group) {
            group.Parent = this;
            group.RecalculateHierarchyDepth();
            this.activeObjects.Add(group);
        }

        /// <summary>
        /// Registers a dynamic group type
        /// </summary>
        /// <param name="type">The lowest type of object that the constructor creates. This type must also be applicable to this dynamic group's applicable type</param>
        /// <param name="displayName">A readable name for the group</param>
        /// <param name="constructor"></param>
        /// <param name="handlerCountMode"></param>
        /// <exception cref="Exception"></exception>
        public void RegisterType(Type type, string displayName, Func<bool?, BasePropertyGroupViewModel> constructor, HandlerCountMode handlerCountMode = HandlerCountMode.Any) {
            if (this.registrations.ContainsKey(type)) {
                throw new Exception("Type already registered: " + type);
            }

            this.registrations[type] = new TypeRegistration(this, type, displayName, handlerCountMode, constructor);
        }

        public override void SetupHierarchyState(IReadOnlyList<object> input) {
            this.ClearHierarchyState();
            int count = input.Count;
            if (!this.IsHandlerCountAcceptable(count)) {
                return;
            }

            if (!AreAnyApplicable(this, input)) {
                return;
            }

            // example input lists:
            //  motion, motion, contrast
            //  motion, contrast

            if (this.UseSingleHandlerPerGroup) {
                for (int i = 0; i < count; i++) {
                    object handler = input[i];
                    Type type = handler.GetType();
                    if (!this.registrations.TryGetValue(type, out TypeRegistration registration))
                        continue;
                    if (!IsHandlerCountAcceptable(registration.handlerCountMode, 1))
                        continue;

                    BasePropertyGroupViewModel group = registration.GetSingleHandlerGroup();
                    group.SetupHierarchyState(new SingletonList<object>(handler));
                    this.AddGroupInternal(group);
                }
            }
            else {
                Dictionary<TypeRegistration, List<object>> counter = new Dictionary<TypeRegistration, List<object>>();
                // use a list here to try and maintain some of the original order, even if some
                // groups get collapsed into 1 (via multiple handlers)
                List<TypeRegistration> typeUsage = new List<TypeRegistration>();
                for (int i = 0; i < count; i++) {
                    object handler = input[i];
                    Type type = handler.GetType();
                    if (!this.registrations.TryGetValue(type, out TypeRegistration registration)) {
                        continue;
                    }

                    if (!counter.TryGetValue(registration, out List<object> list)) {
                        counter[registration] = list = new List<object>(1);
                        typeUsage.Add(registration);
                    }

                    list.Add(handler);
                }

                if (typeUsage.Count < 1) {
                    return;
                }

                // TODO: maybe recycle some of these objects in a Dictionary<Type, WeakReference<BasePropertyGroupViewModel>>?

                foreach (TypeRegistration registration in typeUsage) {
                    List<object> list = counter[registration];
                    HandlerCountMode mode = registration.handlerCountMode;
                    if (!IsHandlerCountAcceptable(mode, list.Count)) {
                        continue;
                    }

                    switch (mode) {
                        case HandlerCountMode.Any:
                        case HandlerCountMode.Multi: {
                            BasePropertyGroupViewModel group = registration.NewDynamicGroup(list.Count == 1);
                            group.SetupHierarchyState(list);
                            this.AddGroupInternal(group);
                            break;
                        }
                        case HandlerCountMode.Single: {
                            foreach (object handler in list) {
                                BasePropertyGroupViewModel group = registration.NewDynamicGroup(true);
                                group.SetupHierarchyState(new SingletonList<object>(handler));
                                this.AddGroupInternal(group);
                            }

                            break;
                        }
                        default: throw new Exception("Registration has an invalid handler count mode");
                    }
                }
            }

            if (this.activeObjects.Count > 0) {
                this.IsCurrentlyApplicable = true;
            }
        }

        public void SetupHierarchyStateExtended(IReadOnlyList<IReadOnlyList<object>> inputLists) {
            this.ClearHierarchyState();
            if (inputLists.Count < 1) {
                return;
            }

            if (inputLists.Count == 1) {
                this.SetupHierarchyState(inputLists[0]);
                return;
            }

            int total = inputLists.CountAll(x => x.Count);
            if (!this.IsHandlerCountAcceptable(total)) {
                return;
            }

            // This will be calculated based on the below code checking for registered types
            // if (!AreAnyApplicable(this, input))
            //     return;

            // example input lists:
            //  [motion, motion, contrast], [motion, contrast, twirl], [motion, motion, twirl]
            // in this case, only contrast and twirl should be shown, because there's no way to merge those
            // 3 'motion' effects into 1 or 2 editors, at least, not without either labelling each
            // effect so they are uniquely identifiable or asking the user how they want to group them,
            // ... or unless all inner lists were semantically the same (same types at the same indices)

            // use a list here to try and maintain some of the original order, even if some
            // groups get collapsed into 1 (via multiple handlers)
            List<TypeRegistration> typeUsage = new List<TypeRegistration>();
            Dictionary<TypeRegistration, List<object>> counter = new Dictionary<TypeRegistration, List<object>>();
            Dictionary<TypeRegistration, object> inner_counter = new Dictionary<TypeRegistration, object>();
            foreach (IReadOnlyList<object> input in inputLists) {
                int count = input.Count;
                for (int i = 0; i < count; i++) {
                    object handler = input[i];
                    Type type = handler.GetType();
                    if (!this.registrations.TryGetValue(type, out TypeRegistration registration)) {
                        continue;
                    }

                    // Here, we only want to allow 1 of the same type of registration per inner list
                    // If there are multiple, then set the handler to null, which allows it to get ignored
                    // but also takes up a slot in the dictionary meaning it never gets set to a non-null value
                    if (!inner_counter.ContainsKey(registration)) {
                        inner_counter[registration] = handler;
                    }
                    else {
                        // we encountered multiple handlers for the same type in another list...
                        // that's no good, so we remove it from the possible handlers to be used
                        if (counter.ContainsKey(registration)) {
                            counter[registration] = null;
                            typeUsage.Remove(registration);
                        }

                        // Still takes up a slot, but can be used in the outer loop to check
                        // if the type registration can no longer be used
                        inner_counter[registration] = null;
                    }
                }

                foreach (KeyValuePair<TypeRegistration,object> entry in inner_counter.ToList()) {
                    if (entry.Value != null) {
                        if (!counter.TryGetValue(entry.Key, out List<object> list)) {
                            counter[entry.Key] = list = new List<object>();
                            typeUsage.Add(entry.Key);
                        }

                        list.Add(entry.Value);

                        // remove the key so that it has a chance of being used in another list
                        inner_counter.Remove(entry.Key);
                    }
                }
            }

            if (this.RequireAllHandlerListsHaveCommonHandlerCountForExtendedMode) {
                foreach (KeyValuePair<TypeRegistration, List<object>> entry in counter.ToList()) {
                    if (entry.Value.Count != inputLists.Count) {
                        counter.Remove(entry.Key);
                        typeUsage.Remove(entry.Key);
                    }
                }
            }

            if (typeUsage.Count < 1) {
                return;
            }

            // TODO: maybe recycle some of these objects in a Dictionary<Type, WeakReference<BasePropertyGroupViewModel>>?

            foreach (TypeRegistration registration in typeUsage) {
                List<object> list = counter[registration];
                HandlerCountMode mode = registration.handlerCountMode;
                if (!IsHandlerCountAcceptable(mode, list.Count)) {
                    continue;
                }

                switch (mode) {
                    case HandlerCountMode.Any:
                    case HandlerCountMode.Multi: {
                        BasePropertyGroupViewModel group = registration.NewDynamicGroup(list.Count == 1);
                        group.SetupHierarchyState(list);
                        this.AddGroupInternal(group);
                        break;
                    }
                    case HandlerCountMode.Single: {
                        foreach (object handler in list) {
                            BasePropertyGroupViewModel group = registration.NewDynamicGroup(true);
                            group.SetupHierarchyState(new SingletonList<object>(handler));
                            this.AddGroupInternal(group);
                        }

                        break;
                    }
                    default: throw new Exception("Registration has an invalid handler count mode");
                }
            }

            if (this.activeObjects.Count > 0) {
                this.IsCurrentlyApplicable = true;
            }
        }

        public override void ClearHierarchyState() {
            if (!this.IsCurrentlyApplicable) {
                return;
            }

            List<BasePropertyObjectViewModel> items = this.activeObjects.ToList();
            this.activeObjects.Clear();
            foreach (BasePropertyObjectViewModel obj in items) {
                ((BasePropertyGroupViewModel) obj).ClearHierarchyState();
                obj.Parent = null;
            }

            this.IsCurrentlyApplicable = false;
        }

        private class TypeRegistration {
            private readonly Type type;
            private readonly string id; // used for debugging... for now
            public readonly HandlerCountMode handlerCountMode;
            private readonly Func<bool?, BasePropertyGroupViewModel> constructor;
            private BasePropertyGroupViewModel singleHandlerInstance;
            private readonly DynamicPropertyGroupViewModel group;

            public TypeRegistration(DynamicPropertyGroupViewModel group, Type type, string id, HandlerCountMode handlerCountMode, Func<bool?, BasePropertyGroupViewModel> constructor) {
                this.group = group;
                this.type = type;
                this.id = id;
                this.handlerCountMode = handlerCountMode;
                this.constructor = constructor;
            }

            /// <summary>
            /// Creates an instance of this registration's group
            /// </summary>
            /// <param name="isUsingSingleHandler">
            /// True meaning it will only be used for a single handler, false meaning it
            /// may be used for more than 1 handler, or null meaning it can be used for both
            /// </param>
            /// <returns></returns>
            public BasePropertyGroupViewModel NewDynamicGroup(bool? isUsingSingleHandler) {
                return this.ProcessObject(this.constructor(isUsingSingleHandler));
            }

            public BasePropertyGroupViewModel GetSingleHandlerGroup() {
                return this.singleHandlerInstance ?? (this.singleHandlerInstance = this.ProcessObject(this.constructor(true)));
            }

            private BasePropertyGroupViewModel ProcessObject(BasePropertyGroupViewModel obj) {
                if (obj == null)
                    throw new Exception("Constructed object was null");
                if (obj.ApplicableType == null)
                    throw new Exception("Constructed object has a null applicable type");
                if (!this.type.IsAssignableFrom(obj.ApplicableType))
                    throw new Exception($"Constructed object's applicable type is not assignable to the current applicable type: {obj.ApplicableType} != {this.type}");
                if (!string.IsNullOrWhiteSpace(this.id))
                    obj.DisplayName = this.id;
                return obj;
            }
        }
    }
}