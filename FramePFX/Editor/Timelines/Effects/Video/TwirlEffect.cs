using System;
using System.Numerics;
using FramePFX.Rendering;
using FramePFX.Rendering.Utils;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video
{
    public class TwirlEffect : VideoEffect
    {
        private Shader shader;
        private int vao;
        private int vbo;

        public TwirlEffect()
        {
        }

        protected override void OnAddedToClip()
        {
            base.OnAddedToClip();
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

            this.vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Unbind the VAO after configuration.
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            this.shader = new Shader(@"
// uniforms
uniform mat4 mvp;

// Globals
in vec4 in_vec;

// Outputs
out vec2 ex_uv;

void main()
{
    gl_Position = mvp * vec4(in_vec.xy, 0.0, 1.0);
    ex_uv = in_vec.zw;
}
", /* fragment */ @"
in vec2 ex_uv;
uniform sampler2D tex;
uniform vec2 texSize;

void main (void)
{
    const float radius = 200.0;
    const float angle = 0.8;
    const vec2 center = vec2(400.0, 300.0);
    vec2 tc = ex_uv * texSize;
    tc -= center;
    float dist = length(tc);
    if (dist < radius) 
    {
      float percent = (radius - dist) / radius;
      float theta = percent * percent * angle * 8.0;
      float s = sin(theta);
      float c = cos(theta);
      tc = vec2(dot(tc, vec2(c, -s)), dot(tc, vec2(s, c)));
    }
    tc += center;
    vec3 color = texture2D(tex, tc / texSize).rgb;
    gl_FragColor = vec4(color, 1.0);
}
");
        }

        protected override void OnRemovedFromClip()
        {
            base.OnRemovedFromClip();
            this.shader.Dispose();
            this.shader = null;
            GL.DeleteVertexArray(this.vao);
            GL.DeleteBuffer(this.vbo);
        }

        public override void PostProcessFrame(long frame, RenderContext rc, Vector2? frameSize)
        {
            Vector2 size = frameSize ?? rc.FrameSize;

            Matrix4x4 matrix = Matrix4x4.CreateScale(size.X / 2f, size.Y / 2f, 1f) * rc.MatrixStack.Matrix;
            Matrix4x4 mvp = matrix * rc.Projection;

            this.shader.Use();
            this.shader.SetUniformMatrix4("mvp", ref mvp);
            this.shader.SetUniformVec2("texSize", size);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, rc.ActiveFrameBufferTexture);

            GL.BindVertexArray(this.vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
        }
    }
}