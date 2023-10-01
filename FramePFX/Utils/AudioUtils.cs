using System;

namespace FramePFX.Utils
{
    public static class AudioUtils
    {
        public static double DbToVolume(double db) => Math.Pow(10d, 0.05d * db);
        public static float DbToVolume(float db) => (float) Math.Pow(10d, 0.05d * db);

        public static double VolumeToDb(double vol) => 20d * Math.Log10(vol);
        public static float VolumeToDb(float vol) => (float) (20d * Math.Log10(vol));
    }
}