using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            MemberExpression automationData = Expression.Property(paramSeq, "AutomationData");
            UnaryExpression dataOwner = Expression.Convert(Expression.Property(automationData, "Owner"), automatable.GetType());
            Expression target = Expression.PropertyOrField(dataOwner, key.Id);

            MethodCallExpression getValue = Expression.Call(paramSeq, mdName, null, paramFrame, ConstFalse);
            Expression body = Expression.Assign(target, getValue);
            handler = Expression.Lambda<UpdateAutomationValueEventHandler>(body, paramSeq, paramFrame).Compile();
            CachedHandlers[key] = handler;
            return handler;
        }
    }
}