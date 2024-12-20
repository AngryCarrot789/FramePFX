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

using System.Reflection;
using System.Runtime.Loader;

namespace FramePFX.Plugins;

public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver myResolver;

    public PluginAssemblyLoadContext(string pluginPath)
    {
        this.myResolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = this.myResolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath == null)
            return null;

        return this.LoadFromAssemblyPath(assemblyPath);

    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = this.myResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath == null)
            return IntPtr.Zero;

        return this.LoadUnmanagedDllFromPath(libraryPath);

    }
}