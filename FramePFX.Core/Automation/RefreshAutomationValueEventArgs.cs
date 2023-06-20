namespace FramePFX.Core.Automation {
    public readonly struct RefreshAutomationValueEventArgs {
        /// <summary>
        /// The frame where the play head is
        /// </summary>
        public readonly long Frame;

        /// <summary>
        /// Whether or not this refresh was caused by the playback
        /// </summary>
        public readonly bool IsPlaybackSource;

        public RefreshAutomationValueEventArgs(long frame, bool isPlaybackSource) {
            this.Frame = frame;
            this.IsPlaybackSource = isPlaybackSource;
        }
    }
}