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

using System.Runtime.InteropServices;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines.Clips.Audio;
using FramePFX.Utils.Accessing;

namespace FramePFX.Editing.Timelines.Tracks;

public class AudioTrack : Track
{
    public static readonly ParameterBool IsMutedParameter =
        Parameter.RegisterBool(
            typeof(AudioTrack),
            nameof(AudioTrack),
            nameof(IsMuted), false,
            ValueAccessors.Reflective<bool>(typeof(AudioTrack), nameof(IsMuted)),
            ParameterFlags.StandardProjectVisual);

    public static readonly ParameterFloat VolumeParameter =
        Parameter.RegisterFloat(
            typeof(AudioTrack),
            nameof(AudioTrack),
            nameof(Volume), 1.0F, 0.0F, 1.0F,
            ValueAccessors.Reflective<float>(typeof(AudioTrack), nameof(Volume)),
            ParameterFlags.StandardProjectVisual);

    private bool IsMuted = IsMutedParameter.Descriptor.DefaultValue;
    private float Volume = VolumeParameter.Descriptor.DefaultValue;

    public unsafe float* renderedSamples; // float*
    public int renderedSamplesCount;

    public unsafe Span<float> Samples => new Span<float>(this.renderedSamples, 4096);

    private AudioClip theClipToRender;
    private float render_Amplitude;

    public AudioTrack() {
    }

    public override bool IsClipTypeAccepted(Type type)
    {
        return typeof(AudioClip).IsAssignableFrom(type);
    }

    public override bool IsEffectTypeAccepted(Type effectType)
    {
        return false;
    }

    protected override unsafe void OnProjectChanged(Project? oldProject, Project? newProject)
    {
        base.OnProjectChanged(oldProject, newProject);
        if (newProject != null)
        {
            this.renderedSamplesCount = newProject.Settings.BufferSize * 2;
            this.renderedSamples = (float*) Marshal.AllocHGlobal(this.renderedSamplesCount * sizeof(float));
        }
        else
        {
            Marshal.FreeHGlobal((IntPtr) this.renderedSamples);
        }
    }

    public bool PrepareRenderFrame(long frame, long samples, EnumRenderQuality quality)
    {
        AudioClip clip = (AudioClip) this.GetClipAtFrame(frame);
        if (clip == null || !clip.BeginRenderAudio(frame, samples))
        {
            return false;
        }

        this.theClipToRender = clip;
        this.render_Amplitude = this.Volume;
        return true;
    }

    /// <summary>
    /// Renders this track's audio samples into its own internal buffer
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="quality"></param>
    public unsafe void RenderAudioFrame(long samples, EnumRenderQuality quality)
    {
        this.theClipToRender.ProvideSamples(this.renderedSamples, samples, this.render_Amplitude);
    }
}