namespace FrameControlEx.Core.Views.Dialogs.Message {
    public interface IMessageDialogStateStorage {
        // TODO: implement this to write the "remember this option" states to a file
        void Register(string id, MessageDialog dialog);
    }
}