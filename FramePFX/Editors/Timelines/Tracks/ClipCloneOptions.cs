namespace FramePFX.Editors.Timelines.Tracks {
    public struct ClipCloneOptions {
        // no options yet, but in here could be options like
        // True/False to clone effects and automation data
        public static readonly ClipCloneOptions Default = new ClipCloneOptions(true, true, true);

        public readonly bool CloneAutomationData;
        public readonly bool CloneEffects;
        public readonly bool CloneResourceLinks;

        public ClipCloneOptions(bool cloneAutomationData, bool cloneEffects, bool cloneResourceLinks) {
            this.CloneAutomationData = cloneAutomationData;
            this.CloneEffects = cloneEffects;
            this.CloneResourceLinks = cloneResourceLinks;
        }
    }
}