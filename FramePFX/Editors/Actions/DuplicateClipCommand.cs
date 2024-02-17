using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.Actions {
    public class DuplicateClipCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip clip) && clip.Track is Track track) {
                if (clip.Track.TryGetSpanUntilClip(clip.FrameSpan.EndIndex, out FrameSpan span, clip.FrameSpan.Duration, clip.FrameSpan.Duration)) {
                    if (track.Timeline != null) {
                        track.Timeline.TryExpandForFrame(span.EndIndex);
                    }

                    Clip clone = clip.Clone();
                    clone.FrameSpan = span;
                    track.AddClip(clone);
                    clip.IsSelected = false;
                    clone.IsSelected = true;
                    if (track.Timeline != null) {
                        VideoEditorPropertyEditor.Instance.UpdateClipSelectionAsync(track.Timeline);
                    }
                }
            }
        }
    }
}