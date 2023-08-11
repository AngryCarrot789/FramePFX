namespace FramePFX.Core.Automation {
    public readonly struct RefreshAutomationValueEventArgs {
        /// <summary>
        /// The frame where the play head is
        /// </summary>
        public readonly long Frame;

        /// <summary>
        /// Whether or not a playback is in progress
        /// </summary>
        public readonly bool IsDuringPlayback;

        /// <summary>
        /// Whether or not this refresh was caused by the playback, and not the user moving a clip or the timeline
        /// </summary>
        public readonly bool IsPlaybackTick;

        public RefreshAutomationValueEventArgs(long frame, bool isDuringPlayback, bool isPlaybackTick) {
            this.Frame = frame;
            this.IsDuringPlayback = isDuringPlayback;
            this.IsPlaybackTick = isPlaybackTick;
        }
    }
}