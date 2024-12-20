namespace FramePFX.Plugins.Exceptions;

public class UnknownPluginLoadException : BasePluginLoadException
{
    public UnknownPluginLoadException(Exception cause) : base("Unknown error while loading plugin", cause)
    {
    }
}