namespace FramePFX.Editor.Audio
{
    public unsafe struct AudioBusBuffers
    {
        public int numChannels;
        public long silenceFlags;
        public float** channelBuffers32; // 32-bit precision -- not used
        public double** channelBuffers64; // 64-bit precision
    }
}