using System;
using System.Numerics;
using OpenTK;
using Vector3 = System.Numerics.Vector3;

namespace FramePFX.Rendering.Utils
{
    public class MatrixUtils
    {
        /// <summary>
        /// Creates a matrix that can be used to transform world coordinates into local coordinates, using the given position, rotation and scale
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotate"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Matrix4x4 WorldToLocal(Vector3 position, Vector3 rotate, Vector3 scale)
        {
            Vector3 s = new Vector3(1.0f / scale.X, 1.0f / scale.Y, 1.0f / scale.Z);
            return Matrix4x4.CreateScale(s) * CreateNegativeRotationZXY(rotate) * Matrix4x4.CreateTranslation(-position);
        }

        /// <summary>
        /// Creates a matrix that can be used to transform local coordinates into world coordinates, using the given position, rotation and scale
        /// <para>
        /// A Translation matrix is created, then it is multiplied by 3 rotation matrices (X, Y then Z), then that is multiplied by a scale matrix
        /// </para>
        /// </summary>
        /// <param name="position">Position (aka origin of the translation)</param>
        /// <param name="rotate">Rotation, relative to the new position</param>
        /// <param name="scale">Scale, relative to the new position</param>
        /// <returns>A transformation matrix, aka model matrix</returns>
        public static Matrix4x4 LocalToWorld(Vector3 position, Vector3 rotate, Vector3 scale)
        {
            return Matrix4x4.CreateTranslation(position) * CreateRotationYXZ(rotate) * Matrix4x4.CreateScale(scale);
        }

        /// <summary>
        /// Creates three rotation matrices and multiplies them in the order of Y, X and Z
        /// </summary>
        /// <param name="rotation">Input rotation vector</param>
        /// <returns>Output rotation matrix</returns>
        public static Matrix4x4 CreateRotationYXZ(Vector3 rotation) => Matrix4x4.CreateRotationY(rotation.Y) * Matrix4x4.CreateRotationX(rotation.X) * Matrix4x4.CreateRotationZ(rotation.Z);

        /// <summary>
        /// Creates three rotation matrices and multiplies them in the order of Z, X and Y
        /// </summary>
        /// <param name="rotation">Input rotation vector</param>
        /// <returns>Output rotation matrix</returns>
        public static Matrix4x4 CreateNegativeRotationZXY(Vector3 rotation) => Matrix4x4.CreateRotationZ(-rotation.Z) * Matrix4x4.CreateRotationX(-rotation.X) * Matrix4x4.CreateRotationY(-rotation.Y);

        public static Matrix4x4 CreateHeading(float heading)
        {
            float cos = (float) Math.Cos(heading);
            float sin = (float) Math.Sin(heading);
            Matrix4x4 mat = Matrix4x4.Identity;
            mat.M11 = cos;
            mat.M12 = -sin;
            mat.M21 = sin;
            mat.M22 = cos;
            return mat;
        }

        public static Matrix4x4 CreatePitch(float pitch)
        {
            float cos = (float) Math.Cos(pitch);
            float sin = (float) Math.Sin(pitch);
            Matrix4x4 mat = Matrix4x4.Identity;
            mat.M11 = cos;
            mat.M13 = sin;
            mat.M31 = -sin;
            mat.M33 = cos;
            return mat;
        }

        public static Matrix4x4 CreateBearing(float pitch)
        {
            float cos = (float) Math.Cos(pitch);
            float sin = (float) Math.Sin(pitch);
            Matrix4x4 mat = Matrix4x4.Identity;
            mat.M22 = cos;
            mat.M23 = -sin;
            mat.M32 = sin;
            mat.M33 = cos;
            return mat;
        }

        public static Matrix4x4 CreateRotationYPR(Vector3 rotation) => CreateHeading(rotation.Y) * CreatePitch(rotation.X) * CreateBearing(rotation.Z);

        public static Matrix4d LookAt(Vector3d src, Vector3d dst)
        {
            Vector3d zaxis = Vector3d.Normalize(src - dst);
            Vector3d xaxis = Vector3d.Normalize(new Vector3d(zaxis.Z, 0f, -zaxis.X));
            Vector3d yaxis = Vector3d.Cross(zaxis, xaxis);
            double dX = -Vector3d.Dot(xaxis, src);
            double dY = -Vector3d.Dot(yaxis, src);
            double dZ = -Vector3d.Dot(zaxis, src);

            Matrix4d result = new Matrix4d(
                new Vector4d(xaxis.X, yaxis.X, zaxis.X, 0f),
                new Vector4d(xaxis.Y, yaxis.Y, zaxis.Y, 0f),
                new Vector4d(xaxis.Z, yaxis.Z, zaxis.Z, 0f),
                new Vector4d(dX, dY, dZ, 1f)
            );

            return result;
        }
    }
}