namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// A separator element between menu items
    /// </summary>
    public class SeparatorEntry : IContextEntry {
        public static SeparatorEntry NewInstance => new SeparatorEntry();
    }
}