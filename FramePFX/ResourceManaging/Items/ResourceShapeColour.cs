namespace FramePFX.ResourceManaging.Items {
    public class ResourceShapeColour : ResourceItem {
        private float r;
        private float g;
        private float b;
        private float a;

        public float Red {
            get => this.r;
            set {
                this.RaisePropertyChanged(ref this.r, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        public float Green {
            get => this.g;
            set {
                this.RaisePropertyChanged(ref this.g, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        public float Blue {
            get => this.b;
            set {
                this.RaisePropertyChanged(ref this.b, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        public float Alpha {
            get => this.a;
            set {
                this.RaisePropertyChanged(ref this.a, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        public ResourceShapeColour() {
            this.a = 1f;
        }
    }
}