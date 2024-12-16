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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using FramePFX.AdvancedMenuService;
using FramePFX.DataTransfer;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Factories;
using FramePFX.Editing.ResourceManaging.NewResourceHelper;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Serialisation;
using FramePFX.Utils.Destroying;
using FramePFX.Utils.RBC;

namespace FramePFX.Editing.Timelines.Clips;

public delegate void ClipEventHandler(Clip clip);

public delegate void ClipSpanChangedEventHandler(Clip clip, FrameSpan oldSpan, FrameSpan newSpan);

public delegate void ClipMediaOffsetChangedEventHandler(Clip clip, long oldOffset, long newOffset);

public delegate void ClipTrackChangedEventHandler(Clip clip, Track? oldTrack, Track? newTrack);

public delegate void ClipActiveSequenceChangedEventHandler(Clip clip, AutomationSequence? oldSequence, AutomationSequence? newSequence);

public abstract class Clip : IClip, IDestroy
{
    public static readonly ContextRegistry ClipContextRegistry = new ContextRegistry("Clips");

    public static readonly SerialisationRegistry SerialisationRegistry;
    private readonly List<BaseEffect> internalEffectList;
    private FrameSpan span;
    private string? displayName;
    private AutomationSequence? activeSequence;
    private long mediaFrameOffset;

    private ClipGroup? myGroup; // TODO

    public Track? Track { get; private set; }

    public Timeline? Timeline { get; private set; }

    public Project? Project { get; private set; }

    public IReadOnlyList<BaseEffect> Effects => this.internalEffectList;

    public TransferableData TransferableData { get; }

    public AutomationData AutomationData { get; }

