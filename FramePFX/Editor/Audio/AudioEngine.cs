using System;
using System.Runtime.InteropServices;
using System.Threading;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editor.Audio {
    public class AudioEngine {
        // public const int TicksPerBar = 192;
        // public const int StepsPerBar = 16;
        // public const int BeatsPerBar = TicksPerBar / StepsPerBar; // 12
        public static readonly Rational AudioPlaybackInterval = new Rational(1, 5);

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
        private AutoResetEvent callbackEvent;

        public int DesiredLatency { get; set; }
        public int NumberOfBuffers { get; set; }
        public int DeviceNumber { get; set; } = -1;

        public AudioEngine() {
            this.sampleRate = 44100;
            this.DesiredLatency = 100;
            this.NumberOfBuffers = 2;
            this.waveOutLock = new object();
        }

        public void UpdateFPS(double fps) {
            this.rawSamplesPerTick = (this.sampleRate / fps);
            this.samplesPerTick = Maths.Ceil((long) Math.Ceiling(this.rawSamplesPerTick), 256);
        }

        // assume:
        //  frame = 30
        //  currentSample = 44144.144144144144144144144144144
        //  sampleOffset = 44.144144144144144144144144144144

        /// <summary>
        /// Generates the sames for a specific frame and then plays them
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="frame"></param>
        public unsafe void ProcessNext(Timeline timeline, long frame) {
            if ((frame - 1) != this.lastFrame) {
                // frame seeked
                this.currentSample = (long) Math.Ceiling(this.lastFrame * this.rawSamplesPerTick);
            }

            // long currSample = (long) Math.Ceiling(frame * this.rawSamplesPerTick);
            // int samples = checked((int) Math.Abs(currSample - this.currentSample));
            int samples = checked((int) (this.sampleRate * (1000 / this.DesiredLatency)));
            if (samples == 0) {
                return;
            }

            int offset = checked((int) (this.currentSample % this.sampleRate));

            // TODO: really improve this LOT

            double** ptr_in = stackalloc double*[2];
            double* ptr_in_l = (double*) Marshal.AllocHGlobal(samples * 8);
            double* ptr_in_r = (double*) Marshal.AllocHGlobal(samples * 8);
            ptr_in[0] = ptr_in_l;
            ptr_in[1] = ptr_in_r;

            double** ptr_out = stackalloc double*[2];
            double* ptr_out_l = (double*) Marshal.AllocHGlobal(samples * 8);
            double* ptr_out_r = (double*) Marshal.AllocHGlobal(samples * 8);
            ptr_out[0] = ptr_out_l;
            ptr_out[1] = ptr_out_r;

            try {
                AudioBusBuffers buffer_l = new AudioBusBuffers {
                    numChannels = 2,
                    channelBuffers64 = &ptr_out_l
                }; // in

                AudioBusBuffers buffer_r = new AudioBusBuffers {
                    numChannels = 2,
                    channelBuffers64 = &ptr_out_r
                }; // in

                AudioProcessData data = new AudioProcessData() {
                    sampleSize = EnumSampleSize.Bit64,
                    numSamples = samples,
                    numInputs = 1,
                    numOutputs = 1,
                    inputs = &buffer_l,
                    outputs = &buffer_r
                };

                foreach (Track track in timeline.Tracks) {
                    if (!(track is AudioTrack audioTrack) || audioTrack.IsMuted || audioTrack.Volume < 0.0001f) {
                        continue;
                    }

                    audioTrack.ProcessAudio(this, ref data, frame);
                }

                // convert samples to bytes
                {
                    byte* p_samp_l = stackalloc byte[samples];
                    for (int i = 0; i < samples; i++)
                        p_samp_l[i] = (byte) (data.outputs->channelBuffers64[0][i] * 255D);
                    // SmoothSamples(p_samp_l, samples);
                    // this.buffers[0].WriteSamplesAndWriteWaveOut(p_samp_l, samples);
                }

                {
                    byte* p_samp_r = stackalloc byte[samples];
                    for (int i = 0; i < samples; i++)
                        p_samp_r[i] = (byte) (data.outputs->channelBuffers64[1][i] * 255D);
                    // SmoothSamples(p_samp_r, samples);
                    // this.buffers[1].WriteSamplesAndWriteWaveOut(p_samp_r, samples);
                }

                this.lastFrame = frame;
            }
            finally {
                Marshal.FreeHGlobal((IntPtr) ptr_in_l);
                Marshal.FreeHGlobal((IntPtr) ptr_in_r);
                Marshal.FreeHGlobal((IntPtr) ptr_out_l);
                Marshal.FreeHGlobal((IntPtr) ptr_out_r);
            }
        }

        public static unsafe void SmoothSamples(byte* data, int count) {
            const int smoothing = 500;
            double value = 0.0d;
            int max = Math.Min(smoothing, count);
            double incr = 1d / max;
            for (int i = 0; i < max; i++) {
                data[i] = (byte) (data[i] * value);
                value += incr;
            }

            value = 0d;
            for (int i = count - 1, begin = count - max; i >= begin; i--) {
                data[i] = (byte) (data[i] * value);
                value += incr;
            }
        }
    }
}