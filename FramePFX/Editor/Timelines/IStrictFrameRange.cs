namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// An interface for objects (typically clips and effects) that have a strict frame span range which
    /// requires translating play head frames into relative frames
    /// </summary>
    public interface IStrictFrameRange {
        /// <summary>
        /// Converts a relative frame to an absolute frame, relative to the timeline
        /// </summary>
        /// <param name="relative">Input relative frame</param>
        /// <returns>Output absolute frame</returns>
        long ConvertRelativeToTimelineFrame(long relative);

        /// <summary>
        /// Converts an absolute timeline frame into a relative frame
        /// </summary>
        /// <param name="timeline">Input timeline frame</param>
        /// <param name="inRange">
        /// True if the timeline frame is within our strict frame range,
        /// otherwise false, meaning it is out of range (and technically invalid)
        /// </param>
        /// <returns>Output relative frame</returns>
        long ConvertTimelineToRelativeFrame(long timeline, out bool inRange);

        /// <summary>
        /// Returns true if the timeline frame is within our strict frame range,
        /// otherwise false, meaning it is out of range (and technically invalid)
        /// </summary>
        /// <param name="timeline">Input timeline frame</param>
        /// <returns>See above</returns>
        bool IsTimelineFrameInRange(long timeline);
    }
}