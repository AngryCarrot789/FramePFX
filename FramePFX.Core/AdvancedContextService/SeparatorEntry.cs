namespace FrameControlEx.Core.AdvancedContextService {
    /// <summary>
    /// A separator element between menu items
    /// </summary>
    public class SeparatorEntry : IContextEntry {
        public static readonly SeparatorEntry Instance = new SeparatorEntry();
    }
}