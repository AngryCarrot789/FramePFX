// 
// Copyright (c) 2026-2026 REghZy
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

using PFXToolKitUI.Utils.Events;

namespace FramePFX.Audio;

/// <summary>
/// An abstraction around an audio system
/// </summary>
public abstract class AudioSystem {
    private static AudioSystem? instance;

    public static AudioSystem Instance => instance ?? throw new InvalidOperationException("No audio system available");

    public static bool IsAvailable => instance != null;

    private readonly RingBuffer sampleBuffer;

    /// <summary>
    /// Gets whether playback is running
    /// </summary>
    public bool IsPlaying {
        get => field;
        private set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IsPlayingChanged);
    }

    /// <summary>
    /// Gets the sample rate of this audio system
    /// </summary>
    public int SampleRate { get; }

    public event EventHandler? IsPlayingChanged;
    
    /// <summary>
    /// An event raised when the audio engine has enqueued samples into a driver/hardware to be played back 
    /// </summary>
    public event EventHandler<PlaybackProgressedEventArgs>? PlaybackProgressed;

    protected AudioSystem(int sampleRate) {
        this.SampleRate = sampleRate;
        this.sampleBuffer = new RingBuffer(sampleRate);
    }

    /// <summary>
    /// Begins asynchronous playback of the audio buffer
    /// </summary>
    public void BeginPlayback() {
        if (!this.IsPlaying) {
            this.IsPlaying = true;
            this.BeginPlaybackCore();
        }
    }

    /// <summary>
    /// Stops asynchronous playback of the audio buffer
    /// </summary>
    public void StopPlayback() {
        if (this.IsPlaying) {
            this.IsPlaying = false;
            this.StopPlaybackCore();
            this.sampleBuffer.Clear();
        }
    }

    /// <summary>
    /// Enqueues the samples for playback into our ring buffer
    /// </summary>
    /// <param name="samples">The samples to copy into the head of our playback buffer</param>
    /// <returns>
    /// The amount of samples actually copied. <c>samples.Length - Enqueue(samples)</c> will specify how
    /// many samples were not copied due to the playback buffer being full
    /// </returns>
    public int Enqueue(ReadOnlySpan<float> samples) {
        return this.sampleBuffer.Enqueue(samples);
    }

    /// <summary>
    /// Reads samples from our ring buffer. This offsets the tail and clears any samples there after being copied to the provided span
    /// </summary>
    /// <param name="dstSamples">The destination for samples to be read from</param>
    /// <returns>The amount of samples actually written into the span</returns>
    public int ReadSamples(Span<float> dstSamples) {
        return this.sampleBuffer.Read(dstSamples);
    }

    /// <summary>
    /// Reads samples from our ring buffer without modifying the tail position nor clearing the read data
    /// </summary>
    /// <param name="dstSamples">The destination for samples to be read from</param>
    /// <returns>The amount of samples actually written into the span</returns>
    public int PeekSamples(Span<float> dstSamples) {
        return this.sampleBuffer.Read(dstSamples);
    }

    /// <summary>
    /// Implementation of the BeginPlayback function
    /// </summary>
    protected abstract void BeginPlaybackCore();

    /// <summary>
    /// Implementation of the StopPlayback function
    /// </summary>
    protected abstract void StopPlaybackCore();

    protected virtual void RaisePlaybackProgressed(int count) {
        this.PlaybackProgressed?.Invoke(this, new PlaybackProgressedEventArgs(count));
    }

    protected virtual void OnOpened() {
        
    }

    /// <summary>
    /// Dispose of any native components
    /// </summary>
    protected virtual void Dispose() {
    }

    /// <summary>
    /// Sets the global audio system for FramePFX
    /// </summary>
    /// <param name="newAudioSystem">The new audio system</param>
    /// <exception cref="InvalidOperationException">Audio system already opened</exception>
    public static void OpenAudioSystem(AudioSystem newAudioSystem) {
        ArgumentNullException.ThrowIfNull(newAudioSystem);
        if (instance != null)
            throw new InvalidOperationException("Audio system already opened");

        instance = newAudioSystem;
        instance.OnOpened();
    }

    /// <summary>
    /// Closes the current audio system
    /// </summary>
    /// <exception cref="InvalidOperationException">No audio system opened</exception>
    public static void CloseAudioSystem() {
        if (instance == null)
            throw new InvalidOperationException("No audio system opened");

        try {
            instance.StopPlayback();
            instance.Dispose();
        }
        finally {
            instance = null;
        }
    }
}

public readonly struct PlaybackProgressedEventArgs(int sampledProcessed) {
    public int SampledProcessed { get; } = sampledProcessed;
}