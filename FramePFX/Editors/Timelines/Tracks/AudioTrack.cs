// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Runtime.InteropServices;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines.Tracks {
    public class AudioTrack : Track {
        private unsafe float* renderedSamples; // float*

        private unsafe Span<float> Samples => new Span<float>(this.renderedSamples, 4096);

        private AudioClip theClipToRender;

        public AudioTrack() {
        }

        public override bool IsClipTypeAccepted(Type type) {
            return typeof(AudioClip).IsAssignableFrom(type);
        }

        public override bool IsEffectTypeAccepted(Type effectType) {
            return false;
        }

        protected override unsafe void OnProjectChanged(Project oldProject, Project newProject) {
            base.OnProjectChanged(oldProject, newProject);
            if (newProject != null) {
                int bytes = newProject.Settings.BufferSize * 2 * sizeof(float);
                this.renderedSamples = (float*) Marshal.AllocHGlobal(bytes);
            }
            else {
                Marshal.FreeHGlobal((IntPtr) this.renderedSamples);
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
            this.theClipToRender.ProvideSamples(this.renderedSamples, samples);
        }

        public unsafe void WriteSamples(AudioRingBuffer dstBuffer, long samples) {
            dstBuffer.WriteToRingBuffer(this.renderedSamples, (int) samples);
        }
    }
}