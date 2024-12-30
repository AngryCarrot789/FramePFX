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

using Avalonia;

namespace FramePFX.BaseFrontEnd.CommandUsages;

/// <summary>
/// This class helps associated a command usage with a control, so that it may do
/// things like execute a command or update its look based on the command
/// </summary>
public class CommandUsageManager {
    public static readonly AttachedProperty<string?> SimpleButtonCommandIdProperty = AvaloniaProperty.RegisterAttached<CommandUsageManager, AvaloniaObject, string?>("SimpleButtonCommandId");
    public static readonly AttachedProperty<Type?> UsageClassTypeProperty = AvaloniaProperty.RegisterAttached<CommandUsageManager, AvaloniaObject, Type?>("UsageClassType", validate: Validate);
    public static readonly AttachedProperty<CommandUsage?> CommandUsageProperty = AvaloniaProperty.RegisterAttached<CommandUsageManager, AvaloniaObject, CommandUsage?>("CommandUsage");

    private static bool Validate(Type? value) => value == null || (value is Type type && typeof(CommandUsage).IsAssignableFrom(type));

    static CommandUsageManager() {
        CommandUsageProperty.Changed.AddClassHandler<AvaloniaObject, CommandUsage?>((d, e) => OnCommandUsageChanged(d, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        SimpleButtonCommandIdProperty.Changed.AddClassHandler<AvaloniaObject, string?>((d, e) => {
            if (e.NewValue.GetValueOrDefault() is string cmdId) {
                d.SetValue(CommandUsageProperty, new SimpleButtonCommandUsage(cmdId));
            }
            else {
                d.SetValue(CommandUsageProperty, null);
            }
        });

        UsageClassTypeProperty.Changed.AddClassHandler<AvaloniaObject, Type?>((d, e) => {
            if (e.NewValue.GetValueOrDefault() is Type newType) {
                if (!newType.IsAssignableTo(typeof(CommandUsage)))
                    throw new InvalidOperationException("UsageClass type does not represent a CommandUsage");

                CommandUsage usage = (CommandUsage) Activator.CreateInstance(newType)!;
                d.SetValue(CommandUsageProperty, usage);
            }
            else {
                d.SetValue(CommandUsageProperty, null);
            }
        });
    }
    
    private static void OnCommandUsageChanged(AvaloniaObject target, CommandUsage? oldValue, CommandUsage? newValue) {
        oldValue?.Disconnect();
        newValue?.Connect(target);
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
    
    public static CommandUsage? GetCommandUsage(AvaloniaObject element) => element.GetValue(CommandUsageProperty);
    
    public static void SetCommandUsage(AvaloniaObject element, CommandUsage? value) => element.SetValue(CommandUsageProperty, value);
}