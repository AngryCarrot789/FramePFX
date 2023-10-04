using System.Collections.Generic;
using System.Numerics;

namespace FramePFX.Rendering
{
    public class MatrixStack
    {
        private readonly Matrix4x4 identity;
        private readonly Stack<Matrix4x4> stack;

        /// <summary>
        /// The current translation matrix. This can be modified at will
        /// </summary>
        public Matrix4x4 Matrix;

        /// <summary>
        /// Gets the number of matrices in the stack
        /// </summary>
        public int Count => this.stack.Count;

        public IReadOnlyCollection<Matrix4x4> Stack => this.stack;

        public MatrixStack() : this(Matrix4x4.Identity) { }

        public MatrixStack(Matrix4x4 identity)
        {
            this.stack = new Stack<Matrix4x4>();
            this.identity = identity;
            this.Matrix = identity;
        }

        /// <summary>
        /// Pushes the current matrix (<see cref="Matrix"/>) onto the stack
        /// </summary>
        public void PushMatrix() => this.stack.Push(this.Matrix);

        /// <summary>
        /// Pops the last matrix in the stack and sets <see cref="Matrix"/> as that matrix
        /// </summary>
        public void PopMatrix() => this.Matrix = this.stack.Pop();

        /// <summary>
        /// Clears all matrices and sets <see cref="Matrix"/> to <see cref="Matrix4x4.Identity"/>
        /// </summary>
        public void Clear()
        {
            this.stack.Clear();
            this.Matrix = this.identity;
        }

        public void Translate(Vector3 pos) => this.Matrix *= Matrix4x4.CreateTranslation(pos);
        public void Scale(Vector3 scale) => this.Matrix *= Matrix4x4.CreateScale(scale);
        public void Scale(Vector3 scale, Vector3 origin) => this.Matrix *= Matrix4x4.CreateScale(scale, origin);
        public void RotateX(float radians) => this.Matrix *= Matrix4x4.CreateRotationX(radians);
        public void RotateX(float radians, Vector3 origin) => this.Matrix *= Matrix4x4.CreateRotationX(radians, origin);
        public void RotateY(float radians) => this.Matrix *= Matrix4x4.CreateRotationY(radians);
        public void RotateY(float radians, Vector3 origin) => this.Matrix *= Matrix4x4.CreateRotationY(radians, origin);
        public void RotateZ(float radians) => this.Matrix *= Matrix4x4.CreateRotationZ(radians);
        public void RotateZ(float radians, Vector3 origin) => this.Matrix *= Matrix4x4.CreateRotationZ(radians, origin);

        // not using these; was just using to debug if matrices were actually working

        public static Vector3 MultiplyMatrix(ref Matrix4x4 m, Vector3 v)
        {
            Vector4 v4 = Transform(new Vector4(v.X, v.Y, v.Z, 1.0f), ref m);
            Vector3 r = new Vector3(v4.X / v4.W, v4.Y / v4.W, v4.Z / v4.W);
            return r;
        }

        public static Vector4 Transform(Vector4 v, ref Matrix4x4 m)
        {
            return new Vector4(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + v.W * m.M41,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + v.W * m.M42,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + v.W * m.M43,
                v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + v.W * m.M44);
        }
    }
}