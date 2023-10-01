using System.Collections.Generic;
using System.Numerics;

namespace FramePFX.Rendering
{
    public class MatrixStack
    {
        private readonly Stack<Matrix4x4> oldMatrices;

        /// <summary>
        /// The current translation matrix. This can be modified at will
        /// </summary>
        public Matrix4x4 Matrix;

        public MatrixStack()
        {
            this.oldMatrices = new Stack<Matrix4x4>();
            this.Clear();
        }

        /// <summary>
        /// Pushes the previous matrix into a stack and then multiplies that matrix by the given matrix
        /// </summary>
        /// <param name="matrix">Input matrix</param>
        public void PushMatrix(Matrix4x4 matrix) => this.PushMatrix(ref matrix);

        /// <summary>
        /// Pushes the previous matrix into a stack and then multiplies that matrix by the given matrix
        /// </summary>
        /// <param name="matrix">Input matrix</param>
        public void PushMatrix(ref Matrix4x4 matrix)
        {
            this.oldMatrices.Push(this.Matrix);
            this.Matrix *= matrix;
        }

        /// <summary>
        /// Pushes the previous matrix into a stack and then sets <see cref="Matrix"/> to the given matrix.
        /// Similar to <see cref="PushMatrix(System.Numerics.Matrix4x4)"/> but without multiplying any matrices
        /// </summary>
        /// <param name="matrix"></param>
        public void PushReplaceMatrix(Matrix4x4 matrix)
        {
            this.oldMatrices.Push(this.Matrix);
            this.Matrix = matrix;
        }

        /// <summary>
        /// Pops the last matrix in the stack and <see cref="Matrix"/> as it and also returns that matrix
        /// </summary>
        /// <returns>The previous (and now current) matrix</returns>
        public Matrix4x4 PopMatrix()
        {
            Matrix4x4 top = this.oldMatrices.Pop();
            this.Matrix = top;
            return top;
        }

        /// <summary>
        /// Clears all matrices and sets <see cref="Matrix"/> to <see cref="Matrix4x4.Identity"/>
        /// </summary>
        public void Clear()
        {
            this.oldMatrices.Clear();
            this.Matrix = Matrix4x4.Identity;
        }

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