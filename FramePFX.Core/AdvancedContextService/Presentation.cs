namespace MCNBTViewer.Core.AdvancedContextService {
    public struct Presentation {
        public bool IsVisible { get; }

        public bool IsDisabled { get; }

        public static Presentation Enabled => new Presentation(true, false);
        public static Presentation Disabled => new Presentation(true, true);
        public static Presentation Hidden => new Presentation(true, false);

        public Presentation(bool isVisible, bool isDisabled) {
            this.IsVisible = isVisible;
            this.IsDisabled = isDisabled;
        }
    }
}