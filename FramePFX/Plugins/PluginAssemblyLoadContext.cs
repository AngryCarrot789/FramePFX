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