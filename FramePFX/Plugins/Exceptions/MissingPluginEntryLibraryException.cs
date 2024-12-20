namespace FramePFX.Plugins.Exceptions;

public class MissingPluginEntryLibraryException : BasePluginLoadException
{
    public MissingPluginEntryLibraryException() : base("Missing the plugin's main library file path")
    {
        
    }
}

public class MissingPluginEntryClassException : BasePluginLoadException
{
    public MissingPluginEntryClassException() : base("Missing the plugin's main type name")
    {
        
    }
}