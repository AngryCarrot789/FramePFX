namespace FramePFX.Core.Editor.ViewModels.Timelines
{
    public interface IClipDragHandler
    {
        /// <summary>
        /// Whether or not this clip's left thumb is being dragged
        /// </summary>
        bool IsDraggingLeftThumb { get; }

        /// <summary>
        /// Whether or not this clip's right thumb is being dragged
        /// </summary>
        bool IsDraggingRightThumb { get; }

        /// <summary>
        /// Whether or not this clip is being dragged
        /// </summary>
        bool IsDraggingClip { get; }

        /// <summary>
        /// Invoked when the user starts dragging the clip's left thumb (e.g. mouse down and mouse up)
        /// </summary>
        void OnLeftThumbDragStart();

        /// <summary>
        /// Invoked when the user stops dragging the clip's left thumb (e.g. mouse down and mouse up)
        /// </summary>
        /// <param name="cancelled">If the drag was cancelled (user clicked escape, causing the previous data to be restored) or if the drag was success</param>
        void OnLeftThumbDragStop(bool cancelled);

        /// <summary>
        /// Invoked when the clip's left thumb is dragged left or right
        /// </summary>
        /// <param name="offset">The amount of frames this clip was dragged. Positive is towards the right, negative is towards the left</param>
        void OnLeftThumbDelta(long offset);

        /// <summary>
        /// Invoked when the user starts dragging the clip's right thumb (e.g. mouse down and mouse up)
        /// </summary>
        void OnRightThumbDragStart();

        /// <summary>
        /// Invoked when the user stops dragging the clip's right thumb (e.g. mouse down and mouse up)
        /// </summary>
        /// <param name="cancelled">If the drag was cancelled (user clicked escape, causing the previous data to be restored) or if the drag was success</param>
        void OnRightThumbDragStop(bool cancelled);

        /// <summary>
        /// Invoked when the clip's right thumb is dragged left or right
        /// </summary>
        /// <param name="offset">The amount of frames this clip was dragged. Positive is towards the right, negative is towards the left</param>
        void OnRightThumbDelta(long offset);

        /// <summary>
        /// Invoked when the user starts dragging the clip (e.g. mouse down and mouse up)
        /// </summary>
        void OnDragStart();

        /// <summary>
        /// Invoked when the user stops dragging the clip (e.g. mouse down and mouse up)
        /// </summary>
        /// <param name="cancelled">If the drag was cancelled (user clicked escape, causing the previous data to be restored) or if the drag was success</param>
        void OnDragStop(bool cancelled);

        /// <summary>
        /// Invoked when the clip is dragged left or right
        /// </summary>
        /// <param name="offset">The amount of frames this clip was dragged. Positive is towards the right, negative is towards the left</param>
        void OnDragDelta(long offset);

        void OnDragToTrack(int index);
    }
}