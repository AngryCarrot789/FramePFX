namespace FramePFX.Editor.Rendering.Utils {
    public enum RotationType {
        /// <summary>
        /// Rotation uses heading/yaw, elevation/pitch and bank/roll (aka YPR)
        /// </summary>
        Bearings,
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX
    }
}