using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FramePFX.Actions.Contexts;
using FramePFX.Utils;
using FramePFX.Utils.Collections;

namespace FramePFX.Interactivity {
    /// <summary>
    /// A class that supports basic type-based drag-drop processing. This class contains a dictionary which
    /// maps a droppable object type to another dictionary which maps the handler/target type to the handler functions
    /// </summary>
    /// <typeparam name="THandler">The base handler type. This is only here to help reduce bugs</typeparam>
    public class DragDropRegistry<THandler> where THandler : class {
        private delegate EnumDropType CustomCanDropDelegate(object target, object drop, EnumDropType dropType, IDataContext context);
        private delegate Task CustomOnDroppedDelegate(object target, object drop, EnumDropType dropType, IDataContext context);
        private delegate EnumDropType NativeCanDropDelegate(object target, IDataObjekt drop, EnumDropType dropType, IDataContext context);
        private delegate Task NativeOnDroppedDelegate(object target, IDataObjekt drop, EnumDropType dropType, IDataContext context);

        // [dropType -> [handler -> func]]
        private readonly Dictionary<Type, InheritanceDictionary<CustomHandlerPair>> registryCustom;
        private readonly Dictionary<string, InheritanceDictionary<NativeHandlerPair>> registryNative;

        public DragDropRegistry() {
            this.registryCustom = new Dictionary<Type, InheritanceDictionary<CustomHandlerPair>>();
            this.registryNative = new Dictionary<string, InheritanceDictionary<NativeHandlerPair>>();
        }

        public void Register<T, TValue>(
            Func<T, TValue, EnumDropType, IDataContext, EnumDropType> canDrop,
            Func<T, TValue, EnumDropType, IDataContext, Task> onDropped)
            where T : THandler {
            Type dropType = typeof(TValue);
            Type handlerType = typeof(T);

            if (!this.registryCustom.TryGetValue(dropType, out InheritanceDictionary<CustomHandlerPair> handlerMap)) {
                this.registryCustom[dropType] = handlerMap = new InheritanceDictionary<CustomHandlerPair>();
            }
            else if (handlerMap.HasLocalValue(handlerType)) {
                throw new Exception("Handler type already registered: " + handlerType.Name);
            }

            handlerMap.SetValue(handlerType, new CustomHandlerPair((h, d, t, c) => canDrop((T) h, (TValue) d, t, c), (h, d, t, c) => onDropped((T) h, (TValue) d, t, c)));
        }

        public void RegisterNative<T>(
            string dropType,
            Func<T, IDataObjekt, EnumDropType, IDataContext, EnumDropType> canDrop,
            Func<T, IDataObjekt, EnumDropType, IDataContext, Task> onDropped)
            where T : THandler {
            Type handlerType = typeof(T);
            if (!this.registryNative.TryGetValue(dropType, out InheritanceDictionary<NativeHandlerPair> handlerMap)) {
                this.registryNative[dropType] = handlerMap = new InheritanceDictionary<NativeHandlerPair>();
            }
            else if (handlerMap.HasLocalValue(handlerType)) {
                throw new Exception("Handler type already registered: " + handlerType.Name);
            }

            handlerMap[handlerType] = new NativeHandlerPair((h, d, t, c) => canDrop((T) h, d, t, c), (h, d, t, c) => onDropped((T) h, d, t, c));
        }

        // TODO: Could remove the OnDropped/OnDroppedNative and make new methods that accept IDataObjekt
        // however, we would lose specific control over that data dropped (e.g. a List<T> where there can only be 1 item dropped allowed)

        /// <summary>
        /// Called when the user drags the given CLR object over/around the given target, with the given dropType
        /// </summary>
        /// <param name="target">Object in which the value is being dragged over</param>
        /// <param name="value">The value being dragged</param>
        /// <param name="dropType">The drag drop type</param>
        /// <returns>True if the drag can occur (and show the appropriate icon based on the dropType), otherwise false</returns>
        public EnumDropType CanDrop(THandler target, object value, EnumDropType dropType, IDataContext context = null) {
            for (Type valueType = value.GetType(); valueType != null; valueType = valueType.BaseType) {
                if (this.registryCustom.TryGetValue(valueType, out InheritanceDictionary<CustomHandlerPair> handlerMap)) {
                    ITypeEntry<CustomHandlerPair> entry = handlerMap.FindNearestBaseType(target.GetType());
                    while (entry != null) {
                        EnumDropType dt = entry.LocalValue.CanDrop(target, value, dropType, context);
                        if (dt != EnumDropType.None) {
                            return dt;
                        }

                        entry = entry.NearestBaseTypeWithLocalValue;
                    }
                }
            }

            return EnumDropType.None;
        }

