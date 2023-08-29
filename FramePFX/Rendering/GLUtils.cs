using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering {
    public static class GLUtils {
        // Prepare
        // IoC.SkiaOGLContext.ResetContext(GRGlBackendState.All);
        // then...

        public static void CleanGL() {
            // Reset (using OpenTK's GL bindings)
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.VertexProgramPointSize);
            GL.BindVertexArray(0);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.Enable(EnableCap.FramebufferSrgb);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.UseProgram(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.Enable(EnableCap.Dither);
            GL.DepthMask(true);
            GL.Enable(EnableCap.Multisample);
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}