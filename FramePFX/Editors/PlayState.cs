namespace FramePFX.Editors {
    public enum PlayState {
        /// <summary>
        /// Starts playing, optionally at a specific frame. This can also be used to jump to
        /// another frame too without pausing and playing (the jumped frame is used by stop too)
        /// </summary>
        Play,
        /// <summary>
        /// Pauses playback, saving the current playhead position
        /// </summary>
        Pause,
        /// <summary>
        /// Playback stopped, and the playhead is moved back to when we began playing
        /// </summary>
        Stop
    }
}