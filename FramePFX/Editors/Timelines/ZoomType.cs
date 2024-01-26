namespace FramePFX.Editors.Timelines {
    /// <summary>
    /// Used by the UI to post process zooming, e.g., automatically scroll to the cursor based on the zoom change
    /// </summary>
    public enum ZoomType {
        /// <summary>
        /// No additional processing of the new zoom value should be done, e.g., don't scroll the timeline
        /// </summary>
        Direct,
        /// <summary>
        /// Zoom towards the start of the view port
        /// </summary>
        ViewPortBegin,
        /// <summary>
        /// Zoom towards the middle of the view port
        /// </summary>
        ViewPortMiddle,
        /// <summary>
        /// Zoom towards the end of the view port
        /// </summary>
        ViewPortEnd,
        /// <summary>
        /// Zoom towards the play head
        /// </summary>
        PlayHead,
        /// <summary>
        /// Zoom towards the mouse cursor
        /// </summary>
        MouseCursor
    }
}