    public FrameSpan FrameSpan
    {
        get => this.span;
        set
        {
            FrameSpan oldSpan = this.span;
            if (oldSpan == value)
                return;
            if (value.Begin < 0 || value.Duration < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Span contained negative values");
            if (value.Duration < 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Span duration cannot be zero");
            
            this.span = value;
            
            Track.InternalOnClipSpanChanged(this, oldSpan);
            this.OnFrameSpanChanged(oldSpan, value);
        }
    }

    /// <summary>
    /// A frame offset (relative to the project FPS) that is how many frames ahead or behind this clip's media begins.
    /// This is changed when a clip's left grip is dragged. This is negative when the media starts before the clip
    /// starts, and is positive when the media begins after the clip starts.
    /// <para>
    /// This value is only modified by the UI drag actions when <see cref="IsMediaFrameSensitive"/> is true
    /// </para>
    /// <para>
    /// When dragging the left grip, it is calculated as: <code>MediaFrameOffset += (oldSpan.Begin - newSpan.Begin)</code>
    /// </para>
    /// </summary>
    public long MediaFrameOffset
    {
        get => this.mediaFrameOffset;
        set
        {
            long oldValue = this.mediaFrameOffset;
            if (value == oldValue)
                return;
            this.mediaFrameOffset = value;
            this.MediaFrameOffsetChanged?.Invoke(this, oldValue, value);
            this.MarkProjectModified();
        }
    }

    public string? DisplayName
    {
        get => this.displayName;
        set
        {
            string? oldValue = this.displayName;
            if (oldValue == value)
                return;
            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this, oldValue, value);
            this.MarkProjectModified();
        }
    }

    /// <summary>
    /// Gets or sets a value which indicates that this clip is sensitive to the <see cref="MediaFrameOffset"/> value. False by default,
    /// meaning <see cref="MediaFrameOffset"/> is ignored by this clip. True for things like audio clips and AVMedia clips
    /// </summary>
    public bool IsMediaFrameSensitive { get; protected set; }

    /// <summary>
    /// Stores the sequence that this clip's automation sequence editor is using. This is only really used for the UI
    /// </summary>
    public AutomationSequence? ActiveSequence
    {
        get => this.activeSequence;
        set
        {
            AutomationSequence? oldSequence = this.activeSequence;
            if (oldSequence == value)
                return;
            this.activeSequence = value;
            this.ActiveSequenceChanged?.Invoke(this, oldSequence, value);
        }
    }

    public ResourceHelper ResourceHelper { get; }

    public string FactoryId => ClipFactory.Instance.GetId(this.GetType());

    public event EffectOwnerEventHandler? EffectAdded;
    public event EffectOwnerEventHandler? EffectRemoved;
    public event EffectMovedEventHandler? EffectMoved;
    public event ClipSpanChangedEventHandler? FrameSpanChanged;
    public event DisplayNameChangedEventHandler? DisplayNameChanged;
    public event ClipMediaOffsetChangedEventHandler? MediaFrameOffsetChanged;
    public event ClipTrackChangedEventHandler? TrackChanged;
    public event TimelineChangedEventHandler? TimelineChanged;

    /// <summary>
    /// An event fired when this clip's automation sequence editor's sequence changes. The new sequence
    /// may not directly belong to the clip, but may belong to an effect added to the clip
    /// </summary>
    public event ClipActiveSequenceChangedEventHandler? ActiveSequenceChanged;

    protected Clip()
    {
        this.internalEffectList = new List<BaseEffect>();
        this.ResourceHelper = new ResourceHelper(this);
        this.AutomationData = new AutomationData(this);
        this.TransferableData = new TransferableData(this);
    }

    // We don't want any deriving object to try and change the equality behaviour.
    // Clips should only be reference comparable
    
    public sealed override bool Equals(object? obj) => this == obj;

    public sealed override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    static Clip()
    {
        SerialisationRegistry = new SerialisationRegistry();

        // Is this even a good system? Minor updates could be handled in one of these too i suppose...
        // Build Version 0 is the absolute lowest version the app can be in. If there's a new feature added,
        // that obviously means the app build number is now higher and so that version should be used to register
        // a new serialiser/deserialiser that also calls the previous version (or does a complete rewrite, if necessary)
        SerialisationRegistry.Register<Clip>(0, (clip, data, ctx) =>
        {
            clip.displayName = data.GetString(nameof(clip.DisplayName), null);
            clip.FrameSpan = data.GetStruct<FrameSpan>(nameof(clip.FrameSpan));
            clip.MediaFrameOffset = data.GetLong(nameof(clip.MediaFrameOffset));
            // clip.IsRenderingEnabled = data.GetBool(nameof(clip.IsRenderingEnabled), true);
            clip.AutomationData.ReadFromRBE(data.GetDictionary(nameof(clip.AutomationData)));
            BaseEffect.ReadSerialisedWithIdList(clip, data.GetList("Effects"));
            clip.ResourceHelper.ReadFromRootRBE(data);
        }, (clip, data, ctx) =>
        {
            if (!string.IsNullOrEmpty(clip.displayName))
                data.SetString(nameof(clip.DisplayName), clip.displayName);
            data.SetStruct(nameof(clip.FrameSpan), clip.FrameSpan);
            data.SetLong(nameof(clip.MediaFrameOffset), clip.MediaFrameOffset);
            // data.SetBool(nameof(clip.IsRenderingEnabled), clip.IsRenderingEnabled);
            clip.AutomationData.WriteToRBE(data.CreateDictionary(nameof(clip.AutomationData)));
            BaseEffect.WriteSerialisedWithIdList(clip, data.CreateList("Effects"));
            clip.ResourceHelper.WriteToRootRBE(data);
        });

        FixedContextGroup modGeneric = ClipContextRegistry.GetFixedGroup("modify.general");
        modGeneric.AddHeader("General");
        modGeneric.AddCommand("commands.editor.RenameClip", "Rename", "Open a dialog to rename this clip");
        modGeneric.AddDynamicSubGroup((group, ctx, items) =>
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip? clip) && clip is VideoClip videoClip)
            {
                if (VideoClip.IsEnabledParameter.GetCurrentValue(videoClip))
                {
                    items.Add(new CommandContextEntry("commands.editor.DisableClips", "Disable", "Disable this clip"));
                }
                else
                {
                    items.Add(new CommandContextEntry("commands.editor.EnableClips", "Enable", "Enable this clip"));
                }
            }
            else
            {
                items.Add(new CommandContextEntry("commands.editor.EnableClips", "Enable", "Enable the selected clips"));
                items.Add(new CommandContextEntry("commands.editor.DisableClips", "Disable", "Disable the selected clips"));
                items.Add(new CommandContextEntry("commands.editor.ToggleClipsEnabled", "Toggle Enabled", "Toggle the enabled state of the selected clips"));
            }
        });

        FixedContextGroup modEdit = ClipContextRegistry.GetFixedGroup("modify.edit");
        modEdit.AddHeader("Edit");
        modEdit.AddCommand("commands.editor.SplitClipsCommand", "Split", "Slice this clip at the playhead");
        modEdit.AddCommand("commands.editor.ChangeClipPlaybackSpeed", "Change Speed", "Change the playback speed of this clip");
        modEdit.AddDynamicSubGroup((group, ctx, items) =>
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip? clip) && clip is CompositionVideoClip)
            {
                items.Add(new CommandContextEntry("commands.editor.OpenCompositionClipTimeline", "Open Timeline", "Opens this clip's timeline"));
            }
        });

        FixedContextGroup modDestruction = ClipContextRegistry.GetFixedGroup("modify.destruction", 100000);
        modDestruction.AddCommand("commands.editor.DeleteClipOwnerTrack", "Delete Track", "Delete the track this clip resides in");

        // Example new serialisers for new feature added in new build version
        // SerialisationRegistry.Register<Clip>(1, (clip, data, ctx) => {
        //     ctx.DeserialiseLastVersion(clip, data);
        //     clip.SpecialProperty = ...
        // }, (clip, data, ctx) => {
        //     ctx.SerialiseLastVersion(clip, data);
        //     ... = clip.SpecialProperty;
        // });
    }

    /// <summary>
    /// Marks our project (if available) as modified
    /// </summary>
    public void MarkProjectModified() => this.Project?.MarkModified();

    protected virtual void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan)
    {
        this.FrameSpanChanged?.Invoke(this, oldSpan, newSpan);
        this.MarkProjectModified();
        if (this.GetRelativePlayHead(out long relativeFrame))
            AutomationEngine.UpdateValues(this, relativeFrame);
    }

    public bool GetRelativePlayHead(out long playHead)
    {
        playHead = this.ConvertTimelineToRelativeFrame(this.Timeline?.PlayHeadPosition ?? this.span.Begin, out bool isInRange);
        return isInRange;
    }

    public Clip Clone() => this.Clone(ClipCloneOptions.Default);

    public Clip Clone(ClipCloneOptions options)
    {
        string id = this.FactoryId;
        Clip clone = ClipFactory.Instance.NewClip(id);
        if (clone.GetType() != this.GetType())
            throw new Exception("Cloned object type does not match the item type");

        this.LoadDataIntoClone(clone, options);
        if (options.CloneEffects)
        {
            foreach (BaseEffect effect in this.Effects)
            {
                if (effect.IsCloneable)
                    clone.AddEffect(effect.Clone());
            }
        }

        if (options.CloneAutomationData)
            this.AutomationData.LoadDataIntoClone(clone.AutomationData);

        if (options.CloneResourceLinks)
            this.ResourceHelper.LoadDataIntoClone(clone.ResourceHelper);

        return clone;
    }

    public static void WriteSerialisedWithId(RBEDictionary dictionary, Clip clip)
    {
        if (!(clip.FactoryId is string id))
            throw new Exception("Unknown clip type: " + clip.GetType());
        dictionary.SetString(nameof(FactoryId), id);
        SerialisationRegistry.Serialise(clip, dictionary.CreateDictionary("Data"));
    }

    public static Clip ReadSerialisedWithId(RBEDictionary dictionary)
    {
        string? id = dictionary.GetString(nameof(FactoryId));
        Clip clip = ClipFactory.Instance.NewClip(id);
        SerialisationRegistry.Deserialise(clip, dictionary.GetDictionary("Data"));
        return clip;
    }

    protected virtual void LoadDataIntoClone(Clip clone, ClipCloneOptions options)
    {
        clone.span = this.span;
        clone.displayName = this.displayName;
        clone.mediaFrameOffset = this.mediaFrameOffset;
        clone.span = this.span;
        // other cloneable objects are processed in the main Clone method
    }

    public void MoveToTrack(Track dstTrack)
    {
        if (ReferenceEquals(this.Track, dstTrack))
            return;
        this.MoveToTrack(dstTrack, dstTrack.Clips.Count);
    }

    public void MoveToTrack(Track dstTrack, int dstIndex)
    {
        if (this.Track == null)
        {
            dstTrack.InsertClip(dstIndex, this);
            return;
        }

        int index = this.Track.Clips.IndexOf(this);
        if (index == -1)
        {
            throw new Exception("Fatal error: clip did not exist in its owner track");
        }

        this.Track.MoveClipToTrack(index, dstTrack, dstIndex);
    }

    /// <summary>
    /// Invoked when this clip's track changes. The cause of this is either the clip being added to, removed
    /// from or moved between tracks. This method calls <see cref="OnProjectChanged"/> if possible. The
    /// old and new tracks will not match. This method must be called by overriders, as this method
    /// updates the resource helper, fires appropriate events, etc.
    /// </summary>
    /// <param name="oldTrack">The previous track</param>
    /// <param name="newTrack">The new track</param>
    protected virtual void OnTrackChanged(Track? oldTrack, Track? newTrack)
    {
        // Debug.WriteLine("Clip's track changed: " + oldTrack + " -> " + newTrack);
        this.TrackChanged?.Invoke(this, oldTrack, newTrack);
        Timeline? oldTimeline = oldTrack?.Timeline;
        Timeline? newTimeline = newTrack?.Timeline;
        if (!ReferenceEquals(oldTimeline, newTimeline))
        {
            this.Timeline = newTimeline;
            this.OnTimelineChanged(oldTimeline, newTimeline);
        }
    }

    protected virtual void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline)
    {
        // Debug.WriteLine("Clip's timeline changed: " + oldTimeline + " -> " + newTimeline);
        this.TimelineChanged?.Invoke(this, oldTimeline, newTimeline);
        Project? oldProject = oldTimeline?.Project;
        Project? newProject = newTimeline?.Project;
        if (!ReferenceEquals(oldProject, newProject))
        {
            this.Project = newProject;
            this.OnProjectChanged(oldProject, newProject);
        }

        foreach (BaseEffect effect in this.Effects)
        {
            BaseEffect.OnClipTimelineChanged(effect, oldTimeline, newTimeline);
        }
    }

    /// <summary>
    /// Invoked when this clip's project changes. The cause of this can be many, such as a clip being added to or
    /// removed from a track, our track being added to a timeline, our track's timeline's project changing (only
    /// possible with composition timelines), and possibly other causes maybe. The old and new project will not match
    /// </summary>
    /// <param name="oldProject">The previous project</param>
    /// <param name="newProject">The new project</param>
    protected virtual void OnProjectChanged(Project? oldProject, Project? newProject)
    {
        // Debug.WriteLine("Clip's project changed: " + oldProject + " -> " + newProject);
        this.ResourceHelper.OnResourceManagerChanged(newProject?.ResourceManager);
    }

    public bool IntersectsFrameAt(long playHead)
    {
        return this.span.Intersects(playHead);
    }

    /// <summary>
    /// Shrinks this clips and creates a clone in front of this clip, effectively "splitting" this clip into 2
    /// </summary>
    /// <param name="offset">The frame to split this clip at, relative to this clip</param>
    public void CutAt(long offset)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");
        if (offset == 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be zero");
        if (offset >= this.span.Duration)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot exceed our span's range");
        if (this.Track == null)
            throw new InvalidOperationException("Clip is not in a track");
        
        long begin = this.span.Begin;
        FrameSpan spanLeft = FrameSpan.FromIndex(begin, begin + offset);
        FrameSpan spanRight = FrameSpan.FromIndex(spanLeft.EndIndex, this.span.EndIndex);

        Clip clone = this.Clone();
        this.Track.AddClip(clone);

        this.FrameSpan = spanLeft;
        clone.FrameSpan = spanRight;
        if (clone.IsMediaFrameSensitive)
            clone.MediaFrameOffset -= offset;
    }

    public void Duplicate() {
    }

    public long ConvertRelativeToTimelineFrame(long relative) => this.span.Begin + relative;

    public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange)
    {
        long frame = timeline - this.span.Begin;
        inRange = frame >= 0 && frame < this.span.Duration;
        return frame;
    }

    public bool IsTimelineFrameInRange(long timeline)
    {
        long frame = timeline - this.span.Begin;
        return frame >= 0 && frame < this.span.Duration;
    }

    public bool IsRelativeFrameInRange(long relative)
    {
        return relative >= 0 && relative < this.span.Duration;
    }

    public bool IsAutomated(Parameter parameter)
    {
        return this.AutomationData.IsAutomated(parameter);
    }

    public abstract bool IsEffectTypeAccepted(Type effectType);

    public void AddEffect(BaseEffect effect)
    {
        this.InsertEffect(this.internalEffectList.Count, effect);
    }

    public void InsertEffect(int index, BaseEffect effect)
    {
        BaseEffect.ValidateInsertEffect(this, effect, index);
        this.internalEffectList.Insert(index, effect);
        BaseEffect.OnAddedInternal(this, effect);
        this.OnEffectAdded(index, effect);
    }

    public bool RemoveEffect(BaseEffect effect)
    {
        if (effect.Owner != this)
            return false;

        int index = this.internalEffectList.IndexOf(effect);
        if (index == -1)
        {
            // what to do here?????
            Debug.WriteLine("EFFECT OWNER MATCHES THIS CLIP BUT IT IS NOT PLACED IN THE COLLECTION!!!");
            Debugger.Break();
            return false;
        }

        this.RemoveEffectAtInternal(index, effect);
        return true;
    }

    public void RemoveEffectAt(int index)
    {
        BaseEffect effect = this.internalEffectList[index];
        if (!ReferenceEquals(effect.Owner, this))
        {
            Debug.WriteLine("EFFECT STORED IN CLIP HAS A MISMATCHING OWNER!!!");
            Debugger.Break();
        }

        this.RemoveEffectAtInternal(index, effect);
    }

    public void MoveEffect(int oldIndex, int newIndex)
    {
        if (newIndex < 0 || newIndex >= this.internalEffectList.Count)
            throw new IndexOutOfRangeException($"{nameof(newIndex)} is not within range: {(newIndex < 0 ? "less than zero" : "greater than list length")} ({newIndex})");
        BaseEffect effect = this.internalEffectList[oldIndex];
        this.internalEffectList.RemoveAt(oldIndex);
        this.internalEffectList.Insert(newIndex, effect);
        this.EffectMoved?.Invoke(this, effect, oldIndex, newIndex);
    }

    private void RemoveEffectAtInternal(int index, BaseEffect effect)
    {
        this.internalEffectList.RemoveAt(index);
        BaseEffect.OnRemovedInternal(effect);
        this.OnEffectRemoved(index, effect);
    }

    private void OnEffectAdded(int index, BaseEffect effect)
    {
        this.EffectAdded?.Invoke(this, effect, index);
    }

    private void OnEffectRemoved(int index, BaseEffect effect)
    {
        this.EffectRemoved?.Invoke(this, effect, index);
    }

    public virtual void Destroy()
    {
        for (int i = this.Effects.Count - 1; i >= 0; i--)
        {
            BaseEffect effect = this.Effects[i];
            effect.Destroy();
            this.RemoveEffectAt(i);
        }
    }

    /// <summary>
    /// [INTERNAL ONLY] Called when the clip is added to the given track
    /// </summary>
    internal static void InternalOnClipAddedToTrack(Clip clip, Track track)
    {
        Track oldTrack = clip.Track;
        if (oldTrack == track)
            throw new Exception("Clip added to the same track?");
        clip.OnTrackChanged(oldTrack, clip.Track = track);
    }

    /// <summary>
    /// [INTERNAL ONLY] Called when the clip is removed from its owner track
    /// </summary>
    internal static void InternalOnClipRemovedFromTrack(Clip clip)
    {
        Track oldTrack = clip.Track;
        if (oldTrack == null)
            throw new InvalidOperationException("Clip removed from no track???");
        clip.OnTrackChanged(oldTrack, clip.Track = null);
    }

    /// <summary>
    /// [INTERNAL ONLY] Called when a clip moves from one track to another
    /// </summary>
    internal static void InternalOnClipMovedToTrack(Clip clip, Track? oldTrack, Track? newTrack)
    {
        if (clip.Track != oldTrack)
            throw new InvalidOperationException("Expected clip's old timeline to equal the given old timeline");
        clip.OnTrackChanged(oldTrack, clip.Track = newTrack);
    }

    /// <summary>
    /// [INTERNAL ONLY] Called when the timeline of the track that the given clip resides in changes
    /// </summary>
    internal static void InternalOnTrackTimelineChanged(Clip clip, Timeline? oldTimeline, Timeline? newTimeline)
    {
        if (clip.Timeline != oldTimeline)
            throw new InvalidOperationException("Expected clip's old timeline to equal the given old timeline");
        clip.OnTimelineChanged(oldTimeline, clip.Timeline = newTimeline);
    }

    /// <summary>
    /// [INTERNAL ONLY] Called when the timeline of the track that the given clip resides in changes
    /// </summary>
    internal static void InternalOnTimelineProjectChanged(Clip clip, Project? oldProject, Project? newProject)
    {
        if (clip.Project != oldProject)
            throw new InvalidOperationException("Expected clip's old project to equal the given old project");
        if (clip.Project == newProject)
            throw new InvalidOperationException("Did not expect clip's current project to equal the new project");
        clip.OnProjectChanged(oldProject, clip.Project = newProject);
    }

    internal static ClipGroup? InternalGetGroup(Clip clip) => clip.myGroup;
    internal static void InternalSetGroup(Clip clip, ClipGroup? group) => clip.myGroup = group;

    // Only used for faster code
    internal static List<BaseEffect> InternalGetEffectListUnsafe(Clip clip) => clip.internalEffectList;
}