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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Expression = System.Linq.Expressions.Expression;

namespace FramePFX.Utils.Visuals {
    /// <summary>
    /// A class that provides a way to 'interface' with the VisualAncestorChanged event.
    /// The event is internal to WPF core, so accessing it must be done reflectively or via this class
    /// </summary>
    public static class VisualAncestorChangedEventInterface {
        private static readonly MethodInfo InvokeCustomHandler = typeof(Action<DependencyObject, DependencyObject>).GetMethod("Invoke");
        private static readonly EventInfo VisualAncestorChangedEventInfo;
        private static readonly Type VisualAncestorChangedEventHandlerType;
        private static readonly ParameterExpression ParamSenderObject;
        private static readonly ParameterExpression ParamVACEventArgs;
        private static readonly MemberExpression AccessAncestorExpression;
        private static readonly MemberExpression AccessOldParentExpression;

        static VisualAncestorChangedEventInterface() {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            VisualAncestorChangedEventInfo = typeof(Visual).GetEvent("VisualAncestorChanged", flags);
            if (ReferenceEquals(VisualAncestorChangedEventInfo, null))
                throw new Exception("Could not find VisualAncestorChanged event in the Visual class");

            VisualAncestorChangedEventHandlerType = VisualAncestorChangedEventInfo.EventHandlerType;
            MethodInfo ancestorHandlerInvokeMd = VisualAncestorChangedEventHandlerType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            Debug.Assert(ancestorHandlerInvokeMd != null, "Expected Invoke method to exist");

            Type eventArgsType = ancestorHandlerInvokeMd.GetParameters()[1].ParameterType;
            FieldInfo ancestorField = eventArgsType.GetField("_subRoot", flags) ?? throw new Exception("Could not find _subRoot field");
            FieldInfo oldParField = eventArgsType.GetField("_oldParent", flags) ?? throw new Exception("Could not find _oldParent field");

            // General access to VisualAncestorChangedEventHandler params and VisualAncestorChangedEventArgs fields
            ParamSenderObject = Expression.Parameter(typeof(object), "sender");
            ParamVACEventArgs = Expression.Parameter(eventArgsType, "args");
            AccessAncestorExpression = Expression.Field(ParamVACEventArgs, ancestorField);
            AccessOldParentExpression = Expression.Field(ParamVACEventArgs, oldParField);
        }

        /// <summary>
        /// Creates two closure actions: AddHandler and RemoveHandler, which respectively add and remove
        /// the given event handler to and from a visual for the VisualAncestorChanged event.
        /// This method should be called in the static constructor of a class
        /// </summary>
        /// <param name="handler">The handler that gets called when VisualAncestorChanged is raised</param>
        /// <param name="addHandler">The action that adds the handler to a visual</param>
        /// <param name="removeHandler">The action that removes the handler from a visual</param>
        public static void CreateInterface(Action<DependencyObject, DependencyObject> handler, out Action<Visual> addHandler, out Action<Visual> removeHandler) {
            MethodCallExpression invoke = Expression.Call(Expression.Constant(handler), InvokeCustomHandler, AccessAncestorExpression, AccessOldParentExpression);
            Delegate theEventHandler = Expression.Lambda(VisualAncestorChangedEventHandlerType, invoke, ParamSenderObject, ParamVACEventArgs).Compile();
            ConstantExpression constEventHandler = Expression.Constant(theEventHandler);

            // Create the add and remove invoker methods for the VisualAncestorChanged event
            ParameterExpression paramVisual = Expression.Parameter(typeof(Visual), "target");

            // Instead of passing a delegate to the action, we instead store the constant handler in the compiled action
            // so that we avoid the cast from Delegate to Visual.VisualAncestorChangedEventHandler
            addHandler = Expression.Lambda<Action<Visual>>(Expression.Call(paramVisual, VisualAncestorChangedEventInfo.AddMethod, constEventHandler), paramVisual).Compile();
            removeHandler = Expression.Lambda<Action<Visual>>(Expression.Call(paramVisual, VisualAncestorChangedEventInfo.RemoveMethod, constEventHandler), paramVisual).Compile();
        }
    }
}