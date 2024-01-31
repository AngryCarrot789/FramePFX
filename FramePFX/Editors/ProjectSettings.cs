namespace FramePFX.Editors {
    public delegate void ProjectSettingsEventHandler(ProjectSettings settings);

    public class ProjectSettings {
        public static ProjectSettings Default => new ProjectSettings(1920, 1080, 60);

        private int width;
        private int height;
        private double frameRate;

        public int Width {
            get => this.width;
            set {
                if (this.width == value)
                    return;
                this.width = value;
                this.WidthChanged?.Invoke(this);
            }
        }

        public int Height {
            get => this.height;
            set {
                if (this.height == value)
                    return;
                this.height = value;
                this.HeightChanged?.Invoke(this);
            }
        }

        public double FrameRate {
            get => this.frameRate;
            set {
                if (this.frameRate == value)
                    return;
                this.frameRate = value;
                this.FrameRateChanged?.Invoke(this);
            }
        }

        public event ProjectSettingsEventHandler WidthChanged;
        public event ProjectSettingsEventHandler HeightChanged;
        public event ProjectSettingsEventHandler FrameRateChanged;

        public ProjectSettings() {
        }

        public ProjectSettings(int width, int height, double frameRate) {
            this.Width = width;
            this.Height = height;
            this.FrameRate = frameRate;
        }
    }
}