using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video {
    /// <summary>
    /// An interface for an effect that provides a transformation matrix, used to calculate a final matrix for a clip during the render phase
    /// </summary>
    public interface ITransformationEffect {
        /// <summary>
        /// Gets the transformation matrix. This may be cached and only updated when the state of the object changed
        /// </summary>
        SKMatrix TransformationMatrix { get; }
    }
}