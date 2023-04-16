using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Project;
using FramePFX.Render;
using FramePFX.Timeline.ViewModels;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;

namespace FramePFX {
    public class ViewportPlayback : BaseViewModel {
        private volatile bool isPlaying;
        public bool IsPlaying {
            get => this.isPlaying;
            set {
                this.isPlaying = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand PlayPauseCommand { get; set; }

        public RelayCommand PlayCommand { get; set; }

        public RelayCommand PauseCommand { get; set; }

        /// <summary>
        /// A handle to the main view port
        /// </summary>
        public IViewPort ViewPortHandle { get; set; }

        public VideoEditor Editor { get; }

        private readonly Thread playbackThread;
        private long nextPlaybackTick;
        private long lastPlaybackTick;
        public volatile bool isPlaybackThreadRunning;
        public readonly NumberAverager playbackAverageIntervalMS = new NumberAverager(10);

        public ViewportPlayback(VideoEditor editor) {
            this.Editor = editor;
            this.PlayPauseCommand = new RelayCommand(() => {
                if (this.IsPlaying) {
                    this.PauseAction();
                }
                else {
                    this.PlayAction();
                }
            });

            this.PlayCommand = new RelayCommand(this.PlayAction, () => !this.IsPlaying);
            this.PauseCommand = new RelayCommand(this.PauseAction, () => this.IsPlaying);
            this.isPlaybackThreadRunning = true;
            // using a DispatcherTimer instead of a Thread will not make anything better
            this.playbackThread = new Thread(this.PlaybackThreadMain) {
                Name = "ViewPort Playback Thread"
            };
            this.playbackThread.Start();
        }

        public bool IsReadyForRender() {
            return this.ViewPortHandle != null && this.ViewPortHandle.IsReady;
        }

        public void RenderTimeline(EditorTimeline timeline) {
            // Render main view port
            this.RenderTimeline(timeline, timeline.PlayHeadFrame, this.ViewPortHandle);
        }

        public void RenderTimeline(EditorTimeline timeline, long playHead, IViewPort view) {
            if (view != null && view.BeginRender(true)) {
                List<TimelineVideoClip> clips = timeline.GetVideoClipsIntersectingFrame().ToList();
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                // TODO: change this to support layer opacity. And also move to shaders because this glVertex3f old stuff it no good
                foreach (TimelineVideoClip clip in clips) {
                    clip.Render(view, playHead);
                    // TODO: add audio... somehow. I have no idea how to do audio lololol
                    // else if (clip.Content is IAudioClip audioClip) {
                    //     audioClip.RenderAudioSomehow();
                    // }
                }

                view.FlushFrame();
                view.EndRender();
            }
        }

        // TODO: Maybe move this into a non-viewmodel, so that it's more MVVM-ey?
        private void PlaybackThreadMain() {
            this.lastPlaybackTick = 0;
            this.nextPlaybackTick = 0;
            while (this.isPlaybackThreadRunning) {
                EditorProject project;
                EditorTimeline timeline;
                if ((project = IoC.ActiveProject) == null || (timeline = project.Timeline) == null) {
                    Thread.Sleep(100);
                    continue;
                }

                if (this.IsPlaying) {
                    long time = Time.GetSystemMillis();
                    if (time >= this.nextPlaybackTick) { //  || (time + 1) >= this.nextPlaybackTick // time + 1 for ahead of time playback... just in case
                        this.playbackAverageIntervalMS.PushValue(time - this.lastPlaybackTick);
                        this.lastPlaybackTick = time;
                        this.nextPlaybackTick = time + 33L; // 33ms = 30fps
                        timeline.StepFrame();
                    }

                    if (timeline.IsRenderDirty) {
                        this.RenderTimeline(timeline);
                    }

                    // yield results in a generally higher CPU usage due to the fact that
                    // the thread time-splice duration may be in the order of 1s of millis
                    // meaning this function will generally have very high precision and will
                    // absolutely nail the FPS with pinpoint precision
                    Thread.Yield();
                }
                else if (timeline.isFramePropertyChangeScheduled) { // just in case...
                    timeline.RaisePropertyChanged(nameof(timeline.PlayHeadFrame));
                    timeline.isFramePropertyChangeScheduled = false;
                }
                else {
                    Thread.Sleep(20);
                }
            }
        }

        public void PlayAction() {
            if (this.IsPlaying) {
                return;
            }

            this.nextPlaybackTick = Time.GetSystemMillis();
            this.IsPlaying = true;
            this.PlayCommand.RaiseCanExecuteChanged();
            this.PauseCommand.RaiseCanExecuteChanged();
            EditorProject project = this.Editor.ActiveProject;
            if (project != null) {
                project.OnPlayBegin();
            }
        }

        public void PauseAction() {
            if (!this.IsPlaying) {
                return;
            }

            this.IsPlaying = false;
            this.PlayCommand.RaiseCanExecuteChanged();
            this.PauseCommand.RaiseCanExecuteChanged();
            EditorProject project = this.Editor.ActiveProject;
            if (project != null) {
                project.OnPlayEnd();
            }
        }
    }
}
