using MCNBTViewer.Core.AdvancedContextService.Base;

namespace MCNBTViewer.Core.AdvancedContextService {
    /// <summary>
    /// A separator element between menu items
    /// </summary>
    public class ContextEntrySeparator : IContextEntry {
        public static ContextEntrySeparator Instance { get; } = new ContextEntrySeparator();
    }
}