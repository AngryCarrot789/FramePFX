namespace FramePFX.ResourceManaging.Items {
    public class ResourceText : ResourceItem {
        private string text;
        public string Text {
            get => this.text;
            set {
                this.RaisePropertyChanged(ref this.text, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        private int fontSize;
        public int FontSize {
            get => this.fontSize;
            set {
                this.RaisePropertyChanged(ref this.fontSize, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        private string fontFamily;
        public string FontFamily {
            get => this.fontFamily;
            set {
                this.RaisePropertyChanged(ref this.fontFamily, value);
                this.RaiseResourceModifiedAuto();
            }
        }

        // TODO: Add extra formatting data here. A handful of these view models probably can't be
        // TODO: "cored" aka put into their own core project... but oh well
    }
}