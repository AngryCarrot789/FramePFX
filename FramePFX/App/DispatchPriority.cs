namespace FramePFX.App {
    /// <summary>
    /// A priority enum for dispatching method invocations onto a thread
    /// </summary>
    public enum DispatchPriority {
        /// <summary>
        /// Application idle priority, which is executed once all work from higher priorities is done
        /// </summary>
        AppIdle,

        /// <summary>
        /// Background priority, in which tasks are executed once all other critical tasks are completed
        /// </summary>
        Background,

        /// <summary>
        /// Rendering priority. Tasks executed with this priority will be executed after rendering is completed
        /// </summary>
        AfterRender,

        /// <summary>
        /// The normal priority which most things use. The dispatcher system won't be bypassed using this priority
        /// </summary>
        Normal,

        /// <summary>
        /// The highest priority. Allows the dispatcher system to be completely bypassed if already on the owner thread
        /// </summary>
        Send
    }
}