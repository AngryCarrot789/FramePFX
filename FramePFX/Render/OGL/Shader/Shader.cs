using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FramePFX.Core.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Render.OGL.Shader {
    // Took this from my AtominaCraftV4 project (was trying to remake minecraft lol)
    public class Shader {
        private readonly List<string> attributes;
        private readonly string name;
        private int vertexId;
        private int fragmentId;
        private readonly int programId;
        private readonly int nextAttribute;

        private readonly Dictionary<string, int> uniformVariables;

        public static readonly Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        public int ProgramId => this.programId;

        public string Name => this.name;

        private static void LoadShader(string directory, string name) {
            try {
                shaders[name] = Load(Path.Combine(directory, name + ".vert"), Path.Combine(directory, name + ".frag"));
            }
            catch (Exception e) {
                throw new Exception($"Failed to load shader: {name}", e);
            }
        }

        static Shader() {
            LoadShader(ResourceLocator.ShaderFolderPath, "pink");
            LoadShader(ResourceLocator.ShaderFolderPath, "xyz");
            LoadShader(ResourceLocator.ShaderFolderPath, "rgb");
            LoadShader(ResourceLocator.ShaderFolderPath, "sky");
            LoadShader(ResourceLocator.ShaderFolderPath, "texture");
            LoadShader(ResourceLocator.ShaderFolderPath, "textureatlas");
        }

        public static Shader Load(string vertexPath, string fragmentPath) {
            if (!File.Exists(vertexPath)) {
                throw new FileNotFoundException(vertexPath);
            }

            if (!File.Exists(fragmentPath)) {
                throw new FileNotFoundException(fragmentPath);
            }

            StringBuilder vc = new StringBuilder(512);
            foreach (string line in File.ReadLines(vertexPath)) {
                vc.AppendLine(line);
            }

            StringBuilder fc = new StringBuilder(512);
            foreach (string line in File.ReadLines(fragmentPath)) {
                fc.AppendLine(line);
            }

            return new Shader(Path.GetFileNameWithoutExtension(vertexPath), vc.ToString(), fc.ToString());
        }

        public Shader(string name, string vertexCode, string fragmentCode) {
            this.name = name;
            this.attributes = new List<string>();
            this.uniformVariables = new Dictionary<string, int>();
            this.Compile(vertexCode, fragmentCode);

            this.programId = GL.CreateProgram();
            GL.AttachShader(this.ProgramId, this.vertexId);
            GL.AttachShader(this.ProgramId, this.fragmentId);

            foreach (string attribute in this.attributes) {
                if (attribute.StartsWith("gl_")) {
                    throw new Exception($"Name cannot start with 'gl_', as it is reserved, for shader '{this.Name}'");
                }
                else if (attribute.IndexOf(' ') != -1) {
                    throw new Exception($"Name cannot contain whitespaces, as variables cannot have those, for shader '{this.Name}'");
                }

                GL.BindAttribLocation(this.ProgramId, this.nextAttribute++, attribute);
            }

            GL.LinkProgram(this.ProgramId);
            GL.DeleteShader(this.vertexId);
            GL.DeleteShader(this.fragmentId);
        }

        public int GetUniformLocation(string name) {
            if (this.uniformVariables.TryGetValue(name, out int uniform)) {
                return uniform;
            }

            return this.uniformVariables[name] = GL.GetUniformLocation(this.ProgramId, name);
        }

        public void SetUniformBool(string name, bool value) {
            GL.Uniform1(this.GetUniformLocation(name), value ? 1 : 0);
        }

        public void SetUniformInt(string name, int value) {
            GL.Uniform1(this.GetUniformLocation(name), value);
        }

        public void SetUniformFloat(string name, float value) {
            GL.Uniform1(this.GetUniformLocation(name), value);
        }

        public void SetUniformVec2(string name, in Vector2 value) {
            GL.Uniform2(this.GetUniformLocation(name), value.X, value.Y);
        }

        public void SetUniformVec2(int location, in Vector2 value) {
            GL.Uniform2(location, value.X, value.Y);
        }

        public static void SetUniformVec2(int location, float x, float z) {
            GL.Uniform2(location, x, z);
        }

        public void SetUniformVec3(string name, in Vector3 value) {
            GL.Uniform3(this.GetUniformLocation(name), value.X, value.Y, value.Z);
        }

        public void SetUniformVec4(string name, in Vector4 value) {
            GL.Uniform4(this.GetUniformLocation(name), value.X, value.Y, value.Z, value.W);
        }

        public void SetUniformMatrix4(string name, Matrix4 value) {
            this.SetUniformMatrix4(name, ref value);
        }

        public void SetUniformMatrix4(string name, ref Matrix4 value) {
            GL.UniformMatrix4(this.GetUniformLocation(name), true, ref value);
        }

        public static void SetUniformMatrix4(int location, Matrix4 value) {
            SetUniformMatrix4(location, ref value);
        }

        public static void SetUniformMatrix4(int location, ref Matrix4 value) {
            GL.UniformMatrix4(location, true, ref value);
        }

        public void Use() {
            GL.UseProgram(this.ProgramId);
        }

        private void Compile(string vertexCode, string fragmentCode) {
            this.vertexId = this.CompileSource(vertexCode, ShaderType.VertexShader);
            this.fragmentId = this.CompileSource(fragmentCode, ShaderType.FragmentShader);
        }

        private int CompileSource(string sourceCode, ShaderType type) {
            int id = GL.CreateShader(type);
            GL.ShaderSource(id, sourceCode);
            GL.CompileShader(id);

            int[] isCompiled = new int[1];
            GL.GetShader(id, ShaderParameter.CompileStatus, isCompiled);
            if (isCompiled[0] < 1) {
                throw new Exception($"Failed to compile shader '{this.Name}':\n" + GL.GetShaderInfoLog(id));
            }

            // if (type == ShaderType.VertexShader) {
                foreach(string line in sourceCode.Split('\n')) {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed[0] == '/') {
                        continue;
                    }

                    if (trimmed.Length < 2 || !trimmed.StartsWith("in")) {
                        continue;
                    }

                    int spaceIndex = trimmed.IndexOf(' ', 3);
                    if (spaceIndex == -1) {
                        throw new Exception($"Missing whitespace between input variable type and name, but it compiled???? for shader '{this.Name}': " + trimmed);
                    }

                    int endIndex = trimmed.IndexOf(';', spaceIndex);
                    if (endIndex == -1) {
                        throw new Exception($"Missing semicolon for input variable, but it compiled???? for shader '{this.Name}': " + trimmed);
                    }

                    this.attributes.Add(trimmed.JSubstring(spaceIndex + 1, endIndex));
                }
            // }

            return id;
        }
    }
}