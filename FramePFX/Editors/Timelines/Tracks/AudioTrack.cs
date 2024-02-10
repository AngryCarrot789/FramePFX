using System;
using System.Runtime.InteropServices;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines.Tracks {
    public class AudioTrack : Track {
        private IntPtr channelLeft; // float*
        private IntPtr channelRight; // float*

        private AudioClip theClipToRender;

        public AudioTrack() {
        }

        public override bool IsClipTypeAccepted(Type type) {
            return typeof(AudioClip).IsAssignableFrom(type);
        }

        public override bool IsEffectTypeAccepted(Type effectType) {
            return false;
        }

        protected override void OnProjectChanged(Project oldProject, Project newProject) {
            base.OnProjectChanged(oldProject, newProject);
            if (newProject != null) {
                int bytes = newProject.Settings.BufferSize * sizeof(float);
                this.channelLeft = Marshal.AllocHGlobal(bytes);
                this.channelRight = Marshal.AllocHGlobal(bytes);
            }
            else {
                Marshal.FreeHGlobal(this.channelLeft);
                Marshal.FreeHGlobal(this.channelRight);
            }
        }

        public bool PrepareRenderFrame(long frame, long samples, EnumRenderQuality quality) {
            AudioClip clip = (AudioClip) this.GetClipAtFrame(frame);
            if (clip == null || !clip.BeginRenderAudio(frame, samples)) {
                return false;
            }

            this.theClipToRender = clip;
            return true;
        }

        /// <summary>
        /// Renders this track's audio samples into its own internal buffer
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="quality"></param>
        public unsafe void RenderAudioFrame(long samples, EnumRenderQuality quality) {
            this.theClipToRender.ProvideSamples((float*) this.channelLeft, (float*) this.channelRight, samples);
        }

        public unsafe void CopySamples(float* pDstLeft, float* pDstRight, long samples) {
            float* srcL = (float*) this.channelLeft;
            float* srcR = (float*) this.channelRight;

            for (int i = 0; i < samples; i++) {
                *pDstLeft++ = *srcL++;
                *pDstRight++ = *srcR++;
            }
        }
    }
}