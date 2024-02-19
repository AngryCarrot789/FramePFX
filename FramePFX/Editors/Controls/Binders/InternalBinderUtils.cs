//
// Copyright (c) 2023-2024 REghZy
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FramePFX.Editors.Controls.Binders {
    internal static class InternalBinderUtils {
        internal static readonly MethodInfo InvokeActionMethod = typeof(Action).GetMethod("Invoke");
        internal static readonly Dictionary<Type, CachedEventTypeInfo> CachedEventTypeMap = new Dictionary<Type, CachedEventTypeInfo>();

        internal class CachedEventTypeInfo {
            public ParameterExpression[] paramExpressions;
        }

        public static Delegate CreateDelegateToInvokeActionFromEvent(Type eventType, Action actionToInvoke) {
            // Get or create cached array of the eventType's parameters. Generic parameters cannot be handled currently
            if (!CachedEventTypeMap.TryGetValue(eventType, out CachedEventTypeInfo info)) {
                CachedEventTypeMap[eventType] = info = new CachedEventTypeInfo();
                MethodInfo invokeMethod = eventType.GetMethod("Invoke") ?? throw new Exception(eventType.Name + " is not a delegate type");
                info.paramExpressions = invokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            }

            // This can't really be optimised any further
            // Creates a lambda, with the eventType's delegate method signature, that invokes actionToInvoke
            MethodCallExpression invokeAction = Expression.Call(Expression.Constant(actionToInvoke), InvokeActionMethod);
            return Expression.Lambda(eventType, invokeAction, info.paramExpressions).Compile();
        }
    }
}