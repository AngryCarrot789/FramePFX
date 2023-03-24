namespace FramePFX.Timeline.Layer.Clips.Resizable {
    public abstract class ResizableVideoClipViewModel : VideoClipViewModel {
        protected float posX;
        public float PosX {
            get => this.posX;
            set => this.RaisePropertyChanged(ref this.posX, value);
        }

        protected float posY;
        public float PosY {
            get => this.posY;
            set => this.RaisePropertyChanged(ref this.posY, value);
        }

        protected float width;
        public float Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        protected float height;
        public float Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        protected float rotZ;
        public float RotZ {
            get => this.rotZ;
            set => this.RaisePropertyChanged(ref this.rotZ, value);
        }

        protected ResizableVideoClipViewModel() {

        }

        public void SetShape(float x, float y, float w, float h) {
            this.PosX = x;
            this.PosY = y;
            this.Width = w;
            this.Height = h;
        }
    }
}