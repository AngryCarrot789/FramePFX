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
        Task RenderToViewPortAsync(TimelineViewModel timeline, bool scheduleRender = false);

        /// <summary>
        /// Opens and selects the UI element representing the given timeline. If it is not open,
        /// a new UI object is created and selected. If it is already open but not selected,
        /// then it is selected. And if it's already selected then nothing happens
        /// </summary>
        /// <param name="timeline">The timeline to show in the UI</param>
        void OpenAndSelectTimeline(TimelineViewModel timeline);

        /// <summary>
        /// Called by the editor when the project frame rate changes, in order to try to adjust the timeline zoom
        /// level and scroll such that it looks like nothing visually changed (apart from the ruler times which will change)
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="ratio"></param>
        void OnFrameRateRatioChanged(TimelineViewModel timeline, double ratio);

        /// <summary>
        /// Invoked on the main thread when an export begins
        /// </summary>
        /// <param name="prepare">True when on the main thread, false when on the export thread</param>
        void OnExportBegin(bool prepare);

        /// <summary>
        /// Invoked on the export thread when an export ends
        /// </summary>
        void OnExportEnd();

        void CloseAllTimelinesExcept(TimelineViewModel timeline);
    }
}