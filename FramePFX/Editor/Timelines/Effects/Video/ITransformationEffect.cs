using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video {
    public delegate void MatrixChangedEventHandler(ITransformationEffect effect);

    /// <summary>
    /// An interface for an effect that provides a transformation matrix, used to calculate a final matrix for a clip during the render phase
    /// </summary>
    public interface ITransformationEffect {
        /// <summary>
        /// Gets the transformation matrix
        /// </summary>
        SKMatrix TransformationMatrix { get; }

        /// <summary>
        /// An event fired when our <see cref="TransformationMatrix"/> is modified in some way
        /// </summary>
        event MatrixChangedEventHandler MatrixChanged;
    }
}