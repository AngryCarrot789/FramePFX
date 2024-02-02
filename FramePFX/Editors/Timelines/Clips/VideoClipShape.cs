using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    /// <summary>
    /// A video clip that draws a basic square, used as a debug video clip mostly
    /// </summary>
    public class VideoClipShape : VideoClip {
        public static readonly ParameterVector2 SizeParameter =
            Parameter.RegisterVector2(
                typeof(VideoClipShape),
                nameof(VideoClipShape),
                nameof(Size),
                new Vector2(100, 30),
                ValueAccessors.LinqExpression<Vector2>(typeof(VideoClipShape), nameof(Size)),
                ParameterFlags.AffectsRender);

        private RenderData renderData;

        public Vector2 Size;

        public IResourcePathKey<ResourceColour> ColourKey { get; }

        public VideoClipShape() {
            this.UsesCustomOpacityCalculation = true;
            this.Size = SizeParameter.Descriptor.DefaultValue;
            this.ColourKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceColour>();
            this.ColourKey.ResourceChanged += (key, item, newItem) => {
                this.InvalidateRender();
            };
        }

        public override Vector2? GetRenderSize() {
            return new Vector2(this.Size.X, this.Size.Y);
        }

        public override bool PrepareRenderFrame(PreRenderContext ctx, long frame) {
            this.renderData = new RenderData() {
                size = this.Size,
                colour = this.ColourKey.TryGetResource(out ResourceColour resource) ? resource.Colour : (this.Track?.Colour ?? SKColors.White)
            };

            return true;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            RenderData d = this.renderData;
            SKColor colour = RenderUtils.BlendAlpha(d.colour, this.InternalRenderOpacity);
            using (SKPaint paint = new SKPaint() {Color = colour, IsAntialias = true}) {
                rc.Canvas.DrawRect(0, 0, d.size.X, d.size.Y, paint);
            }

            renderArea = rc.TranslateRect(new SKRect(0, 0, d.size.X, d.size.Y));
        }

        private struct RenderData {
            public Vector2 size;
            public SKColor colour;
        }
    }
}