using System.Threading.Tasks;
using FramePFX.Core.Notifications;
using FramePFX.Core.Views;

namespace FramePFX.Core.Editor {
    /// <summary>
    /// An interface for a video editor view
    /// </summary>
    public interface IVideoEditor : IViewBase {
        void Render(bool scheduleRender = false);

        Task RenderAsync();

        void UpdateClipSelection();

        void UpdateResourceSelection();

        void PushNotificationMessage(string message);

        NotificationPanelViewModel NotificationPanel { get; }
    }
}