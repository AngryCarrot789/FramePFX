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

using System;
using Avalonia;

namespace FramePFX.Avalonia.CommandSystem.Usages;

/// <summary>
/// This class helps associated a command usage with a control, so that it may do
/// things like execute a command or update its look based on the command
/// </summary>
public class CommandUsageManager {
    public static readonly AttachedProperty<string?> SimpleButtonCommandIdProperty = AvaloniaProperty.RegisterAttached<CommandUsageManager, AvaloniaObject, string?>("SimpleButtonCommandId");
    public static readonly AttachedProperty<Type?> UsageClassTypeProperty = AvaloniaProperty.RegisterAttached<CommandUsageManager, AvaloniaObject, Type?>("UsageClassType", validate: Validate);
    public static readonly AttachedProperty<CommandUsage?> InternalCommandContextProperty = AvaloniaProperty.RegisterAttached<CommandUsageManager, AvaloniaObject, CommandUsage?>("InternalCommandContext");

    private static bool Validate(Type? value) => value == null || (value is Type type && typeof(CommandUsage).IsAssignableFrom(type));

    static CommandUsageManager() {
        SimpleButtonCommandIdProperty.Changed.AddClassHandler<AvaloniaObject, string?>((d, e) => {
            if (d.GetValue(InternalCommandContextProperty) is CommandUsage oldContext) {
                oldContext.Disconnect();
            }

            if (e.NewValue.GetValueOrDefault() is string cmdId) {
                CommandUsage ctx = new BasicButtonCommandUsage(cmdId);
                d.SetValue(InternalCommandContextProperty, ctx);
                ctx.Connect(d);
            }
            else {
                d.SetValue(InternalCommandContextProperty, null);
            }
        });

        UsageClassTypeProperty.Changed.AddClassHandler<AvaloniaObject, Type?>((d, e) => {
            if (d.GetValue(InternalCommandContextProperty) is CommandUsage oldContext) {
                oldContext.Disconnect();
            }

            if (e.NewValue.GetValueOrDefault() is Type newType) {
                CommandUsage usage = (CommandUsage) Activator.CreateInstance(newType)!;
                d.SetValue(InternalCommandContextProperty, usage);
                usage.Connect(d);
            }
            else {
                d.SetValue(InternalCommandContextProperty, null);
            }
        });
    }

    public static void SetSimpleButtonCommandId(AvaloniaObject element, string value) => element.SetValue(SimpleButtonCommandIdProperty, value);

    public static string? GetSimpleButtonCommandId(AvaloniaObject element) => element.GetValue(SimpleButtonCommandIdProperty);

    /// <summary>
    /// Sets the command usage class type for this element
    /// </summary>
    public static void SetUsageClassType(AvaloniaObject element, Type value) => element.SetValue(UsageClassTypeProperty, value);

    /// <summary>
    /// Gets the command usage class type for this element
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static Type? GetUsageClassType(AvaloniaObject element) => (Type?) element.GetValue(UsageClassTypeProperty);
}