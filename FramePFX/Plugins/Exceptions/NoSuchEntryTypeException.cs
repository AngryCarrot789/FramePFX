namespace FramePFX.Plugins.Exceptions;

public class NoSuchEntryTypeException : BasePluginLoadException
{
    public NoSuchEntryTypeException(string? typeName, Exception? innerException) : base($"No such type exists in the plugin's assembly: '{typeName}'", innerException)
    {
        
    }
}