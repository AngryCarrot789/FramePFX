namespace FramePFX.Plugins.Exceptions;

public class BasePluginLoadException : Exception
{
    public BasePluginLoadException() { }
    public BasePluginLoadException(string? message) : base(message) { }
    public BasePluginLoadException(string? message, Exception? innerException) : base(message, innerException) { }
}