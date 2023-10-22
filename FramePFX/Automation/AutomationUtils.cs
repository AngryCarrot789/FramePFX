using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.Utils;

namespace FramePFX.Automation {
    public static class AutomationUtils {
        private static readonly Dictionary<AutomationKey, UpdateAutomationValueEventHandler> CachedHandlers;
        private static readonly ConstantExpression ConstFalse;

        static AutomationUtils() {
            CachedHandlers = new Dictionary<AutomationKey, UpdateAutomationValueEventHandler>();
            ConstFalse = Expression.Constant(BoolBox.False);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UpdateAutomationValueEventHandler CreateAssignment(this IAutomatable automatable, AutomationKey key) {
            if (CachedHandlers.TryGetValue(key, out UpdateAutomationValueEventHandler handler)) {
                return handler;
            }

            string mdName;
            switch (key.DataType) {
                case AutomationDataType.Float:
                    mdName = nameof(AutomationSequence.GetFloatValue);
                    break;
                case AutomationDataType.Double:
                    mdName = nameof(AutomationSequence.GetDoubleValue);
                    break;
                case AutomationDataType.Long:
                    mdName = nameof(AutomationSequence.GetLongValue);
                    break;
                case AutomationDataType.Boolean:
                    mdName = nameof(AutomationSequence.GetBooleanValue);
                    break;
                case AutomationDataType.Vector2:
                    mdName = nameof(AutomationSequence.GetVector2Value);
                    break;
                default: throw new ArgumentException("Unsupported key data type", nameof(key));
            }

            ParameterExpression paramSeq = Expression.Parameter(typeof(AutomationSequence), "s");
            ParameterExpression paramFrame = Expression.Parameter(typeof(long), "f");
            MemberExpression automationData = Expression.Property(paramSeq, nameof(AutomationSequence.AutomationData));
            MemberExpression rawDataOwner = Expression.Property(automationData, nameof(AutomationData.Owner));

            MemberInfo targetMember = GetTargetPropertyOrField(automatable.GetType(), key.Id);
            Type lowestOwnerType = targetMember.DeclaringType;
            if (lowestOwnerType == null)
                throw new Exception($"The target member named '{key.Id}' does not have a declaring type");

            Expression target = Expression.MakeMemberAccess(Expression.Convert(rawDataOwner, lowestOwnerType), targetMember);
            MethodCallExpression getValue = Expression.Call(paramSeq, mdName, null, paramFrame, ConstFalse);
            Expression body = Expression.Assign(target, getValue);
            handler = Expression.Lambda<UpdateAutomationValueEventHandler>(body, paramSeq, paramFrame).Compile();
            CachedHandlers[key] = handler;
            return handler;
        }

        private static MemberInfo GetTargetPropertyOrField(Type type, string name) {
            // Can't get public or private in a single method call, according to the Expression class
            PropertyInfo p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (p != null)
                return p;
            FieldInfo f = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (f != null)
                return f;
            if ((p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) != null)
                return p;
            if ((f = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) != null)
                return f;
            throw new Exception($"Could not find a property or field with the name '{name}' in the type hierarchy for '{type.Name}'");
        }
    }
}