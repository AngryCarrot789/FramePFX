using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FramePFX.Editor.Project;
using FramePFX.Editor.Project.ViewModels;
using FramePFX.Editor.Timeline.New;
using FramePFX.Editor.Timeline.New.Clips;
using FramePFX.Editor.Timeline.ViewModels;
using FramePFX.Editor.Timeline.ViewModels.Clips;
using FramePFX.Render;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Editor {
    public class PFXPlayback {
        public delegate void PlaybackActionEventHandler(PlayAction action);
        public event PlaybackActionEventHandler OnPlayAction;

        private volatile bool isPlaying;
        public bool IsPlaying {
            get => this.isPlaying;
            set => this.isPlaying = value;
        }

        public IViewPort ViewPortHandle { get; set; }

        public PFXVideoEditor Editor { get; }

        public readonly Thread playbackThread;
        public long nextPlaybackTick;
        public long lastPlaybackTick;
        public volatile bool isPlaybackThreadRunning;
        public readonly NumberAverager playbackAverageIntervalMS = new NumberAverager(10);

        public PFXPlayback(PFXVideoEditor editor) {
            this.Editor = editor;
            this.isPlaybackThreadRunning = true;
            this.playbackThread = new Thread(this.PlaybackThreadMain) {
                Name = "ViewPort Playback Thread"
            };
            this.playbackThread.Start();
        }

        public bool IsReadyForRender() {
            return this.ViewPortHandle != null && this.ViewPortHandle.IsReady;
        }

        public void RenderTimeline(PFXTimeline timeline) {
            // Render main view port
            this.RenderTimeline(timeline, timeline.PlayHeadFrame, this.ViewPortHandle);
        }

        public void RenderTimeline(PFXTimeline timeline, long playHead, IViewPort view) {
            if (view != null && view.BeginRender(true)) {
                List<PFXClip> clips = timeline.GetClipsAtFrame().ToList();
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                // TODO: change this to support layer opacity. And also move to shaders because this glVertex3f old stuff it no good
                foreach (PFXClip clip in clips) {
                    if (clip is PFXVideoClip videoClip) {
                        videoClip.Render(view, playHead);
                    }
                    // TODO: add audio... somehow. I have no idea how to do audio lololol
                    // else if (clip is PFXAudioClip audioClip) {
                    //     audioClip.RenderAudioSomehow();
                    // }
                }

                view.FlushFrame();
                view.EndRender();
            }
        }

        private void PlaybackThreadMain() {
            this.lastPlaybackTick = 0;
            this.nextPlaybackTick = 0;
            while (this.isPlaybackThreadRunning) {
                PFXProject project = this.Editor.ActiveProject;
                if (project == null) {
                    Thread.Sleep(100);
                    continue;
                }

                if (this.IsPlaying) {
                    long time = Time.GetSystemMillis();
                    if (time >= this.nextPlaybackTick) { //  || (time + 1) >= this.nextPlaybackTick // time + 1 for ahead of time playback... just in case
                        this.playbackAverageIntervalMS.PushValue(time - this.lastPlaybackTick);
                        this.lastPlaybackTick = time;
                        this.nextPlaybackTick = time + (int) Math.Round(1000.0 / this.Editor.ActiveProject.FrameRate);

                        PFXTimeline timeline = project.Timeline;
                        timeline.StepFrame();
                        this.RenderTimeline(timeline);
                    }

                    // yield results in a generally higher CPU usage due to the fact that
                    // the thread time-splice duration may be in the order of 1s of millis
                    // meaning this function will generally have very high precision and will
                    // absolutely nail the FPS with pinpoint precision
                    Thread.Yield();
                }
                else {
                    Thread.Sleep(20);
                }
            }
        }

        public void Play() {
            this.nextPlaybackTick = Time.GetSystemMillis();
            this.OnPlayAction?.Invoke(PlayAction.Play);
        }

        public void Stop() {
            this.OnPlayAction?.Invoke(PlayAction.Stop);
        }
    }
}