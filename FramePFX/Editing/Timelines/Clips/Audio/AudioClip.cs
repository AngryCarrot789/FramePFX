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

namespace FramePFX.Editing.Timelines.Clips.Audio;

public class AudioClip : Clip
{
    private float phase;

    public override bool IsEffectTypeAccepted(Type effectType)
    {
        return false;
    }

    public bool BeginRenderAudio(long frame, long sampleFrames)
    {
        return true;
    }

    /// <summary>
    /// The main audio samples provider for audio clips
    /// </summary>
    /// <param name="outputs">An array of output channels</param>
    /// <param name="sampleFrames">The number of samples to generate</param>
    public unsafe void ProvideSamples(float* outSamples, long sampleFrames, float trackAmplitude)
    {
        const int sampleRate = 44100;
        float amplitude = 0.5F * trackAmplitude;
        const float freq = 440F;
        const float deltaPhase = (float) (2.0 * Math.PI * freq / sampleRate);
        const float PI2 = (float) Math.PI * 2.0F;

        for (int i = 0; i < sampleFrames; ++i)
        {
            float sample = (float) (Math.Sin(this.phase) * amplitude);

            *outSamples++ = sample;
            *outSamples++ = sample;

            this.phase += deltaPhase;
            if (this.phase >= PI2)
                this.phase -= PI2;
        }
    }
}