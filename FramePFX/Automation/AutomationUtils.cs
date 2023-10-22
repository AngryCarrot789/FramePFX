using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.Utils;

namespace FramePFX.Automation {
    public static class AutomationUtils {
        // private static readonly Dictionary<AutomationKey, UpdateAutomationValueEventHandler> CachedHandlers;
        private static readonly ConstantExpression ConstFalse;

        static AutomationUtils() {
            // CachedHandlers = new Dictionary<AutomationKey, UpdateAutomationValueEventHandler>();
            ConstFalse = Expression.Constant(BoolBox.False);
        }

        public static UpdateAutomationValueEventHandler CreateAssignment(this IAutomatable automatable, AutomationKey key) {
            return CreateOrCreateAssignmentInternal(automatable.GetType(), key.Id, key);
        }

        public static UpdateAutomationValueEventHandler CreateAssignment(this IAutomatable automatable, string memberName, AutomationKey key) {
            return CreateOrCreateAssignmentInternal(automatable.GetType(), memberName, key);
        }

        public static UpdateAutomationValueEventHandler CreateAssignment(Type targetType, AutomationKey key) {
            return CreateOrCreateAssignmentInternal(targetType, key.Id, key);
        }

        public static UpdateAutomationValueEventHandler CreateAssignment(Type targetType, string memberName, AutomationKey key) {
            return CreateOrCreateAssignmentInternal(targetType, memberName, key);
        }

        public static UpdateAutomationValueEventHandler CreateOrCreateAssignmentInternal(Type targetType, string memberName, AutomationKey key) {
            UpdateAutomationValueEventHandler handler = key.__cachedUpdateHandler;
            if (handler == null)
                key.__cachedUpdateHandler = handler = CreateAssignmentInternal(targetType, key.DataType, memberName);
            return handler;
        }

        private static UpdateAutomationValueEventHandler CreateAssignmentInternal(Type targetType, AutomationDataType dataType, string memberName) {
            string mdName;
            switch (dataType) {
                case AutomationDataType.Float:   mdName = nameof(AutomationSequence.GetFloatValue); break;
                case AutomationDataType.Double:  mdName = nameof(AutomationSequence.GetDoubleValue); break;
                case AutomationDataType.Long:    mdName = nameof(AutomationSequence.GetLongValue); break;
                case AutomationDataType.Boolean: mdName = nameof(AutomationSequence.GetBooleanValue); break;
                case AutomationDataType.Vector2: mdName = nameof(AutomationSequence.GetVector2Value); break;
                default: throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "Invalid data type");
            }

            ParameterExpression paramSeq = Expression.Parameter(typeof(AutomationSequence), "s");
            ParameterExpression paramFrame = Expression.Parameter(typeof(long), "f");
            MemberExpression automationData = Expression.Property(paramSeq, nameof(AutomationSequence.AutomationData));
            MemberExpression rawDataOwner = Expression.Property(automationData, nameof(AutomationData.Owner));

            // need to search for the member ourself, so that we can access the lowest applicable
            // type (aka declaring type) to cast to, as targetType may be a derived type of the declaring type
            MemberInfo targetMember = GetTargetPropertyOrField(targetType, memberName);
            Type lowestOwnerType = targetMember.DeclaringType;
            if (lowestOwnerType == null)
                throw new Exception($"The target member named '{memberName}' does not have a declaring type");

            Expression target = Expression.MakeMemberAccess(Expression.Convert(rawDataOwner, lowestOwnerType), targetMember);
            MethodCallExpression getValue = Expression.Call(paramSeq, mdName, null, paramFrame, ConstFalse);
            Expression body = Expression.Assign(target, getValue);
            return Expression.Lambda<UpdateAutomationValueEventHandler>(body, paramSeq, paramFrame).Compile();
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