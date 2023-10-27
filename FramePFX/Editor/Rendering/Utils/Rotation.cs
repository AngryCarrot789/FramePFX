using System;
using System.Numerics;

namespace FramePFX.Editor.Rendering.Utils {
    public static class Rotation {
        /// <summary>
        /// Returns the position of a camera that is at a specific yaw and pitch, orbiting around 0,0,0
        /// </summary>
        public static Vector3 GetOrbitPosition(float yaw, float pitch) {
            Vector3 dir = new Vector3(
                (float) (Math.Cos(-pitch) * Math.Sin(yaw)),
                (float) Math.Sin(-pitch),
                (float) (Math.Cos(-pitch) * Math.Cos(yaw))
            );

            return dir;
        }
    }
}