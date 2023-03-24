namespace FramePFX.Timeline.Layer.Clips.Resizable {
    /// <summary>
    /// Contains data about a clip that can resize
    /// </summary>
    public interface IResizableClipData {
        float ShapeX { get; set; }

        float ShapeY { get; set; }

        float ShapeWidth { get; set; }

        float ShapeHeight { get; set; }

        bool UseScaledRender { get; set; }
    }
}