namespace FramePFX.Core.Actions {
    public class ActionUpdateEvent : ActionEvent {
        public bool IsVisible { get; set; }

        public bool IsEnabled { get; set; }

        public ActionUpdateEvent(object dataContext) : base(dataContext) {
            this.IsEnabled = true;
            this.IsVisible = true;
        }
    }
}