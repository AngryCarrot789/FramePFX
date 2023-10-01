namespace FramePFX.Shortcuts.Managing
{
    public enum RepeatMode
    {
        /// <summary>
        /// Repeated input strokes are ignored
        /// </summary>
        Ignored,

        /// <summary>
        /// Input strokes that were not repeated are accepted (as in, when a user holds down a key,
        /// this requires that only that first key stroke should be processed, and any
        /// proceeding key strokes that are not release key strokes should be ignored)
        /// </summary>
        NonRepeat,

        /// <summary>
        /// Input strokes that are only processable when they are repeated. No real reason to handle this but it's a possibility
        /// </summary>
        RepeatOnly
    }
}