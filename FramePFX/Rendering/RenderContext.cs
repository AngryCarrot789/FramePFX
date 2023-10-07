using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Editor;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.VideoClips;
using SkiaSharp;

namespace FramePFX.Rendering {
    public sealed class RenderContext {
        /// <summary>
        /// The target render surface
        /// </summary>
        public SKSurface Surface { get; }

        /// <summary>
        /// The surface's canvas
        /// </summary>
        public SKCanvas Canvas { get; }

        /// <summary>
        /// The image info about the surface
        /// </summary>
        public SKImageInfo FrameInfo { get; }

        /// <summary>
        /// The size of the rendering canvas, e.g. 1920,1080
        /// </summary>
        public Vector2 FrameSize { get; }

        /// <summary>
        /// Gets the rendering quality for the project at the time of the render
        /// </summary>
        public EnumRenderQuality RenderQuality;

        /// <summary>
        /// Gets the <see cref="SKFilterQuality"/> which is based on the <see cref="RenderQuality"/>
        /// </summary>
        public SKFilterQuality RenderFilterQuality;

        private List<(VideoClip, SKRect)> clipBoxes;

        public List<(VideoClip, SKRect)> ClipBoundingBoxes => this.clipBoxes ?? (this.clipBoxes = new List<(VideoClip, SKRect)>());

        /// <summary>
        /// Gets or sets a value that states if the render process should provide clip bounding box information (added to <see cref="ClipBoundingBoxes"/>)
        /// </summary>
        public bool ShouldProvideClipBounds;

        /// <summary>
        /// The timeline render depth. Rendering the main timeline only means that this value only ever reaches a value of 1
        /// </summary>
        public int Depth;

        public RenderContext(SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) {
            this.Surface = surface;
            this.Canvas = canvas;
            this.FrameInfo = frameInfo;
            this.FrameSize = new Vector2(frameInfo.Width, frameInfo.Height);
        }

        /// <summary>
        /// Clears the context's drawing canvas
        /// </summary>
        public void ClearPixels() {
            this.Canvas.Clear(SKColors.Black);
        }

        public void SetRenderQuality(EnumRenderQuality quality) {
            this.RenderQuality = quality;
            this.RenderFilterQuality = quality.ToFilterQuality();
        }
    }

    public class TimelineRenderState {
        public readonly List<VideoClip> RenderList;
        public readonly List<AdjustmentVideoClip> AdjustmentStack;
        public readonly Timeline Timeline;

        public bool IsRendering { get; set; }

        public TimelineRenderState(Timeline timeline) {
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            this.RenderList = new List<VideoClip>();
            this.AdjustmentStack = new List<AdjustmentVideoClip>();
        }

        public void Reset() {
            this.RenderList.Clear();
            this.AdjustmentStack.Clear();
        }
    }
}