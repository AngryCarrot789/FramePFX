using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History.ViewModels;

namespace FramePFX.History.Actions {
    public class UndoAction : AnAction {
        public static HistoryManagerViewModel GetHistoryManager(IDataContext context, out VideoEditorViewModel editor) {
            if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null && (editor = clip.Track.Timeline.Project.Editor) != null) {
                return editor.HistoryManager;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (editor = timeline.Project.Editor) != null) {
                return editor.HistoryManager;
            }
            else if (context.TryGetContext(out ProjectViewModel project) && (editor = project.Editor) != null) {
                return editor.HistoryManager;
            }
            else if (context.TryGetContext(out editor)) {
                return editor.HistoryManager;
            }
            else if (context.TryGetContext(out HistoryManagerViewModel manager)) {
                return manager;
            }
            else {
                return null;
            }
        }

        public static bool TryGetHistoryManager(IDataContext context, out HistoryManagerViewModel manager, out VideoEditorViewModel editor) {
            return (manager = GetHistoryManager(context, out editor)) != null;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (TryGetHistoryManager(e.DataContext, out var manager, out var editor)) {
                if (manager.CanUndo) {
                    await manager.UndoAction();
                }
                else {
                    editor.View.PushNotificationMessage("Nothing to undo!");
                }

                return true;
            }
            else {
                return false;
            }
        }
    }
}