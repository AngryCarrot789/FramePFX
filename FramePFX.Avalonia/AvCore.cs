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
using System.Reflection;
using Avalonia;
using Avalonia.Rendering;
using Avalonia.VisualTree;

namespace FramePFX.Avalonia;

/// <summary>
/// A class that exposes avalonia internals that should be public
/// </summary>
public static class AvCore {
    private static AvaloniaLocator Locator;
    private static MethodInfo GetServiceMethod;

    public static void OnApplicationInitialised() {
        Locator = (AvaloniaLocator) GetProperty<AvaloniaLocator, IAvaloniaDependencyResolver>(null, "Current", true)!;
        GetServiceMethod = typeof(AvaloniaLocator).GetMethod("GetService", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(Type)], null) ?? throw new Exception("Could not find GetService method");

        // Test that the above code works
        GetService(typeof(object));
    }

    public static void OnFrameworkInitialised() {
    }

    /// <summary>
    /// Gets an Avalonia application service
    /// </summary>
    /// <param name="type">The service type</param>
    /// <returns>The service, or null, if no service was found</returns>
    public static object? GetService(Type type) => GetServiceMethod.Invoke(Locator, [type]);

    public static T? GetService<T>() => (T?) GetServiceMethod.Invoke(Locator, [typeof(T)]);

    /// <summary>
    /// Tries to get a service of the generic type
    /// </summary>
    /// <param name="value">The found service</param>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>Whether or not the service was found</returns>
    public static bool TryGetService<T>(out T value) where T : class => (value = (GetService(typeof(T)) as T)!) != null;

    private static TValue? GetProperty<TOwner, TValue>(object? instance, string name, bool isStatic, bool allowNull = false) {
        Type owner = typeof(TOwner);
        BindingFlags initialFlags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
        PropertyInfo? property;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.Public)) != null)
            goto found;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.NonPublic)) != null)
            goto found;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.Public | BindingFlags.FlattenHierarchy)) != null)
            goto found;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) == null)
            throw new Exception("No such property: " + owner.Name + "." + name);

        found:
        object? theValue = property.GetValue(instance);

        if (allowNull && theValue == null)
            return default;

        if (theValue is TValue value)
            return value;

        throw new Exception("Property value is incompatible with type");
    }

    public static Point GetRelativePosition(Visual relativeTo, Point point) {
        IRenderRoot? root = relativeTo.GetVisualRoot();
        if (root == null) {
            return point;
        }

        PixelPoint screen = root.PointToScreen(point);
        return relativeTo.PointToClient(screen);
    }
}