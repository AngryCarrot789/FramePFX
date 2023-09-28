using System;
using System.Numerics;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.ViewModels.Timelines.Events;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.Effects.Video {
    public class TwirlEffectViewModel : VideoEffectViewModel {
        public new TwirlEffect Model => (TwirlEffect) base.Model;

        public TwirlEffectViewModel(TwirlEffect model) : base(model) {
        }
    }
}