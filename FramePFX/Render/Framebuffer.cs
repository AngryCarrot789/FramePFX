using System;
using OpenTK.Graphics.OpenGL;

namespace FrameControl.IO.Rendering {
    public class Framebuffer : IDisposable {
        private bool isDisposed;

        public readonly int width;
        public readonly int height;

        /// <summary>
        /// Framebuffer ID
        /// </summary>
        public readonly int fbId;

        /// <summary>
        /// Texture ID
        /// </summary>
        public readonly int txId;

        public bool IsDisposed => this.isDisposed;

        public Framebuffer(int w, int h, int fb_id, int txt_id) {
            this.width = w;
            this.height = h;
            this.fbId = fb_id;
            this.txId = txt_id;
        }

        public static Framebuffer Create(int width, int height) {
            int framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureId, 0);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete) {
                throw new Exception($"Failed to create frame buffer ({width} x {height}): " + status);
            }

            return new Framebuffer(width, height, framebufferId, textureId);
        }

        public void Use() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.fbId);
        }

        public void Unuse() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose() {
            this.Unuse();
            GL.DeleteTexture(this.txId);
            GL.DeleteFramebuffer(this.fbId);
            this.isDisposed = true;
        }
    }
}
