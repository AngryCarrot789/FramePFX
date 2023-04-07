using FramePFX.Core.AdvancedContextService.Base;

namespace FramePFX.Core.AdvancedContextService {
    /// <summary>
    /// A separator element between menu items
    /// </summary>
    public class ContextEntrySeparator : IContextEntry {
        public static ContextEntrySeparator Instance { get; } = new ContextEntrySeparator();
    }
}