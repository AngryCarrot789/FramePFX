using System;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using SoundIOSharp;

namespace FramePFX.Editors.Timelines.Tracks {
    public class AudioTrack : Track {
        public override bool IsClipTypeAccepted(Type type) {
            return typeof(AudioClip).IsAssignableFrom(type);
        }

        public override bool IsEffectTypeAccepted(Type effectType) {
            return false;
        }

        public static void PlaySineWave() {
            SoundIO api = new SoundIO();
            api.ConnectBackend(SoundIOBackend.Wasapi);
            api.FlushEvents();

            SoundIODevice device = api.GetOutputDevice(api.DefaultOutputDeviceIndex);
            if (device == null) {
                return;
            }

            if (device.ProbeError != 0) {
                return;
            }

            SoundIOOutStream outstream = device.CreateOutStream();
            outstream.WriteCallback = (min, max) => write_callback(outstream, min, max);
            outstream.UnderflowCallback = () => underflow_callback(outstream);
            outstream.SampleRate = 4096;
            if (device.SupportsFormat(SoundIODevice.Float32NE)) {
                outstream.Format = SoundIODevice.Float32NE;
                write_sample = write_sample_float32ne;
            }
            else if (device.SupportsFormat(SoundIODevice.Float64NE)) {
                outstream.Format = SoundIODevice.Float64NE;
                write_sample = write_sample_float64ne;
            }
            else if (device.SupportsFormat(SoundIODevice.S32NE)) {
                outstream.Format = SoundIODevice.S32NE;
                write_sample = write_sample_s32ne;
            }
            else if (device.SupportsFormat(SoundIODevice.S16NE)) {
                outstream.Format = SoundIODevice.S16NE;
                write_sample = write_sample_s16ne;
            }
            else {
                return;
            }

            outstream.Open();

            outstream.Start();

            for (;;) {
                api.FlushEvents();
            }

            outstream.Dispose();
            device.RemoveReference();
            api.Dispose();
        }

        private static Action<IntPtr, double> write_sample;
        private static double seconds_offset = 0.0;
        private static volatile bool want_pause = false;

        private static void write_callback(SoundIOOutStream outstream, int frame_count_min, int frame_count_max) {
            double dSampleRate = outstream.SampleRate;
            double dSecondsPerFrame = 1.0 / dSampleRate;

            int framesLeft = frame_count_max;
            for (;;) {
                int frameCount = framesLeft;
                SoundIOChannelAreas results = outstream.BeginWrite(ref frameCount);

                if (frameCount == 0)
                    break;

                SoundIOChannelLayout layout = outstream.Layout;

                double pitch = 440.0;
                double radians_per_second = pitch * 2.0 * Math.PI;
                for (int frame = 0; frame < frameCount; frame += 1) {
                    double sample = Math.Sin((seconds_offset + frame * dSecondsPerFrame) * radians_per_second);
                    for (int channel = 0; channel < layout.ChannelCount; channel += 1) {
                        SoundIOChannelArea area = results.GetArea(channel);
                        write_sample(area.Pointer, sample);
                        area.Pointer += area.Step;
                    }
                }

                seconds_offset = Math.IEEERemainder(seconds_offset + dSecondsPerFrame * frameCount, 1.0);

                outstream.EndWrite();

                framesLeft -= frameCount;
                if (framesLeft <= 0)
                    break;
            }

            outstream.Pause(want_pause);
        }

        private static int underflow_callback_count = 0;

        private static void underflow_callback(SoundIOOutStream outstream) {
            Console.Error.WriteLine("underflow {0}", underflow_callback_count++);
        }

        private static unsafe void write_sample_s16ne(IntPtr ptr, double sample) {
            short* buf = (short*) ptr;
            double range = (double) short.MaxValue - (double) short.MinValue;
            double val = sample * range / 2.0;
            *buf = (short) val;
        }

        private static unsafe void write_sample_s32ne(IntPtr ptr, double sample) {
            int* buf = (int*) ptr;
            double range = (double) int.MaxValue - (double) int.MinValue;
            double val = sample * range / 2.0;
            *buf = (int) val;
        }

        private static unsafe void write_sample_float32ne(IntPtr ptr, double sample) {
            float* buf = (float*) ptr;
            *buf = (float) sample;
        }

        private static unsafe void write_sample_float64ne(IntPtr ptr, double sample) {
            double* buf = (double*) ptr;
            *buf = sample;
        }

        public bool PrepareRenderFrame(long frame, long samples, EnumRenderQuality quality) {
            return false;
        }

        public void RenderAudioFrame(long samples, EnumRenderQuality quality) {

        }

        public byte[] GetAudioSamples() {
            return null;
        }
    }
}