namespace FramePFX.Editors.Timelines.Clips.Video {
    /// <summary>
    /// The base class for a video source. This is responsible for creating a video source context for a
    /// specific video clip which may or may not assist with actually providing video data.
    /// </summary>
    public abstract class VideoSource {
        /// <summary>
        /// The main method for creating a video source context using a given clip. The given clip's video source context is not affected by this method
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public abstract VideoSourceContext CreateContext(VideoClip clip);
    }
}