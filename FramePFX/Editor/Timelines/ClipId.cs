namespace FramePFX.Editor.Timelines
{
    /// <summary>
    /// A universal ID for a clip
    /// </summary>
    public readonly struct ClipId
    {
        public readonly long trackId;
        public readonly long clipId;

        public ClipId(long trackId, long clipId)
        {
            this.trackId = trackId;
            this.clipId = clipId;
        }
    }
}