namespace FramePFX.Plugins
{
    /// <summary>
    /// Base class for a PFX plugin
    /// </summary>
    public abstract class Plugin
    {
        public abstract string Name { get; }

        protected Plugin()
        {
        }

        /// <summary>
        /// Called when this an instance of this plugin is created, but before any plugin has been loaded
        /// </summary>
        /// <param name="loader"></param>
        public virtual void OnConstructed(PluginLoader loader)
        {
        }

        /// <summary>
        /// Called when this plugin is constructed and loaded. Typically called when the app launched
        /// </summary>
        public abstract void OnLoad(PluginLoader loader);

        /// <summary>
        /// Called when the plugin is unloaded. Typically called when the app exits
        /// </summary>
        public abstract void OnUnload(PluginLoader loader);
    }
}