namespace FramePFX.PropertyEditing.Automation {
    /// <summary>
    /// Contains information about how a number dragger should step value changes
    /// </summary>
    public readonly struct DragStepProfile {
        public static readonly DragStepProfile UnitOne = new DragStepProfile(0.0001, 0.005, 0.001, 0.01);
        public static readonly DragStepProfile Percentage = new DragStepProfile(0.001, 0.01, 0.1, 1.0);
        public static readonly DragStepProfile Rotation = new DragStepProfile(0.005, 0.05, 0.5, 2);
        public static readonly DragStepProfile InfPixelRange = new DragStepProfile(0.05, 0.1, 1.0, 5);

        /// <summary>1
        /// A tiny step change, when holding CTRL+SHIFT
        /// </summary>
        public readonly double TinyStep;

        /// <summary>
        /// A smaller step change, when holding SHIFT
        /// </summary>
        public readonly double SmallStep;

        /// <summary>
        /// A normal step change, when holding no modifier keys
        /// </summary>
        public readonly double NormalStep;

        /// <summary>
        /// A larger step change, when holding CTRL
        /// </summary>
        public readonly double LargeStep;

        public DragStepProfile(double tinyStep, double smallStep, double normalStep, double largeStep) {
            this.TinyStep = tinyStep;
            this.SmallStep = smallStep;
            this.NormalStep = normalStep;
            this.LargeStep = largeStep;
        }
    }
}