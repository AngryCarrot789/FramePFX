namespace FramePFX.History {
    public static class HistoryExtensions {
        /// <summary>
        /// Returns a disposable struct (which can be used in a using statement) that sets
        /// the <see cref="IHistoryHolder.IsHistoryChanging"/> property to true at the call time,
        /// and then sets it to false when disposed. This is used as a fail-fast guard against view-model
        /// properties being modification unknowingly
        /// </summary>
        /// <param name="holder">Holder whose values are about to be modified</param>
        /// <returns>A history usage struct</returns>
        public static HistoryUsage PushUsage(this IHistoryHolder holder) {
            return new HistoryUsage(holder);
        }
    }
}