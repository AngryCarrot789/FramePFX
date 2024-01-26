namespace FramePFX.Editors.Timelines.Tracks {
    public struct ClipCloneOptions {
        // no options yet, but in here could be options like
        // True/False to clone effects and automation data
        public static readonly ClipCloneOptions Default = new ClipCloneOptions(true, true);

        public readonly bool CloneAutomationData;
        public readonly bool CloneEffects;

        public ClipCloneOptions(bool cloneAutomationData, bool cloneEffects) {
            this.CloneAutomationData = cloneAutomationData;
            this.CloneEffects = cloneEffects;
        }
    }
}