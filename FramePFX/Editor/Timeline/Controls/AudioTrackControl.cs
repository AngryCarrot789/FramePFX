using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class AudioTrackControl : TimelineTrackControl {
        public AudioTrackControl() {
        }

        protected override TimelineClipControl GetContainerForItem(object item) {
            if (item == null || item is AudioClipViewModel)
                return new AudioClipControl();
            #if DEBUG
            Debugger.Break();
            #endif
            throw new Exception($"Tried to generate {nameof(AudioClipControl)} for {item.GetType()}");
        }
    }
}