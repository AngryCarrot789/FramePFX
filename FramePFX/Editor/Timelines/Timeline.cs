using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Rendering.ObjectTK;
using FramePFX.Rendering.Utils;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;
using Vector2 = System.Numerics.Vector2;

namespace FramePFX.Editor.Timelines
{
    /// <summary>
    /// A timeline or sequence, which contains a collection of tracks (which contain a collection of clips)
    /// </summary>
    public class Timeline : IAutomatable
    {
        private readonly List<Track> tracks;
        private readonly List<VideoClip> RenderList = new List<VideoClip>();
        private readonly List<AdjustmentVideoClip> AdjustmentStack = new List<AdjustmentVideoClip>();
        private volatile bool isBeginningRender;

        // not a chance anyone's creating more than 9 quintillion clips or tracks
        private long nextClipId;
        private long nextTrackId;

        /// <summary>
        /// The project associated with this timeline. This is set to a non-null value immediately
        /// after creating the project's main timeline. However, for composition timelines, this will
        /// be null until that resource is added to the resource manager hierarchy, and is set to null
        /// when removed from a manager (which typically only happens once)
        /// </summary>
        public Project Project { get; private set; }

        /// <summary>
        /// The current play head
        /// </summary>
        public long PlayHeadFrame { get; set; }

        /// <summary>
        /// This timeline's maximum duration
        /// </summary>
        public long MaxDuration { get; set; }

        public long LargestFrameInUse { get; private set; }

        /// <summary>
        /// A list of tracks that this timeline contains
        /// </summary>
        public IReadOnlyList<Track> Tracks => this.tracks;

        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        public string DisplayName { get; set; }

        public FrameBufferImage FrameBuffer { get; private set; }
        public BasicMesh BasicRectangle { get; set; }
        public Shader BasicShader { get; set; }

        public Timeline()
        {
            this.tracks = new List<Track>();
            this.AutomationData = new AutomationData(this);
        }

        public void UpdateLargestFrame()
        {
            IReadOnlyList<Track> list = this.Tracks;
            int count = list.Count;
            if (count > 0)
            {
                long max = list[0].LargestFrameInUse;
                for (int i = 1; i < count; i++)
                {
                    max = Math.Max(max, list[i].LargestFrameInUse);
                }

                this.LargestFrameInUse = max;
            }
            else
            {
                this.LargestFrameInUse = 0;
            }
        }

        /// <summary>
        /// Sets the project associated with this timeline
        /// <para>
        /// For the project's main timeline, it is called in the project's constructor and therefore only once
        /// </para>
        /// <para>
        /// For composition timelines, this is called when a composition resource's manager changes
        /// </para>
        /// </summary>
        /// <param name="project"></param>
        public void SetProject(Project project)
        {
            Project oldProject = this.Project;
            if (ReferenceEquals(oldProject, project))
                return;

            this.OnProjectChanging(project);
            this.Project = project;
            foreach (Track track in this.Tracks)
            {
                Track.OnTimelineProjectChanged(track, oldProject, project);
            }

            this.OnProjectChanged(oldProject);
        }

        protected virtual void OnProjectChanging(Project newProject)
        {
        }

        protected virtual void OnProjectChanged(Project oldProject)
        {
        }

