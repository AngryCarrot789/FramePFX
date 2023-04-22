namespace FramePFX.ResourceManaging.Items {
    public class ResourceRGBA : ResourceItem {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; } = 1f;

        public ResourceRGBA(ResourceManager manager) : base(manager) {

        }
    }
}