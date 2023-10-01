using System.Numerics;

namespace FramePFX.Rendering
{
    public class Camera
    {
        public Quaternion orientation;
        public Vector3 pos;
        public CamMode mode;
        public float fov;
        public float near;
        public float far;

        public static Camera CreateDefault()
        {
            Camera cam = new Camera()
            {
                orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, -180f, 0), 0f),
                pos = new Vector3(),
                fov = 75f
            };

            return cam;
        }

        public enum CamMode
        {
            Orthographic, Perspective
        }
    }
}