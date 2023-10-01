using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Interactivity
{
    /// <summary>
    /// A class that supports basic type-based drag-drop processing. This class contains a dictionary which
    /// maps a droppable object type to another dictionary which maps the handler/target type to the handler functions
    /// </summary>
    public class DragDropRegistry
    {
        private delegate bool CanDropDelegate(object target, object drop, EnumDropType dropType);

        private delegate Task OnDroppedDelegate(object target, object drop, EnumDropType dropType);

        // [dropType -> [handler -> func]]
        // Func<handler, drop, Task>
        private readonly Dictionary<Type, Dictionary<Type, HandlerPair>> Registry;

        public DragDropRegistry()
        {
            this.Registry = new Dictionary<Type, Dictionary<Type, HandlerPair>>();
        }

        public void Register<THandler, TObject>(Func<THandler, TObject, EnumDropType, bool> canDrop, Func<THandler, TObject, EnumDropType, Task> onDropped) where THandler : class
        {
            Type dropType = typeof(TObject);
            Type handlerType = typeof(THandler);
            if (!this.Registry.TryGetValue(dropType, out Dictionary<Type, HandlerPair> handlerMap))
            {
                this.Registry[dropType] = handlerMap = new Dictionary<Type, HandlerPair>();
            }
            else if (handlerMap.ContainsKey(handlerType))
            {
                throw new Exception("Handler type already registered");
            }

            handlerMap[handlerType] = new HandlerPair(
                (h, d, t) => canDrop((THandler) h, (TObject) d, t),
                (h, d, t) => onDropped((THandler) h, (TObject) d, t));
        }

        public bool CanDrop(object target, object dropped, EnumDropType dropType)
        {
            if (!this.Registry.TryGetValue(dropped.GetType(), out Dictionary<Type, HandlerPair> handlerMap))
            {
                return false;
            }

            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                if (handlerMap.TryGetValue(type, out HandlerPair pair) && pair.CanDrop(target, dropped, dropType))
                {
                    return true;
                }
            }

            return false;
        }

        public Task<bool> OnDropped(object target, object dropped, EnumDropType dropType, bool unsafeSkipCanDropCheck = false)
        {
            if (!this.Registry.TryGetValue(dropped.GetType(), out Dictionary<Type, HandlerPair> handlerMap))
            {
                return Task.FromResult(false);
            }

            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                if (handlerMap.TryGetValue(type, out HandlerPair pair) && (unsafeSkipCanDropCheck || pair.CanDrop(target, dropped, dropType)))
                {
                    return pair.OnDropped(target, dropped, dropType).ContinueWith(t => true);
                }
            }

            return Task.FromResult(false);
        }

        private readonly struct HandlerPair
        {
            public readonly CanDropDelegate CanDrop;
            public readonly OnDroppedDelegate OnDropped;

            public HandlerPair(CanDropDelegate a, OnDroppedDelegate b)
            {
                this.CanDrop = a;
                this.OnDropped = b;
            }
        }
    }
}