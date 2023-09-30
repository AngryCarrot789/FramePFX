using System;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering.ObjectTK {
    public class Framebuffer : OGLObject {
        public Framebuffer() : base(GL.GenFramebuffer()) {
        }

        protected override void Dispose(bool disposing) {
            GL.DeleteFramebuffer(this.Handle);
        }

        /// <summary>
        /// Binds this framebuffer.
        /// </summary>
        /// <param name="target">The framebuffer target to bind to.</param>
        public void Bind(FramebufferTarget target) {
            GL.BindFramebuffer(target, this.Handle);
        }

        /// <summary>
        /// Unbind this framebuffer, i.e. bind the default framebuffer.
        /// </summary>
        /// <param name="target">The framebuffer target to bind to.</param>
        public static void Unbind(FramebufferTarget target) {
            GL.BindFramebuffer(target, 0);
        }

        /// <summary>
        /// Attaches the given texture level to the an attachment point.
        /// </summary>
        /// <remarks>
        /// If texture is a three-dimensional, cube map array, cube map, one- or two-dimensional array, or two-dimensional multisample array texture
        /// the specified texture level is an array of images and the framebuffer attachment is considered to be layered.
        /// </remarks>
        /// <param name="target">The framebuffer target to bind to.</param>
        /// <param name="attachment">The attachment point to attach to.</param>
        /// <param name="texture">The texture to attach.</param>
        /// <param name="level">The level of the texture to attach.</param>
        public void Attach(FramebufferTarget target, FramebufferAttachment attachment, int texture, int level) {
            this.AssertActive(target);
            GL.FramebufferTexture(target, attachment, texture, level);
        }

        /// <summary>
        /// Attaches a single layer of the given texture level to an attachment point.
        /// </summary>
        /// <remarks>
        /// Note that for cube maps and cube map arrays the <paramref name="layer"/> parameter actually indexes the layer-faces.<br/>
        /// Thus for cube maps the layer parameter equals the face to be bound.<br/>
        /// For cube map arrays the layer parameter can be calculated as 6 * arrayLayer + face, which is done automatically when using
        /// the corresponding overload <see cref="Attach(FramebufferTarget, FramebufferAttachment, TextureCubemapArray, int, int, int)"/>.
        /// </remarks>
        /// <param name="target">The framebuffer target to bind to.</param>
        /// <param name="attachment">The attachment point to attach to.</param>
        /// <param name="layeredTexture">The texture to attach.</param>
        /// <param name="layer">The layer of the texture to attach.</param>
        /// <param name="level">The level of the texture to attach.</param>
        public void Attach(FramebufferTarget target, FramebufferAttachment attachment, int layeredTexture, int layer, int level) {
            this.AssertActive(target);
            GL.FramebufferTextureLayer(target, attachment, layeredTexture, level, layer);
        }

        /// <summary>
        /// Attaches the render buffer to the given attachment point.
        /// </summary>
        /// <param name="target">The framebuffer target to bind to.</param>
        /// <param name="attachment">The attachment point to attach to.</param>
        /// <param name="renderbuffer">Render buffer to attach.</param>
        public void Attach(FramebufferTarget target, FramebufferAttachment attachment, int renderbuffer) {
            this.AssertActive(target);
            GL.FramebufferRenderbuffer(target, attachment, RenderbufferTarget.Renderbuffer, renderbuffer);
        }

        /// <summary>
        /// Detaches the currently attached texture from the given attachment point.
        /// </summary>
        /// <param name="attachment">The attachment point to detach from.</param>
        /// <param name="target">The framebuffer target to bind to.</param>
        public void DetachTexture(FramebufferTarget target, FramebufferAttachment attachment) {
            this.AssertActive(target);
            GL.FramebufferTexture(target, attachment, 0, 0);
        }

        /// <summary>
        /// Detaches the currently attached render buffer from the given attachment point.
        /// </summary>
        /// <param name="target">The framebuffer target to bind to.</param>
        /// <param name="attachment">The attachment point to detach from.</param>
        public void DetachRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment) {
            this.AssertActive(target);
            GL.FramebufferRenderbuffer(target, attachment, RenderbufferTarget.Renderbuffer, 0);
        }

        /// <summary>
        /// Throws an <see cref="ObjectNotBoundException"/> if this framebuffer is not the currently active one.
        /// </summary>
        /// <param name="target">The framebuffer target to bind to.</param>
        public void AssertActive(FramebufferTarget target) {
#if DEBUG
            GetPName binding;
            switch (target) {
                case FramebufferTarget.ReadFramebuffer:
                    binding = GetPName.ReadFramebufferBinding;
                    break;
                case FramebufferTarget.DrawFramebuffer:
                    binding = GetPName.DrawFramebufferBinding;
                    break;
                case FramebufferTarget.Framebuffer:
                    binding = GetPName.FramebufferBinding;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            GL.GetInteger(binding, out int activeHandle);
            if (activeHandle != this.Handle) {
                throw new Exception("Can not access an unbound framebuffer. Call Framebuffer.Bind() first.");
            }
#endif
        }
    }
}