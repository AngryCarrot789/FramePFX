namespace FramePFX.History {
    public interface IHistoryManager {
        void AddAction(IHistoryAction action, string information = null);
    }
}