using FramePFX.Services.Messages;

namespace FramePFX {
    public static class IoC {
        public static IMessageDialogService MessageService => ApplicationCore.Instance.Services.GetService<IMessageDialogService>();
    }
}