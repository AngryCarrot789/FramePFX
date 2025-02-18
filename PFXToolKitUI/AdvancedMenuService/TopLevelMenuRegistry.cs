using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.AdvancedMenuService;

public class TopLevelMenuRegistry {
    /// <summary>
    /// Gets all of the top-level menu items
    /// </summary>
    public ObservableList<ContextEntryGroup> Items { get; }

    public TopLevelMenuRegistry() {
        this.Items = new ObservableList<ContextEntryGroup>();
    }
}