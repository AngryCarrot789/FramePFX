using FramePFX.CommandSystem;

namespace FramePFX.Plugins;

/// <summary>
/// The base class for a plugin's entry point class
/// </summary>
public abstract class Plugin
{
    public PluginDescriptor Descriptor { get; internal set; } = null!;
    
    public PluginLoader PluginLoader { get; internal set; } = null!;

    public string PluginFolder { get; internal set; } = null!;
    
    protected Plugin()
    {
        
    }

    /// <summary>
    /// Invoked after the plugin is created and the descriptor is set
    /// </summary>
    public abstract void OnCreated();

    /// <summary>
    /// Adds this plugin's services to the given service manager
    /// </summary>
    /// <param name="manager">Service manager</param>
    public abstract void RegisterServices(ServiceManager manager);
    
    /// <summary>
    /// Register this plugin's commands
    /// </summary>
    /// <param name="manager">Command manager</param>
    public abstract void RegisterCommands(CommandManager manager);

    /// <summary>
    /// Invoked when the application has loaded. This is invoked before an editor window is created
    /// </summary>
    /// <returns></returns>
    public abstract Task OnApplicationLoaded();
}