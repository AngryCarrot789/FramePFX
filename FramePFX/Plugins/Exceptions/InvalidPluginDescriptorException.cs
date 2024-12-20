namespace FramePFX.Plugins.Exceptions;

public class InvalidPluginDescriptorException : BasePluginLoadException
{
    public InvalidPluginDescriptorException(Exception exception) : base("Exception while reading plugin descriptor", exception)
    {
    }
}