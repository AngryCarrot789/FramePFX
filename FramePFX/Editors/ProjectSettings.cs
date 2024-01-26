namespace FramePFX.Editors {
    public class ProjectSettings {
        public static ProjectSettings Default => new ProjectSettings(1280, 720, 30);

        public int Width { get; set; }

        public int Height { get; set; }

        public double FrameRate { get; set; }

        public ProjectSettings() {
        }

        public ProjectSettings(int width, int height, double frameRate) {
            this.Width = width;
            this.Height = height;
            this.FrameRate = frameRate;
        }
    }
}