namespace FramePFX.Plugins;

public class PluginDescriptor
{
    /// <summary>
    /// Gets or sets the DLL path that contains the plugin class. This can be relative to the plugin folder,
    /// so it can just be the actual DLL's name, with .dll on the end
    /// </summary>
    public string? EntryPointLibraryPath { get; set; }

    /// <summary>
    /// Gets or sets the full type name of the class that extends <see cref="Plugin"/> 
    /// </summary>
    public string? EntryPoint { get; set; }

    public List<string>? XamlResources { get; set; }

    public PluginDescriptor()
    {
        
    }
}