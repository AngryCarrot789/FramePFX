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
using System.Windows;
using FramePFX.Editors.Controls.Timelines.CommandContexts;

namespace FramePFX.CommandSystem.Usages {
    /// <summary>
    /// This class helps associated a command usage with a control, so that it may do
    /// things like execute a command or update its look based on the command
    /// </summary>
    public static class CommandUsageManager {
        public static readonly DependencyProperty UsageClassTypeProperty = DependencyProperty.RegisterAttached("UsageClassType", typeof(Type), typeof(CommandUsageManager), new PropertyMetadata(null, OnUsageClassTypeChanged), ValidateUsageType);
        public static readonly DependencyProperty BasicButtonCommandIdProperty = DependencyProperty.RegisterAttached("BasicButtonCommandId", typeof(string), typeof(CommandUsageManager), new PropertyMetadata(null, OnBasicButtonCommandIdChanged));

        private static void OnBasicButtonCommandIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d.GetValue(InternalCommandContextProperty) is CommandUsage oldContext) {
                oldContext.Disconnect();
            }

            if (e.NewValue is string cmdId) {
                CommandUsage ctx = new BasicButtonCommandUsage(cmdId);
                d.SetValue(InternalCommandContextProperty, ctx);
                ctx.Connect(d);
            }
            else {
                d.ClearValue(InternalCommandContextProperty);
            }
        }

        public static void SetBasicButtonCommandId(DependencyObject element, string value) {
            element.SetValue(BasicButtonCommandIdProperty, value);
        }

        public static string GetBasicButtonCommandId(DependencyObject element) {
            return (string) element.GetValue(BasicButtonCommandIdProperty);
        }

        private static readonly DependencyProperty InternalCommandContextProperty =
            DependencyProperty.RegisterAttached(
                "InternalCommandContext",
                typeof(CommandUsage),
                typeof(CommandUsageManager),
                new PropertyMetadata(null));

        public static void SetUsageClassType(DependencyObject element, Type value) => element.SetValue(UsageClassTypeProperty, value);

        public static Type GetUsageClassType(DependencyObject element) => (Type) element.GetValue(UsageClassTypeProperty);

        private static void OnUsageClassTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d.GetValue(InternalCommandContextProperty) is CommandUsage oldContext) {
                oldContext.Disconnect();
            }

            if (e.NewValue is Type newType) {
                CommandUsage ctx = (CommandUsage) Activator.CreateInstance(newType);
                d.SetValue(InternalCommandContextProperty, ctx);
                ctx.Connect(d);
            }
            else {
                d.ClearValue(InternalCommandContextProperty);
            }
        }

        private static bool ValidateUsageType(object value) {
            return (value == null || value == DependencyProperty.UnsetValue) || (value is Type type && typeof(CommandUsage).IsAssignableFrom(type));
        }
    }
}