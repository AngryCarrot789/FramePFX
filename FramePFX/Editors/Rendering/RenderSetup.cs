using System.Threading;
using SoundIOSharp;

namespace FramePFX.Editors.Rendering {
    public readonly struct RenderSetup {
        public readonly long Frame;
        public readonly CancellationToken CancellationToken;
        public readonly EnumRenderQuality RenderQuality;
        public readonly long AudioSamples;
        public readonly SoundIOFormat SoundFormat;
    }
}