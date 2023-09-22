using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Utils;

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
        private IReadOnlyList<IReadOnlyList<object>> extendedHandlerList;

        public override IReadOnlyList<IPropertyEditorObject> PropertyObjects => this.activeObjects;

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

        public IReadOnlyList<IReadOnlyList<object>> ExtendedHandlerList {
            get => this.extendedHandlerList;
            protected set => this.RaisePropertyChanged(ref this.extendedHandlerList, value);
        }

        public DynamicMode CurrentMode { get; private set; }

        public DynamicPropertyGroupViewModel(Type applicableType) : base(applicableType) {
            this.registrations = new Dictionary<Type, TypeRegistration>();
            this.activeObjects = new ObservableCollection<BasePropertyObjectViewModel>();
            this.RequireAllHandlerListsHaveCommonHandlerCountForExtendedMode = true;
            this.UseSingleHandlerPerGroup = true;
        }


        private void AddAndSetup(BasePropertyGroupViewModel group, IReadOnlyList<object> handlers) {
            // WPF will attempt to begin binding as soon as the group is added to the activeObjects list,
            // but most bindings rely on the state of the group having handlers, so this is a little hack that
            group.Parent = this;
            group.RecalculateHierarchyDepth();
            group.SetupHierarchyState(handlers);
            this.activeObjects.Add(group);
        }

        /// <summary>
        /// Registers a dynamic group type
        /// </summary>
        /// <param name="type">The lowest type of object that the constructor creates. This type must also be applicable to this dynamic group's applicable type</param>
        /// <param name="displayName">A readable name for the group</param>
        /// <param name="constructor">
        /// A function that creates an instance of the group. The boolean parameter is labelled 'isUsingSingleHandler',
        /// which is used to determine if the group is only going to be used used for a single handler (true),
        /// multiple handlers (false), or unknown (null). When the bool is null, the function should combine the structure
        /// of the single and multi handlers into one. This is the default value
        /// </param>
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

            // example input list:
            //   [ motion, motion, contrast ]

            this.Handlers = input;
            if (this.UseSingleHandlerPerGroup) {
                this.CurrentMode = DynamicMode.SingleHandlerPerSubGroup;
                Dictionary<TypeRegistration, int> inUseTypes = new Dictionary<TypeRegistration, int>();
                for (int i = 0; i < count; i++) {
                    object handler = input[i];
                    Type type = handler.GetType();
                    if (!this.registrations.TryGetValue(type, out TypeRegistration registration))
                        continue;
                    if (!IsHandlerCountAcceptable(registration.handlerCountMode, 1))
                        continue;
                    inUseTypes.TryGetValue(registration, out int inUse);
                    BasePropertyGroupViewModel instance = registration.GetSingleHandlerInstance(handler);
                    instance.DisplayName = inUse > 0 ? $"{registration.id} ({inUse})" : registration.id;
                    this.AddAndSetup(instance, CollectionUtils.Singleton(handler));
                    inUseTypes[registration] = inUse + 1;
                }
            }
            else {
                this.CurrentMode = DynamicMode.MultipleHandlersPerSubGroup;
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
                            this.AddAndSetup(registration.NewGroupInstance(list.Count == 1), list);
                            break;
                        }
                        case HandlerCountMode.Single: {
                            foreach (object handler in list) {
                                this.AddAndSetup(registration.NewGroupInstance(true), CollectionUtils.Singleton(handler));
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

            // example input:
            //   [ [ motion, motion, contrast ], [ motion, contrast ] ]

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
            this.CurrentMode = DynamicMode.Extended;
            this.ExtendedHandlerList = inputLists;
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
                        this.AddAndSetup(registration.NewGroupInstance(list.Count == 1), list);
                        break;
                    }
                    case HandlerCountMode.Single: {
                        foreach (object handler in list) {
                            this.AddAndSetup(registration.NewGroupInstance(true), CollectionUtils.Singleton(handler));
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
            if (this.CurrentMode == DynamicMode.SingleHandlerPerSubGroup && this.Handlers.Count > 0) {
                foreach (object handler in this.Handlers) {
                    if (this.registrations.TryGetValue(handler.GetType(), out TypeRegistration registration)) {
                        registration.OnSingleHandlerInstanceCleared(handler);
                    }
                }
            }

            this.CurrentMode = DynamicMode.Inactive;
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
            public readonly string id;
            public readonly HandlerCountMode handlerCountMode;
            private readonly Func<bool?, BasePropertyGroupViewModel> constructor;
            private readonly DynamicPropertyGroupViewModel owner;

            // TODO: Maybe make this key constant?
            // then, make it map to a dictionary, which maps the type to a cached BasePropertyGroupViewModel.
            // this way, the view model has the ability to try and serialise the object when writing to RBE
            // so that, during a project save, stuff like the expansion state can be saved
            private readonly string ViewModelDataKey;

            public TypeRegistration(DynamicPropertyGroupViewModel owner, Type type, string id, HandlerCountMode handlerCountMode, Func<bool?, BasePropertyGroupViewModel> constructor) {
                this.owner = owner;
                this.type = type;
                this.id = id;
                this.handlerCountMode = handlerCountMode;
                this.constructor = constructor;
                this.ViewModelDataKey = "CachedPropertyObject_" + this.type.Name;
            }

            /// <summary>
            /// Creates an instance of this registration's group
            /// </summary>
            /// <param name="isUsingSingleHandler">
            /// True meaning it will only be used for a single handler, false meaning it
            /// may be used for more than 1 handler, or null meaning it can be used for both. This
            /// is purely an optimisation feature; the behaviour without this parameter
            /// would be like passing null to the constructor
            /// </param>
            /// <returns></returns>
            public BasePropertyGroupViewModel NewGroupInstance(bool? isUsingSingleHandler) {
                return this.ProcessObject(this.constructor(isUsingSingleHandler));
            }

            public BasePropertyGroupViewModel GetSingleHandlerInstance(object handler) {
                if (handler is BaseViewModel viewModel) {
                    if (!TryGetInternalData(viewModel, this.ViewModelDataKey, out BasePropertyGroupViewModel g)) {
                        SetInternalData(viewModel, this.ViewModelDataKey, g = this.NewGroupInstance(true));
                    }

                    return g;
                }
                else {
                    return this.NewGroupInstance(true);
                }
            }

            public void OnSingleHandlerInstanceCleared(object handler) {
                if (!(handler is BaseViewModel viewModel)) {
                    return;
                }

                if (TryGetInternalData(viewModel, this.ViewModelDataKey, out BasePropertyGroupViewModel g)) {
                    g.ClearHierarchyState();
                }
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