        /// <summary>
        /// Called when the user drags an unknown native object over/around the given target, with the given dropType
        /// </summary>
        /// <param name="target">Object in which the value is being dragged over</param>
        /// <param name="value">The data object that is being dragged</param>
        /// <param name="dropType">The drag drop type</param>
        /// <returns>True if the drag can occur (and show the appropriate icon based on the dropType), otherwise false</returns>
        public EnumDropType CanDropNative(THandler target, IDataObjekt value, EnumDropType dropType, IDataContext context = null) {
            foreach (KeyValuePair<string, InheritanceDictionary<NativeHandlerPair>> pair in this.registryNative) {
                if (!value.GetDataPresent(pair.Key)) {
                    continue;
                }

                for (ITypeEntry<NativeHandlerPair> entry = pair.Value.FindNearestBaseType(target.GetType()); entry != null; entry = entry.NearestBaseTypeWithLocalValue) {
                    EnumDropType dt = entry.LocalValue.CanDrop(target, value, dropType, context);
                    if (dt != EnumDropType.None) {
                        return dt;
                    }
                }
            }

            return EnumDropType.None;
        }

        /// <summary>
        /// Called when a CLR object is dropped onto the given target with the given dropType.
        /// This will always check the CanDrop handler first before calling the OnDropped handler
        /// </summary>
        /// <param name="target">The target object that the value was dropped onto</param>
        /// <param name="value">The dropped CLR object</param>
        /// <param name="dropType">The type of drop</param>
        /// <param name="context">
        /// Additional context for the drop event. This is typically for storing flags and values that are
        /// handled by specific drop targets (e.g. the frame, based on the mouse position, when dropping on a track)
        /// </param>
        /// <returns>True if a drop handler was called, otherwise false</returns>
        public async Task<bool> OnDropped(THandler target, object value, EnumDropType dropType, IDataContext context = null) {
            if (context == null) {
                context = EmptyContext.Instance;
            }

            for (Type valueType = value.GetType(); valueType != null; valueType = valueType.BaseType) {
                if (this.registryCustom.TryGetValue(valueType, out InheritanceDictionary<CustomHandlerPair> handlerMap)) {
                    ITypeEntry<CustomHandlerPair> entry = handlerMap.FindNearestBaseType(target.GetType());
                    while (entry != null) {
                        CustomHandlerPair pair = entry.LocalValue;
                        if (pair.CanDrop(target, value, dropType, context) != EnumDropType.None) {
                            await pair.OnDropped(target, value, dropType, context);
                            return true;
                        }

                        entry = entry.NearestBaseTypeWithLocalValue;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Called when an unknown native object is dropped onto the given target object with the
        /// given dropType. This will always check the CanDrop handler first before calling the OnDropped handler
        /// </summary>
        /// <param name="target">The target object that the data object was dropped onto</param>
        /// <param name="value">The dropped data object containing operating system data (or a CLR object(s))</param>
        /// <param name="dropType">The type of drop</param>
        /// <returns>True if a drop handler was called, otherwise false</returns>
        public async Task<bool> OnDroppedNative(THandler target, IDataObjekt value, EnumDropType dropType, IDataContext context = null) {
            foreach (KeyValuePair<string, InheritanceDictionary<NativeHandlerPair>> registryPair in this.registryNative) {
                if (!value.GetDataPresent(registryPair.Key)) {
                    continue;
                }

                ITypeEntry<NativeHandlerPair> entry = registryPair.Value.FindNearestBaseType(target.GetType());
                while (entry != null) {
                    NativeHandlerPair pair = entry.LocalValue;
                    if (pair.CanDrop(target, value, dropType, context) != EnumDropType.None) {
                        await pair.OnDropped(target, value, dropType, context);
                        return true;
                    }

                    entry = entry.NearestBaseTypeWithLocalValue;
                }
            }

            return false;
        }

        private readonly struct CustomHandlerPair {
            internal readonly CustomCanDropDelegate CanDrop;
            internal readonly CustomOnDroppedDelegate OnDropped;

            public CustomHandlerPair(CustomCanDropDelegate a, CustomOnDroppedDelegate b) {
                this.CanDrop = a;
                this.OnDropped = b;
            }
        }

        private readonly struct NativeHandlerPair {
            internal readonly NativeCanDropDelegate CanDrop;
            internal readonly NativeOnDroppedDelegate OnDropped;

            public NativeHandlerPair(NativeCanDropDelegate a, NativeOnDroppedDelegate b) {
                this.CanDrop = a;
                this.OnDropped = b;
            }
        }
    }
}