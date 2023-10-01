//
// Texture2D.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using System;
using FramePFX.Rendering.Utils;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering.ObjectTK
{
    /// <summary>
    /// A framebuffer with a BGRA texture associated with it (internal format of RGB32F)
    /// </summary>
    public sealed class FrameBufferImage : IDisposable
    {
        public int Width { get; }

        public int Height { get; }

        public int FrameBufferId { get; }

        public int TextureId { get; }

        private readonly Shader shader;
        private readonly int vao;
        private readonly int vbo;

        public FrameBufferImage(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            this.FrameBufferId = GL.GenFramebuffer();
            this.TextureId = GL.GenTexture();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.FrameBufferId);
            GL.BindTexture(TextureTarget.Texture2D, this.TextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, this.TextureId, 0);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer could not be completed");
            }

            float[] verts = new[]
            {
                // verts    // uvs
                 1f,  1f,   1f, 1f,
                -1f,  1f,   0f, 1f,
                -1f, -1f,   0f, 0f,
                 1f,  1f,   1f, 1f,
                 1f, -1f,   1f, 0f,
                -1f, -1f,   0f, 0f,
            };

            this.vao = GL.GenVertexArray();
            GL.BindVertexArray(this.vao);

            const int stride = 4 * sizeof(float);
            this.vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // Unbind the VAO after configuration.
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            this.shader = new Shader(@"
#version 150

//Globals
in vec4 in_vec;

//Outputs
out vec2 ex_uv;

void main(void) {
    gl_Position = vec4(in_vec.xy, 0.0, 1.0);
    ex_uv = in_vec.zw;
}
", /* fragment */ @"
#version 330 core
in vec2 ex_uv;
uniform sampler2D tex;

void main()
{
    gl_FragColor = texture(tex, ex_uv);
}
");
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(this.FrameBufferId);
            GL.DeleteTexture(this.TextureId);
            GL.DeleteVertexArray(this.vao);
            GL.DeleteBuffer(this.vbo);
        }

        /// <summary>
        /// Draws our texture's contents into the given target frame buffer
        /// </summary>
        /// <param name="buffer"></param>
        public void DrawIntoTargetBuffer(int buffer)
        {
            this.shader.Use();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.TextureId);

            GL.BindVertexArray(this.vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Clear(float alpha = 0f)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.FrameBufferId);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.TextureId);
            GL.ClearColor(0f, 0f, 0f, alpha);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}