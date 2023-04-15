
using OpenTK;

namespace FramePFX.Render.OGL.Shader {
    public class PinkShader : Shader {
        const string vertexShader =
            "#version 330\n" +
            "uniform mat4 mvp;\n" +
            "uniform mat4 mv;\n" +
            "in vec3 in_pos;\n" +
            "void main() { gl_Position = mvp * vec4(in_pos, 1.0); }\n";
            // "void main() { gl_Position = mvp * mv * gl_Vertex; }\n";


        const string fragmentShader =
            "#version 330\n" +
            "void main() { gl_FragColor = vec4(0.8, 0.2, 1.0, 1.0); }\n";

        public static readonly PinkShader Instance;

        private PinkShader() : base("Pink", vertexShader, fragmentShader) {

        }

        static PinkShader() {
            Instance = new PinkShader();
        }

        public void SetMVP(in Matrix4 matrix) {
            this.SetUniformMatrix4("mvp", matrix);
        }

        public void SetMV(in Matrix4 matrix) {
            this.SetUniformMatrix4("mv", matrix);
        }
    }
}