        public void SetupRenderData()
        {
            this.ClearRenderData();
            this.BasicRectangle = new BasicMesh(new[]
            {
                 1f,  1f, 0f,
                -1f,  1f, 0f,
                -1f, -1f, 0f,
                 1f,  1f, 0f,
                -1f, -1f, 0f,
                 1f, -1f, 0f
            });

            this.BasicShader = new Shader(@"
#version 150

// Globals
uniform mat4 mvp;

// Inputs
in vec3 in_pos;

void main(void) {
	gl_Position = mvp * vec4(in_pos, 1.0);
}
", /* fragment */ @"
#version 150

// Inputs
uniform vec4 in_colour;

void main(void) {
    gl_FragColor = in_colour;
}
");

            Resolution size = this.Project.Settings.Resolution;
            this.FrameBuffer = new FrameBufferImage(size.Width, size.Height);

            foreach (Track track in this.tracks)
            {
                track.SetupRenderData();
            }
        }

        public void ClearRenderData()
        {
            this.BasicRectangle?.Dispose();
            this.BasicRectangle = null;
            this.FrameBuffer?.Dispose();
            this.FrameBuffer = null;
            foreach (Track track in this.tracks)
            {
                track.ClearRenderData();
            }
        }

        public FrameSpan GetUsedFrameSpan()
        {
            return FrameSpan.UnionAll(this.Tracks.SelectMany(x => x.Clips.Select(y => y.FrameSpan)));
        }

        public void GetClipIndicesAt(long frame, ICollection<int> indices)
        {
            foreach (Track track in this.tracks)
            {
                track.GetClipIndicesAt(frame, indices);
            }
        }

        public void WriteToRBE(RBEDictionary data)
        {
            data.SetLong("NextClipId", this.nextClipId);
            data.SetLong("NextTrackId", this.nextTrackId);
            data.SetLong(nameof(this.PlayHeadFrame), this.PlayHeadFrame);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Tracks));
            foreach (Track track in this.tracks)
            {
                if (!(track.FactoryId is string registryId))
                    throw new Exception("Unknown track type: " + track.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(Track.FactoryId), registryId);
                track.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data)
        {
            if ((this.nextClipId = data.GetLong("NextClipId")) < 0)
                throw new Exception("Invalid next clip id");
            if ((this.nextTrackId = data.GetLong("NextTrackId")) < 0)
                throw new Exception("Invalid next track id");
            this.ClearTracks();
            this.PlayHeadFrame = data.GetLong(nameof(this.PlayHeadFrame));
            this.MaxDuration = data.GetLong(nameof(this.MaxDuration));
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Tracks)).Cast<RBEDictionary>())
            {
                string registryId = dictionary.GetString(nameof(Track.FactoryId));
                Track track = TrackFactory.Instance.CreateModel(registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.MaxDuration = Math.Max(this.MaxDuration, this.tracks.Count < 1 ? 0 : this.tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        }

        // true = adding, false = removing, null = moving track from timeline A to B
        private void ValidateTrack(Track track, bool isAdding)
        {
            if (isAdding)
            {
                if (track.Timeline != null)
                {
                    throw new Exception($"Track already belongs to another timeline: '{track.Timeline.DisplayName}'");
                }

                if (this.tracks.Contains(track))
                {
                    throw new Exception($"Track has no timeline associated but is stored in our track list?");
                }
            }
            else if (track.Timeline != this)
            {
                throw new Exception($"Expected track's timeline to equal the current instance, but instead {(track.Timeline == null ? "it's null" : $" equals {track.Timeline.DisplayName}")}");
            }
        }

        public void AddTrack(Track track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, Track track)
        {
            this.ValidateTrack(track, true);
            this.tracks.Insert(index, track);
            Track.SetTimeline(track, this);
            if (this.Project?.IsLoaded ?? false)
            {
                track.SetupRenderData();
            }
        }

        public bool RemoveTrack(Track track)
        {
            int index = this.tracks.IndexOf(track);
            if (index < 0)
                return false;
            this.RemoveTrackAt(index);
            return true;
        }

        public void RemoveTrackAt(int index)
        {
            Track track = this.tracks[index];
            this.ValidateTrack(track, false);
            ExceptionUtils.Assert(track.Timeline == this, "Expected track's timeline and the current timeline instance to be equal");
            if (this.Project?.IsLoaded ?? false)
            {
                track.SetupRenderData();
            }

            this.tracks.RemoveAt(index);
            Track.SetTimeline(track, null);
        }

        public void MoveTrackIndex(int oldIndex, int newIndex) => this.tracks.MoveItem(oldIndex, newIndex);

        public void MoveTrackToTimeline(Track track, Timeline timeline)
        {
            int index = this.tracks.IndexOf(track);
            if (index == -1)
                throw new Exception("Track is not stored in the current instance");
            this.MoveTrackToTimeline(index, timeline);
        }

        public void MoveTrackToTimeline(int index, Timeline timeline)
        {
            Track track = this.tracks[index];
            this.ValidateTrack(track, false);
            if (timeline.tracks.Contains(track))
            {
                throw new Exception("Target timeline is already storing the track?");
            }

            this.tracks.RemoveAt(index);
            timeline.tracks.Insert(index, track);
            Track.SetTimeline(track, timeline);
        }

        public void ClearTracks()
        {
            using (ErrorList list = new ErrorList())
            {
                for (int i = this.tracks.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        this.tracks[i].Clear();
                    }
                    catch (Exception e)
                    {
                        list.Add(e);
                    }

                    try
                    {
                        this.RemoveTrackAt(i);
                    }
                    catch (Exception e)
                    {
                        list.Add(e);
                    }
                }
            }
        }

        // I did some testing and found that invoking methods declared with async is generally
        // about 10x slower than regular method invocation, when in debug mode (which uses
        // class-based async state machines, AFAIK)

        // And most rendering is probably fully syncrhronous, meaning async is not required (they
        // can return Task.CompletedTask), which means that async probably won't affect the render
        // performance all that much

        // Anyway these methods are really messy but a lot of it is just avoiding the usage of enumerators
        // in order to re-use local variables... because I like fast apps ;-)
        public async Task RenderAsync(RenderContext render, long frame, CancellationToken token)
        {
            if (!this.BeginCompositeRender(frame, token))
                throw new TaskCanceledException("Begin render took too long to complete");
            await this.EndCompositeRenderAsync(render, frame, token);
        }

        private void CompleteRenderList(long frame, bool isCancelled, int i = 0)
        {
            using (ErrorList list = new ErrorList("Failed to " + (isCancelled ? "cancel" : "finalize") + " one or more clip renders"))
            {
                List<VideoClip> renderList = this.RenderList;
                for (int count = renderList.Count; i < count; i++)
                {
                    try
                    {
                        renderList[i].OnRenderCompleted(frame, isCancelled);
                    }
                    catch (Exception e)
                    {
                        list.Add(e);
                    }
                }

                renderList.Clear();
                this.AdjustmentStack.Clear();
            }
        }

        private void CompleteRenderList(long frame, bool isCancelled, Exception e, int i = 0)
        {
            try
            {
                this.CompleteRenderList(frame, isCancelled, i);
            }
            catch (Exception ex)
            {
                e.AddSuppressed(ex);
            }
        }

        public bool BeginCompositeRender(long frame, CancellationToken token)
        {
            if (this.isBeginningRender)
                throw new Exception("Already rendering. Possible asynchronous render");
            if (this.RenderList.Count > 0)
                throw new Exception("Render queue already loaded. Possible asynchronous render or " + nameof(this.EndCompositeRenderAsync) + " was not called");
            try
            {
                this.isBeginningRender = true;
                return this.BeginCompositeRenderInternal(frame, token);
            }
            finally
            {
                this.isBeginningRender = false;
            }
        }

        private bool BeginCompositeRenderInternal(long frame, CancellationToken token)
        {
            List<AdjustmentVideoClip> adjustments = this.AdjustmentStack;
            List<VideoClip> renderList = this.RenderList;
            List<Track> trackList = this.tracks;
            // Render timeline from the bottom to the top
            for (int i = trackList.Count - 1; i >= 0; i--)
            {
                if (token.IsCancellationRequested)
                {
                    // BeginRender phase must have taken too long; call cancel to all clips that were prepared
                    this.CompleteRenderList(frame, true);
                    return false;
                }

                if (trackList[i] is VideoTrack track && track.IsActuallyVisible)
                {
                    Clip clip = track.GetClipAtFrame(frame);
                    if (clip == null)
                    {
                        continue;
                    }

                    if (clip is AdjustmentVideoClip)
                    {
                        adjustments.Add((AdjustmentVideoClip) clip);
                    }
                    else
                    {
                        bool render;
                        try
                        {
                            render = ((VideoClip) clip).OnBeginRender(frame);
                        }
                        catch (Exception e)
                        {
                            this.CompleteRenderList(frame, true, e);
                            throw new Exception("Failed to invoke " + nameof(VideoClip.OnBeginRender) + " for clip", e);
                        }

                        if (render)
                        {
                            renderList.Add((VideoClip) clip);
                        }
                    }
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PreProcessAjustments(long frame, RenderContext render)
        {
            List<AdjustmentVideoClip> list = this.AdjustmentStack;
            int count = list.Count;
            if (count == 0)
                return;

            Vector2 size = render.FrameSize;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    BaseEffect.ProcessEffectList(list[i].Effects, frame, render, size, true);
                }
            }
            catch (Exception e)
            {
                this.CompleteRenderList(frame, true, e);
                throw new Exception("Failed to pre-process adjustment layer effects", e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PostProcessAjustments(long frame, RenderContext render)
        {
            List<AdjustmentVideoClip> list = this.AdjustmentStack;
            int count = list.Count;
            if (count == 0)
                return;

            Vector2 size = render.FrameSize;
            try
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    BaseEffect.ProcessEffectList(list[i].Effects, frame, render, size, false);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to post-process adjustment layer effects", e);
            }
        }

        public async Task EndCompositeRenderAsync(RenderContext render, long frame, CancellationToken token)
        {
            List<VideoClip> renderList = this.RenderList;
            render.PushFrameBuffer(this.FrameBuffer.FrameBufferId, FramebufferTarget.Framebuffer);
            int fbtxid = this.FrameBuffer.TextureId;
            render.ActiveFrameBufferTexture = fbtxid;
            this.FrameBuffer.Clear();

            // TODO: clipping
            render.MatrixStack.PushMatrix();
            render.MatrixStack.Matrix = Matrix4x4.Identity;
            this.PreProcessAjustments(frame, render);
            int i = 0, count = renderList.Count;
            try
            {
                for (; i < count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        // Rendering took too long. Some clips may have already drawn. Cancel the rest
                        this.CompleteRenderList(frame, true, i);
                        throw new TaskCanceledException();
                    }

                    VideoClip clip = renderList[i];
                    VideoTrack track = clip.Track;

                    // TODO: track and clip opacity
                    render.PushFrameBuffer(track.FrameBuffer.FrameBufferId, FramebufferTarget.Framebuffer);
                    render.ActiveFrameBufferTexture = track.FrameBuffer.TextureId;
                    track.FrameBuffer.Clear();

                    Vector2? frameSize = clip.GetSize(render);

                    try
                    {
                        BaseEffect.ProcessEffectList(clip.Effects, frame, render, frameSize, true);
                    }
                    catch (Exception e)
                    {
                        this.CompleteRenderList(frame, true, e, i);
                        RenderContext.TryPopFrameBuffer(render);
                        throw new RenderException("Failed to pre-process effects", e);
                    }

                    try
                    {
                        // actual render the clip
                        Task task = clip.OnEndRender(render, frame);
                        if (!task.IsCompleted)
                        {
                            // possibly help with performance a tiny bit
                            await task;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // do nothing
                    }
                    catch (Exception e)
                    {
                        this.CompleteRenderList(frame, true, e, i);
                        RenderContext.TryPopFrameBuffer(render);
                        throw new RenderException($"Failed to render '{clip}'", e);
                    }

                    try
                    {
                        BaseEffect.ProcessEffectList(clip.Effects, frame, render, frameSize, false);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            clip.OnRenderCompleted(frame, false);
                        }
                        catch (Exception ex)
                        {
                            e.AddSuppressed(new Exception("Failed to finalize clip just after post effect error", ex));
                        }

                        this.CompleteRenderList(frame, true, e, i + 1);
                        RenderContext.TryPopFrameBuffer(render);
                        throw new RenderException("Failed to post-process effects", e);
                    }

                    try
                    {
                        clip.OnRenderCompleted(frame, false);
                    }
                    catch (Exception e)
                    {
                        this.CompleteRenderList(frame, true, e, i + 1);
                        RenderContext.TryPopFrameBuffer(render);
                        throw new RenderException($"Failed to call {nameof(clip.OnRenderCompleted)} for '{clip}'", e);
                    }

                    render.PopFrameBuffer(null);
                    render.ActiveFrameBufferTexture = fbtxid;
                    track.FrameBuffer.DrawIntoTargetBuffer(render.ActiveFrameBuffer, ref render.MatrixStack.Matrix);
                }
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (RenderException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.CompleteRenderList(frame, true, e, i + 1);
                throw new Exception("Unexpected exception occurred during render", e);
            }
            finally
            {
                RenderContext.TryPopFrameBuffer(render);
                render.ActiveFrameBufferTexture = 0;
            }

            this.AdjustmentStack.Clear();
            this.RenderList.Clear();
            this.PostProcessAjustments(frame, render);

            // TODO: restore clipping

            render.MatrixStack.PopMatrix();

            {
                Resolution res = this.Project.Settings.Resolution;
                Matrix4x4 matrix = Matrix4x4.CreateScale(res.Width / 2.0f, res.Height / 2.0f, 1f) * render.MatrixStack.Matrix;
                Matrix4x4 mvp = matrix * render.Projection;
                this.FrameBuffer.DrawIntoTargetBuffer(render.ActiveFrameBuffer, ref mvp);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} ({this.Tracks.Count.ToString()} tracks, {this.tracks.Sum(x => x.Clips.Count).ToString()} total clips)";
        }
    }
}