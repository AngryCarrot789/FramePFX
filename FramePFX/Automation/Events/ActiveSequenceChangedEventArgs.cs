using FramePFX.Automation.ViewModels.Keyframe;

namespace FramePFX.Automation.Events {
    public readonly struct ActiveSequenceChangedEventArgs {
        /// <summary>
        /// The previously active sequence
        /// </summary>
        public AutomationSequenceViewModel OldSequence { get; }

        /// <summary>
        /// The new active sequence
        /// </summary>
        public AutomationSequenceViewModel Sequence { get; }

        public ActiveSequenceChangedEventArgs(AutomationSequenceViewModel oldSequence, AutomationSequenceViewModel sequence) {
            this.OldSequence = oldSequence;
            this.Sequence = sequence;
        }
    }
}