namespace FramePFX.Core.Automation {
    public readonly struct RefreshAutomationValueEventArgs {
        /// <summary>
        /// The frame where the play head is
        /// </summary>
        public readonly long Frame;

        /// <summary>
        /// Whether or not this refresh was caused by the playback. When true (caused by playback), a render
        /// should not be scheduled as the refresh was likely called just before the render happens
        /// </summary>
        public readonly bool IsPlaybackSource;

        public RefreshAutomationValueEventArgs(long frame, bool isPlaybackSource) {
            this.Frame = frame;
            this.IsPlaybackSource = isPlaybackSource;
        }
    }
}