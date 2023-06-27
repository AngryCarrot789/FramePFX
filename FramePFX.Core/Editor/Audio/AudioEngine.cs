using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Tracks;
using FramePFX.Core.Utils;
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

        private int sampleRate = 44100;
        private double samplesPerTick;
        private long lastFrame;


        public void UpdateFPS(double fps) {
            this.samplesPerTick = this.sampleRate / fps;
        }

        // assume:
        //  frame = 30
        //  currentSample = 44144.144144144144144144144144144
        //  sampleOffset = 44.144144144144144144144144144144

        public void OnTick(TimelineModel timeline, long frame) {
            if ((frame - 1) != this.lastFrame) { // frame seeked

            }

            double currentSample = this.samplesPerTick * frame;
            double sampleOffset = currentSample % this.sampleRate;
            int offset = checked((int) Math.Floor(sampleOffset));
            int length = checked((int) (sampleOffset - (long) sampleOffset >= 0.5 ? Math.Ceiling(this.samplesPerTick) : Math.Floor(this.samplesPerTick)));

            foreach (TrackModel track in timeline.Tracks) {
                if (!(track is AudioTrackModel audioTrack) || audioTrack.IsMuted || audioTrack.Volume < 0.0001f) {
                    continue;
                }

                audioTrack.ProcessAudio(this, frame, offset, length);
            }

            this.lastFrame = frame;
        }
    }
}