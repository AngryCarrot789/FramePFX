using System.Collections.Generic;
using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering
{
    public sealed class RenderContext
    {
        private readonly Stack<int> frameBuffers;

        /// <summary>
        /// The size of the rendering canvas, e.g. 1920,1080
        /// </summary>
        public Vector2 FrameSize { get; }

        /// <summary>
        /// The ID of a frame buffer that is currently being rendered into. This is 0 by default (OpenGL default framebuffer)
        /// </summary>
        public int ActiveFrameBuffer { get; private set; }

        public int ActiveFrameBufferTexture;

        /// <summary>
        /// The matrix stack for this rendering context
        /// </summary>
        public readonly MatrixStack MatrixStack;

        /// <summary>
        /// Gets the projection matrix for the current state of the rendering context (created in
        /// the constructor). This should not be modified externally
        /// </summary>
        public readonly Matrix4x4 Projection;

        public RenderContext(Vector2 frameSize)
        {
            this.FrameSize = frameSize;
            this.ActiveFrameBuffer = 0;
            this.MatrixStack = new MatrixStack(Matrix4x4.CreateTranslation(this.FrameSize.X / 2f, this.FrameSize.Y / 2f, 0f));
            this.frameBuffers = new Stack<int>();
            this.Projection = Matrix4x4.CreateOrthographicOffCenter(0, frameSize.X, 0, frameSize.Y, 0.01f, 500f);
        }

        public void PushFrameBuffer(int buffer, FramebufferTarget? framebufferTarget)
        {
            this.frameBuffers.Push(this.ActiveFrameBuffer);
            this.ActiveFrameBuffer = buffer;

            if (framebufferTarget.HasValue)
            {
                GL.BindFramebuffer(framebufferTarget.Value, buffer);
            }
        }

        public int PopFrameBuffer(FramebufferTarget? framebufferTarget)
        {
            int buffer = this.frameBuffers.Pop();
            this.ActiveFrameBuffer = buffer;
            if (framebufferTarget.HasValue)
            {
                GL.BindFramebuffer(framebufferTarget.Value, buffer);
            }

            return buffer;
        }

        public static bool TryPopFrameBuffer(RenderContext context)
        {
            if (context.frameBuffers.Count < 1)
                return false;
            context.PopFrameBuffer(null);
            return true;
        }

        public void ClearPixels()
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        }

        /// <summary>
        /// Resets the context to the default state (identity matrix, default framebuffer, etc.)
        /// </summary>
        public void Reset()
        {
            this.MatrixStack.Clear();
            this.frameBuffers.Clear();
            this.ActiveFrameBuffer = 0;
        }
    }

    /*
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

        public GRContext GrContext { get; set; }

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
    }
     */
}