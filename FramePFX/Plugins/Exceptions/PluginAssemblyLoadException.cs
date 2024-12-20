namespace FramePFX.Plugins.Exceptions;

public class PluginAssemblyLoadException : BasePluginLoadException
{
    public PluginAssemblyLoadException(Exception e) : base("Failed to load plugin's assembly file", e)
    {
        
    }
}