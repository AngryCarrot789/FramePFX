using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Effects;
using SkiaSharp;

namespace FramePFX.Editors {
    public static class MatrixUtils {
        public static SKMatrix ConcatEffectMatrices(IHaveEffects effectOwner, SKMatrix srcMatrix) {
            foreach (BaseEffect effect in effectOwner.Effects) {
                if (effect is ITransformationEffect tfx) {
                    srcMatrix = srcMatrix.PreConcat(tfx.TransformationMatrix);
                }
            }

            return srcMatrix;
        }
    }
}