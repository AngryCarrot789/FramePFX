namespace FramePFX.ResourceManaging.Items {
    public class ResourceColourViewModel : ResourceItemViewModel {
        private float r;
        private float g;
        private float b;
        private float a;

        public float Red {
            get => this.r;
            set => this.RaisePropertyChanged(ref this.r, value);
        }

        public float Green {
            get => this.g;
            set => this.RaisePropertyChanged(ref this.g, value);
        }

        public float Blue {
            get => this.b;
            set => this.RaisePropertyChanged(ref this.b, value);
        }

        public float Alpha {
            get => this.a;
            set => this.RaisePropertyChanged(ref this.a, value);
        }

        public ResourceColourViewModel() {
            this.a = 1f;
        }
    }
}