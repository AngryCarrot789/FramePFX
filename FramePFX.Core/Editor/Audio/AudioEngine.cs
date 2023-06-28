using System;
using System.Threading;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Tracks;
using FramePFX.Core.Utils;
using NAudio;
using NAudio.Wave;

namespace FramePFX.Core.Editor.Audio {
    public class AudioEngine {
        public const int TicksPerBar = 192;
        public const int StepsPerBar = 16;
        public const int BeatsPerBar = TicksPerBar / StepsPerBar; // 12

        // assume:
        //  fps = (30000 / 1001) = 29.97
        //  sampleRate = 44100
        //  samplesPerTick = (44100 / 29.97) = 1471.47147147

        private int sampleRate;
        private long samplesPerTick;
        private double rawSamplesPerTick;
        private long lastFrame;
        private long currentSample;

        private readonly object waveOutLock;
        private IntPtr hWaveOut;
        private AudioEngineWaveBuffer[] buffers;
        private volatile PlaybackState playbackState;
        private AutoResetEvent callbackEvent;

        public int DesiredLatency { get; set; }
        public int NumberOfBuffers { get; set; }
        public int DeviceNumber { get; set; } = -1;

        public WaveFormat OutputWaveFormat => this.WaveStream.WaveFormat;

        public PlaybackState PlaybackState => this.playbackState;

        public BufferedWaveProvider WaveStream { get; private set; }

        public float Volume {
            get => WaveOutUtils.GetWaveOutVolume(this.hWaveOut, this.waveOutLock);
            set => WaveOutUtils.SetWaveOutVolume(value, this.hWaveOut, this.waveOutLock);
        }

        public AudioEngine() {
            this.sampleRate = 44100;
            this.DesiredLatency = 300;
            this.NumberOfBuffers = 2;
            this.waveOutLock = new object();
        }

        ~AudioEngine() => this.Dispose(false);

        public void UpdateFPS(double fps) {
            this.rawSamplesPerTick = (this.sampleRate / fps);
            this.samplesPerTick = Maths.Ceil((long) Math.Ceiling(this.rawSamplesPerTick), 256);
        }

        // assume:
        //  frame = 30
        //  currentSample = 44144.144144144144144144144144144
        //  sampleOffset = 44.144144144144144144144144144144

        public void OnTick(TimelineModel timeline, long frame) {
            if ((frame - 1) != this.lastFrame) {
                this.currentSample = (long) Math.Ceiling(this.lastFrame * this.rawSamplesPerTick);
                // frame seeked
            }

            long nextSample = (long) Math.Ceiling(frame * this.rawSamplesPerTick);
            int offset = checked((int) (this.currentSample % this.sampleRate));
            int length = checked((int) this.samplesPerTick);

            foreach (TrackModel track in timeline.Tracks) {
                if (!(track is AudioTrackModel audioTrack) || audioTrack.IsMuted || audioTrack.Volume < 0.0001f) {
                    continue;
                }

                audioTrack.ProcessAudio(this, frame, offset, length);
            }

            this.PlayAudioSamples();
            this.lastFrame = frame;
        }

        public void Stop(bool disposeBuffers = true) {
            if (this.playbackState == PlaybackState.Stopped)
                return;
            this.playbackState = PlaybackState.Stopped;
            MmResult result;
            lock (this.waveOutLock)
                result = WaveInterop.waveOutReset(this.hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutReset");
            this.callbackEvent.Set();
            if (disposeBuffers && this.hWaveOut != IntPtr.Zero) {
                this.DisposeBuffers();
                this.CloseWaveOut();
            }
        }

        public void Start(WaveFormat format) {
            if (this.playbackState != PlaybackState.Stopped)
                throw new InvalidOperationException("Can't re-initialize during playback");
            if (this.hWaveOut != IntPtr.Zero) {
                this.DisposeBuffers();
                this.CloseWaveOut();
            }

            this.WaveStream = new BufferedWaveProvider(format);
            this.callbackEvent = new AutoResetEvent(false);
            int byteSize = format.ConvertLatencyToByteSize((this.DesiredLatency + this.NumberOfBuffers - 1) / this.NumberOfBuffers);
            MmResult result;
            lock (this.waveOutLock) {
                result = WaveInterop.waveOutOpenWindow(out this.hWaveOut, (IntPtr) this.DeviceNumber, this.WaveStream.WaveFormat, this.callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackEvent);
            }

            MmException.Try(result, "waveOutOpen");
            this.buffers = new AudioEngineWaveBuffer[this.NumberOfBuffers];
            for (int index = 0; index < this.NumberOfBuffers; ++index) {
                this.buffers[index] = new AudioEngineWaveBuffer(this.hWaveOut, byteSize, this.WaveStream, this.waveOutLock);
            }

            this.playbackState = PlaybackState.Playing;
            this.callbackEvent.Set();
        }

        public void PlayAudioSamples() {
            foreach (AudioEngineWaveBuffer buffer in this.buffers) {
                if (buffer.InQueue || buffer.ProcessBuffer()) {
                }
            }
        }

        public void Pause() {
            if (this.playbackState != PlaybackState.Playing)
                return;
            this.playbackState = PlaybackState.Paused;
            MmResult result;
            lock (this.waveOutLock)
                result = WaveInterop.waveOutPause(this.hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutPause");
        }

        private void Resume() {
            if (this.playbackState != PlaybackState.Paused)
                return;
            MmResult result;
            lock (this.waveOutLock)
                result = WaveInterop.waveOutRestart(this.hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutRestart");
            this.playbackState = PlaybackState.Playing;
        }

        public long GetPosition() => WaveOutUtils.GetPositionBytes(this.hWaveOut, this.waveOutLock);

        public void Dispose() {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        protected void Dispose(bool disposing) {
            this.Stop(false);
            if (disposing)
                this.DisposeBuffers();
            this.CloseWaveOut();
        }

        private void CloseWaveOut() {
            if (this.callbackEvent != null) {
                this.callbackEvent.Close();
                this.callbackEvent = null;
            }

            lock (this.waveOutLock) {
                if (!(this.hWaveOut != IntPtr.Zero))
                    return;
                int num = (int) WaveInterop.waveOutClose(this.hWaveOut);
                this.hWaveOut = IntPtr.Zero;
            }
        }

        private void DisposeBuffers() {
            if (this.buffers == null)
                return;
            foreach (AudioEngineWaveBuffer buffer in this.buffers)
                buffer.Dispose();
            this.buffers = null;
        }
    }
}