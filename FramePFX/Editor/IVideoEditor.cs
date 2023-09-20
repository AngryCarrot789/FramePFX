using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Notifications;
using FramePFX.Views;

namespace FramePFX.Editor {
    /// <summary>
    /// An interface for a video editor view
    /// </summary>
    public interface IVideoEditor : IViewBase {
        Task RenderTimelineAsync(TimelineViewModel timeline, bool scheduleRender = false);

        void PushNotificationMessage(string message);

        NotificationPanelViewModel NotificationPanel { get; }
    }
}