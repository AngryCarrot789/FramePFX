using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Notifications;
using FramePFX.Views;

namespace FramePFX.Editor {
    /// <summary>
    /// An interface for a video editor view
    /// </summary>
    public interface IVideoEditor : IViewBase {
        /// <summary>
        /// Renders the current state of the timeline, and draws it into the editor's view port
        /// </summary>
        /// <param name="timeline">The timeline to render</param>
        /// <param name="scheduleRender">True to schedule for some point in the future, false to render immidiately</param>
        /// <returns></returns>
        Task RenderTimelineAsync(TimelineViewModel timeline, bool scheduleRender = false);

        NotificationPanelViewModel NotificationPanel { get; }

        void OpenTimeline(TimelineViewModel timeline);

        void SelectTimeline(TimelineViewModel timeline);
    }
}