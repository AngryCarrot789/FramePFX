using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class AudioTrackControl : TimelineTrackControl {
        public AudioTrackControl() {
        }

        protected override bool IsItemAContainer(object item) => item is AudioClipControl;

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