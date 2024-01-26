using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Controls {
    public interface IClipControl {
        ITrackControl Track { get; }

        Clip Model { get; }

        void OnAddingToTrack();
        void OnAddToTrack();
        void OnRemovingFromTrack();
        void OnRemovedFromTrack();
    }
}