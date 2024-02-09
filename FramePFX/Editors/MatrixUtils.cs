using System.Numerics;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Effects;
using SkiaSharp;

namespace FramePFX.Editors {
    public static class MatrixUtils {
        public static SKMatrix CreateTransformationMatrix(Vector2 pos, Vector2 scale, double rotation, Vector2 scaleOrigin, Vector2 rotationOrigin) {
            SKMatrix matrix = SKMatrix.Identity;
            matrix = matrix.PreConcat(SKMatrix.CreateTranslation(pos.X, pos.Y));
            matrix = matrix.PreConcat(SKMatrix.CreateScale(scale.X, scale.Y, scaleOrigin.X, scaleOrigin.Y));
            matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees((float) rotation, rotationOrigin.X, rotationOrigin.Y));
            return matrix;
        }
    }
}