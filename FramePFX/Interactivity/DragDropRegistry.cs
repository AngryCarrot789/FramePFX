using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Interactivity {
    /// <summary>
    /// A class that supports basic type-based drag-drop processing. This class contains a dictionary which
    /// maps a droppable object type to another dictionary which maps the handler/target type to the handler functions
    /// </summary>
    public class DragDropRegistry {
        private delegate EnumDropType CustomCanDropDelegate(object target, object drop, EnumDropType dropType);
        private delegate Task CustomOnDroppedDelegate(object target, object drop, EnumDropType dropType);

        private delegate EnumDropType NativeCanDropDelegate(object target, IDataObjekt drop, EnumDropType dropType);
        private delegate Task NativeOnDroppedDelegate(object target, IDataObjekt drop, EnumDropType dropType);

        // [dropType -> [handler -> func]]
        private readonly Dictionary<Type, Dictionary<Type, CustomHandlerPair>> registryCustom;
        private readonly Dictionary<string, Dictionary<Type, NativeHandlerPair>> registryNative;

        public DragDropRegistry() {
            this.registryCustom = new Dictionary<Type, Dictionary<Type, CustomHandlerPair>>();
            this.registryNative = new Dictionary<string, Dictionary<Type, NativeHandlerPair>>();
        }

        public void Register<THandler, TObject>(Func<THandler, TObject, EnumDropType, EnumDropType> canDrop, Func<THandler, TObject, EnumDropType, Task> onDropped) where THandler : class {
            Type dropType = typeof(TObject);
            Type handlerType = typeof(THandler);
            if (!this.registryCustom.TryGetValue(dropType, out Dictionary<Type, CustomHandlerPair> handlerMap)) {
                this.registryCustom[dropType] = handlerMap = new Dictionary<Type, CustomHandlerPair>();
            }
            else if (handlerMap.ContainsKey(handlerType)) {
                throw new Exception("Handler type already registered");
            }

            handlerMap[handlerType] = new CustomHandlerPair((h, d, t) => canDrop((THandler) h, (TObject) d, t), (h, d, t) => onDropped((THandler) h, (TObject) d, t));
        }

        public void RegisterNative<THandler>(string dropType, Func<THandler, IDataObjekt, EnumDropType, EnumDropType> canDrop, Func<THandler, IDataObjekt, EnumDropType, Task> onDropped) where THandler : class {
            Type handlerType = typeof(THandler);
            if (!this.registryNative.TryGetValue(dropType, out Dictionary<Type, NativeHandlerPair> handlerMap)) {
                this.registryNative[dropType] = handlerMap = new Dictionary<Type, NativeHandlerPair>();
            }
            else if (handlerMap.ContainsKey(handlerType)) {
                throw new Exception("Handler type already registered");
            }

            handlerMap[handlerType] = new NativeHandlerPair((h, d, t) => canDrop((THandler) h, d, t), (h, d, t) => onDropped((THandler) h, d, t));
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
        public EnumDropType CanDrop(object target, object value, EnumDropType dropType) {
            if (!this.registryCustom.TryGetValue(value.GetType(), out Dictionary<Type, CustomHandlerPair> handlerMap)) {
                return EnumDropType.None;
            }

            for (Type type = target.GetType(); type != null; type = type.BaseType) {
                if (handlerMap.TryGetValue(type, out CustomHandlerPair pair)) {
                    return pair.CanDrop(target, value, dropType);
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
        public EnumDropType CanDropNative(object target, IDataObjekt value, EnumDropType dropType) {
            foreach (KeyValuePair<string, Dictionary<Type, NativeHandlerPair>> entry in this.registryNative) {
                if (!value.GetDataPresent(entry.Key)) {
                    continue;
                }

                Dictionary<Type, NativeHandlerPair> handlerMap = entry.Value;
                for (Type type = target.GetType(); type != null; type = type.BaseType) {
                    if (handlerMap.TryGetValue(type, out NativeHandlerPair pair)) {
                        return pair.CanDrop(target, value, dropType);
                    }
                }
            }

            return EnumDropType.None;
        }

        /// <summary>
        /// Called when a CLR object is dropped onto the given target with the given dropType
        /// </summary>
        /// <param name="target">The target object that the value was dropped onto</param>
        /// <param name="dropped">The dropped CLR object</param>
        /// <param name="dropType">The type of drop</param>
        /// <param name="unsafeSkipCanDropCheck">Unsafely skips the method that checks if a drop can occur and directly calls the drop handler</param>
        /// <returns>True if a drop handler was called, otherwise false</returns>
        public async Task<bool> OnDropped(object target, object dropped, EnumDropType dropType, bool unsafeSkipCanDropCheck = false) {
            if (!this.registryCustom.TryGetValue(dropped.GetType(), out Dictionary<Type, CustomHandlerPair> handlerMap)) {
                return false;
            }

            for (Type type = target.GetType(); type != null; type = type.BaseType) {
                if (handlerMap.TryGetValue(type, out CustomHandlerPair pair) && (unsafeSkipCanDropCheck || (dropType = pair.CanDrop(target, dropped, dropType)) != EnumDropType.None)) {
                    await pair.OnDropped(target, dropped, dropType);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called when an unknown native object is dropped onto the given target object with the given dropType
        /// </summary>
        /// <param name="target">The target object that the data object was dropped onto</param>
        /// <param name="dropped">The dropped data object containing operating system data (or a CLR object(s))</param>
        /// <param name="dropType">The type of drop</param>
        /// <param name="unsafeSkipCanDropCheck">Unsafely skips the method that checks if a drop can occur and directly calls the drop handler</param>
        /// <returns>True if a drop handler was called, otherwise false</returns>
        public async Task<bool> OnDroppedNative(object target, IDataObjekt dropped, EnumDropType dropType, bool unsafeSkipCanDropCheck = false) {
            foreach (KeyValuePair<string, Dictionary<Type, NativeHandlerPair>> entry in this.registryNative) {
                if (!dropped.GetDataPresent(entry.Key)) {
                    continue;
                }

                Dictionary<Type, NativeHandlerPair> handlerMap = entry.Value;
                for (Type type = target.GetType(); type != null; type = type.BaseType) {
                    if (handlerMap.TryGetValue(type, out NativeHandlerPair pair) && (unsafeSkipCanDropCheck || (dropType = pair.CanDrop(target, dropped, dropType)) != EnumDropType.None)) {
                        await pair.OnDropped(target, dropped, dropType);
                        return true;
                    }
                }
            }

            return false;
        }

        private readonly struct CustomHandlerPair {
            public readonly CustomCanDropDelegate CanDrop;
            public readonly CustomOnDroppedDelegate OnDropped;

            public CustomHandlerPair(CustomCanDropDelegate a, CustomOnDroppedDelegate b) {
                this.CanDrop = a;
                this.OnDropped = b;
            }
        }

        private readonly struct NativeHandlerPair {
            public readonly NativeCanDropDelegate CanDrop;
            public readonly NativeOnDroppedDelegate OnDropped;

            public NativeHandlerPair(NativeCanDropDelegate a, NativeOnDroppedDelegate b) {
                this.CanDrop = a;
                this.OnDropped = b;
            }
        }
    }
}