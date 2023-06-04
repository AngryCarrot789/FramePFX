namespace FramePFX.Core.ResourceManaging.Resources {
    public class ResourceText : ResourceItem {
        public string Text { get; set; }

        public double FontSize { get; set; }

        public string FontFamily { get; set; }

        public ResourceText(ResourceManager manager) : base(manager) {

        }
    }
}