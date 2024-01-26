namespace FramePFX.Editors.Controls.Dragger {
    /// <summary>
    /// An interface for pre-processing a <see cref="NumberDragger"/>'s value before it is updated
    /// </summary>
    public interface IValuePreProcessor {
        double Process(double value, double min, double max);
    }
}