using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips.Core {
    public class TextClip : VideoClip {
        public static readonly ParameterDouble FontSizeParameter = Parameter.RegisterDouble(typeof(TextClip), nameof(TextClip), nameof(FontSize), 40, ValueAccessors.LinqExpression<double>(typeof(TextClip), nameof(FontSize)), ParameterFlags.StandardProjectVisual);

        public double FontSize;

        public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
            throw new System.NotImplementedException();
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            throw new System.NotImplementedException();
        }
    }
}