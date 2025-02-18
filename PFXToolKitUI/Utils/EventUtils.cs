// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Linq.Expressions;
using System.Reflection;

namespace PFXToolKitUI.Utils;

public static class EventUtils {
    internal static readonly MethodInfo InvokeActionMethod = typeof(Action).GetMethod("Invoke")!;
    internal static readonly Dictionary<Type, ParameterExpression[]> EventToParamListMap = new Dictionary<Type, ParameterExpression[]>();

    public static Delegate CreateDelegateToInvokeActionFromEvent(Type eventType, Action actionToInvoke) {
        // Get or create cached array of the eventType's parameters. Generic parameters cannot be handled currently
        if (!EventToParamListMap.TryGetValue(eventType, out ParameterExpression[]? paramArray)) {
            MethodInfo invokeMethod = eventType.GetMethod("Invoke") ?? throw new Exception(eventType.Name + " is not a delegate type");
            EventToParamListMap[eventType] = paramArray = invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        }

        // This can't really be optimised any further
        // Creates a lambda, with the eventType's delegate method signature, that invokes actionToInvoke
        MethodCallExpression invokeAction = Expression.Call(Expression.Constant(actionToInvoke), InvokeActionMethod);
        return Expression.Lambda(eventType, invokeAction, paramArray).Compile();
    }

    public static void CreateEventInterface<TTarget, TEvent>(EventInfo info, out Action<TTarget, TEvent> addHandler, out Action<TTarget, TEvent> removeHandler) where TEvent : Delegate {
        ParameterExpression paramTarget = Expression.Parameter(typeof(TTarget), "instance");
        ParameterExpression paramHandler = Expression.Parameter(typeof(TEvent), "handler");
        addHandler = CreateAddOrRemove<TTarget, TEvent>(paramTarget, info.AddMethod!, paramHandler);
        removeHandler = CreateAddOrRemove<TTarget, TEvent>(paramTarget, info.RemoveMethod!, paramHandler);
    }

    public static Action<TTarget, TEvent> CreateAddOrRemove<TTarget, TEvent>(MethodInfo addOrRemoveMethod) where TEvent : Delegate {
        ParameterExpression paramTarget = Expression.Parameter(typeof(TTarget), "instance");
        ParameterExpression paramHandler = Expression.Parameter(typeof(TEvent), "handler");
        return CreateAddOrRemove<TTarget, TEvent>(paramTarget, addOrRemoveMethod, paramHandler);
    }

    public static Action<TTarget, TEvent> CreateAddOrRemove<TTarget, TEvent>(ParameterExpression target, MethodInfo targetMethod, ParameterExpression handler) where TEvent : Delegate {
        return Expression.Lambda<Action<TTarget, TEvent>>(Expression.Call(target, targetMethod, handler), target, handler).Compile();
    }
}