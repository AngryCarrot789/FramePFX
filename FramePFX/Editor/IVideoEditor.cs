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
        /// Gets the editor's notification panel, used to push notifications
        /// </summary>
        NotificationPanelViewModel NotificationPanel { get; }

        /// <summary>
        /// Renders the current state of the timeline and draws it into the editor's view port
        /// </summary>
        /// <param name="timeline">The timeline to render</param>
        /// <param name="scheduleRender">True to schedule for some point in the future, false to render immediately</param>
        /// <returns>A task to await for the render to complete</returns>
        Task RenderTimelineAsync(TimelineViewModel timeline, bool scheduleRender = false);

        /// <summary>
        /// Opens and selects the UI element representing the given timeline. If it is not open,
        /// a new UI object is created and selected. If it is already open but not selected,
        /// then it is selected. And if it's already selected then nothing happens
        /// </summary>
        /// <param name="timeline">The timeline to show in the UI</param>
        void OpenAndSelectTimeline(TimelineViewModel timeline);

        /// <summary>
        /// Invoked when the project frame rate changes, in order to try to adjust the timeline zoom
        /// level and scroll such that it looks like nothing visually changed (apart from the ruler times which will change)
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="ratio"></param>
        void OnFrameRateRatioChanged(TimelineViewModel timeline, double ratio);
    }
}