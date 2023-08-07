namespace FramePFX.Core.AdvancedContextService
{
    /// <summary>
    /// The base interface for all context entries. Currently, this is only used for menu items and separators
    /// <para>
    /// Instead of view models containing a list of context menu item entries and then just dynamically
    /// updating each one when required (which would be really annoying to do), instances of these entries are
    /// instead created on-demand and their state is setup when created (with optional bindable properties to further
    /// update the state of the entry). And then, a generator can be used to generate the items
    /// </para>
    /// </summary>
    public interface IContextEntry
    {
    }
}