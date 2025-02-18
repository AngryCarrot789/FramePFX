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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// A helper class for conveniently accessing scope template children by name
/// </summary>
public static class TemplateUtils {
    /// <summary>
    /// Gets the template child of the given scope. Throws an exception if no such child exists with that name or the child is an invalid type
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="childName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T GetTemplateChild<T>(this INameScope scope, string childName, bool applyChildTemplate = true) where T : AvaloniaObject {
        object? child = scope?.Find(childName);
        if (child == null)
            throw new Exception($"Missing template part '{childName}'");

        if (!(child is T value))
            throw new Exception($"Incompatible template part type '{childName}'. Expected {typeof(T).FullName}, got {child.GetType().FullName}");

        if (applyChildTemplate && value is TemplatedControl c) {
            c.ApplyStyling();
            c.ApplyTemplate();
        }

        return value;
    }

    /// <summary>
    /// A void version of <see cref="GetTemplateChild{T}(INameScope,string)"/> which sets the return value as the given out variable
    /// </summary>
    /// <param name="scope">The scope which has a templated applied</param>
    /// <param name="childName">The name of the templated child</param>
    /// <param name="value">The output child</param>
    /// <typeparam name="T">The type of templated child</typeparam>
    public static void GetTemplateChild<T>(this INameScope scope, string childName, [NotNull] out T value) where T : AvaloniaObject {
        value = GetTemplateChild<T>(scope, childName);
    }

    /// <summary>
    /// Tries to find a templated child with the given name and of the given generic type
    /// </summary>
    /// <param name="scope">The scope which has a templated applied</param>
    /// <param name="childName">The name of the templated child</param>
    /// <param name="value">The found template child</param>
    /// <typeparam name="T">The type of child</typeparam>
    /// <returns>True if the child was found, or false if not</returns>
    public static bool TryGetTemplateChild<T>(this INameScope scope, string childName, [NotNullWhen(true)] out T? value) where T : AvaloniaObject {
        return (value = scope?.Find(childName) as T) != null;
    }

    public static void Apply(Control control) {
        control.ApplyStyling();
        control.ApplyTemplate();
    }

    public static void ApplyRecursive(Control control) {
        Apply(control);
        foreach (Visual child in control.GetVisualChildren()) {
            if (child is Control)
                ApplyRecursive((Control) child);
        }
    }
}