namespace FramePFX.Editor.Timelines.Events {
    /// <summary>
    /// A delegate type for the <see cref="Clip.TrackChanged"/> event
    /// <param name="oldTrack">The previous track. Null when the clip is being added, non-null when removing or moving</param>
    /// <param name="newTrack">The new track. Null when the clip is being removed, non-null when adding or moving</param>
    /// </summary>
    public delegate void TrackChangedEventHandler(Track oldTrack, Track newTrack);
}