// 
// Copyright (c) 2026-2026 REghZy
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

using System.Numerics;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editing.Video;

public abstract class VideoClip : Clip {
    public static readonly DataParameterBool IsVisibleParameter =
        DataParameter.Register(
            new DataParameterBool(
                typeof(VideoClip),
                nameof(IsVisible),
                true,
                ValueAccessors.GetSet(o => ((VideoClip) o).isVisible, (o, v) => ((VideoClip) o).isVisible = v)));
    
    public static readonly DataParameterNumber<double> OpacityParameter =
        DataParameter.Register(
            new DataParameterNumber<double>(
                typeof(VideoClip),
                nameof(Opacity),
                1.0, 0.0, 1.0,
                ValueAccessors.GetSet(o => ((VideoClip) o).opacity, (o, v) => ((VideoClip) o).opacity = v)));

    public static readonly DataParameterVector2 MediaPositionParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoClip),
                nameof(MediaPosition),
                ValueAccessors.GetSet(o => ((VideoClip) o).mediaPosition, (o, v) => ((VideoClip) o).mediaPosition = v)));

    public static readonly DataParameterVector2 MediaScaleParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoClip),
                nameof(MediaScale),
                Vector2.One,
                ValueAccessors.GetSet(o => ((VideoClip) o).mediaScale, (o, v) => ((VideoClip) o).mediaScale = v)));

    public static readonly DataParameterNumber<double> MediaRotationParameter =
        DataParameter.Register(
            new DataParameterNumber<double>(
                typeof(VideoClip),
                nameof(MediaRotation),
                ValueAccessors.GetSet(o => ((VideoClip) o).mediaRotation, (o, v) => ((VideoClip) o).mediaRotation = v)));

    public static readonly DataParameterVector2 MediaScaleOriginParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoClip),
                nameof(MediaScaleOrigin),
                ValueAccessors.GetSet(o => ((VideoClip) o).mediaScaleOrigin, (o, v) => ((VideoClip) o).mediaScaleOrigin = v)));

    public static readonly DataParameterVector2 MediaRotationOriginParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoClip),
                nameof(MediaRotationOrigin),
                ValueAccessors.GetSet(o => ((VideoClip) o).mediaRotationOrigin, (o, v) => ((VideoClip) o).mediaRotationOrigin = v)));


    internal double InternalRenderOpacity;
    internal byte InternalRenderOpacityByte;
    internal SKRect InternalLastRenderRect;
    
    private bool isVisible;
    private double opacity;
    private Vector2 mediaPosition;
    private Vector2 mediaScale;
    private double mediaRotation;
    private Vector2 mediaScaleOrigin;
    private Vector2 mediaRotationOrigin;
    private SKMatrix myTransformationMatrix, myInverseTransformationMatrix;
    private SKMatrix myAbsoluteTransformationMatrix, myAbsoluteInverseTransformationMatrix;
    private bool isMatrixDirty;

    public bool IsVisible {
        get => this.isVisible;
        set => DataParameter.SetValueHelper(this, IsVisibleParameter, ref this.isVisible, value);
    }
    
    public double Opacity {
        get => this.opacity;
        set => DataParameter.SetValueHelper(this, OpacityParameter, ref this.opacity, value);
    }

    public Vector2 MediaPosition {
        get => this.mediaPosition;
        set => DataParameter.SetValueHelper(this, MediaPositionParameter, ref this.mediaPosition, value);
    }

    public Vector2 MediaScale {
        get => this.mediaScale;
        set => DataParameter.SetValueHelper(this, MediaScaleParameter, ref this.mediaScale, value);
    }

    public double MediaRotation {
        get => this.mediaRotation;
        set => DataParameter.SetValueHelper(this, MediaRotationParameter, ref this.mediaRotation, value);
    }

    public Vector2 MediaScaleOrigin {
        get => this.mediaScaleOrigin;
        set => DataParameter.SetValueHelper(this, MediaScaleOriginParameter, ref this.mediaScaleOrigin, value);
    }

    public Vector2 MediaRotationOrigin {
        get => this.mediaRotationOrigin;
        set => DataParameter.SetValueHelper(this, MediaRotationOriginParameter, ref this.mediaRotationOrigin, value);
    }

    /// <summary>
    /// Gets the transformation matrix for the transformation properties in this clip
    /// only, not including parent transformations. This is our local-to-world matrix
    /// </summary>
    public SKMatrix TransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the absolute transformation matrix, which is a concatenation of all of our
    /// parents' matrices and our own. This is our local-to-world matrix
    /// </summary>
    public SKMatrix AbsoluteTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myAbsoluteTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the inverse of our transformation matrix. This is our world-to-local matrix
    /// </summary>
    public SKMatrix InverseTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myInverseTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the inverse of our absolute transformation matrix. This can be used to, for example,
    /// map a location on the entire canvas to this clip. This is our world-to-local matrix
    /// </summary>
    public SKMatrix AbsoluteInverseTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myAbsoluteInverseTransformationMatrix;
        }
    }

    protected double RenderOpacity => this.InternalRenderOpacity;

    protected double RenderOpacityByte => this.InternalRenderOpacityByte;

    /// <summary>
    /// Returns true if this clip handles its own opacity calculations in order for a more
    /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
    /// </summary>
    public bool UsesCustomOpacityCalculation { get; protected set; }

    internal sealed override ClipType InternalClipType => ClipType.Video;

    /// <summary>
    /// An event raised when the render state of this video clip becomes invalid, such as from <see cref="Opacity"/> changing.
    /// This is raised before <see cref="Timeline.RenderInvalidated"/>
    /// </summary>
    public event EventHandler? RenderInvalidated;

    protected VideoClip() {
        this.isMatrixDirty = true;
        this.isVisible = IsVisibleParameter.GetDefaultValue(this);
        this.opacity = OpacityParameter.GetDefaultValue(this);
        this.mediaPosition = MediaPositionParameter.GetDefaultValue(this);
        this.mediaScale = MediaScaleParameter.GetDefaultValue(this);
        this.mediaRotation = MediaRotationParameter.GetDefaultValue(this);
        this.mediaScaleOrigin = MediaScaleOriginParameter.GetDefaultValue(this);
        this.mediaRotationOrigin = MediaRotationOriginParameter.GetDefaultValue(this);
    }

    static VideoClip() {
        AffectsRender(IsVisibleParameter);
        AffectsRender(OpacityParameter);
        AffectsRender(MediaPositionParameter);
        AffectsRender(MediaScaleParameter);
        AffectsRender(MediaRotationParameter);
        AffectsRender(MediaScaleOriginParameter);
        AffectsRender(MediaRotationOriginParameter);
        DataParameter.AddMultipleHandlers((p, o) => ((VideoClip) o.Owner).InvalidateTransformationMatrix(), MediaPositionParameter, MediaScaleParameter, MediaRotationParameter, MediaScaleOriginParameter, MediaRotationOriginParameter);
    }

    protected static void AffectsRender(DataParameter parameter) {
        parameter.ValueChanged += OnRenderAffectingParameterChanged;
    }

    private static void OnRenderAffectingParameterChanged(DataParameter sender, DataParameterValueChangedEventArgs e) {
        ((VideoClip) e.Owner).RaiseRenderInvalidated();
    }

    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event
    /// </summary>
    public void RaiseRenderInvalidated() {
        this.RenderInvalidated?.Invoke(this, EventArgs.Empty);
        ((VideoTrack?) this.Track)?.RaiseRenderInvalidated(this.Span);
    }

    internal void InternalBeginRender(TimeSpan offset, PreRenderContext context) {
        this.InternalRenderOpacity = this.Opacity;
        this.InternalRenderOpacityByte = (byte) Math.Clamp(this.Opacity * 255.0D, 0.0, 255.0);
        this.BeginRender(offset, context);
    }

    internal void InternalEndRender(TimeSpan offset, RenderContext context, ref SKRect renderArea, CancellationToken cancellationToken) {
        this.EndRender(offset, context, ref renderArea, cancellationToken);
    }

    /// <summary>
    /// Begins rendering this video clip. This is always called before <see cref="BeginRender"/>.
    /// Invoking this method multiple times before <see cref="EndRender"/> is permitted although unlikely
    /// </summary>
    /// <param name="offset">The play head position, relative to the clip's <see cref="Clip.Span"/></param>
    /// <param name="context">The pre-rendering context, containing frame info</param>
    protected abstract void BeginRender(TimeSpan offset, PreRenderContext context);

    /// <summary>
    /// Ends rendering of this clip. This is always called after <see cref="BeginRender"/>
    /// </summary>
    /// <param name="offset">The play head position, relative to the clip's <see cref="Clip.Span"/></param>
    /// <param name="rc">The rendering context, containing frame info and a render target</param>
    /// <returns>A task that completes once rendering has finished</returns>
    protected abstract void EndRender(TimeSpan offset, RenderContext rc, ref SKRect renderArea, CancellationToken cancellationToken);
    
    private void GenerateMatrices() {
        this.myTransformationMatrix = MatrixUtils.CreateTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, new Vector2(this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y), new Vector2(this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
        this.myInverseTransformationMatrix = MatrixUtils.CreateInverseTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, new Vector2(this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y), new Vector2(this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
        if (this.Track is VideoTrack vidTrack) {
            // If VideoTrack could easily access the composition clip that is currently in use,
            // These would use the absolute matrices. But since we can't get the clip,
            // VideoTrack only supports non-absolute matrices
            this.myAbsoluteTransformationMatrix = vidTrack.TransformationMatrix.PreConcat(this.myTransformationMatrix);
            this.myAbsoluteInverseTransformationMatrix = this.myInverseTransformationMatrix.PreConcat(vidTrack.InverseTransformationMatrix);
        }
        else {
            this.myAbsoluteTransformationMatrix = this.myTransformationMatrix;
            this.myAbsoluteInverseTransformationMatrix = this.myInverseTransformationMatrix;
        }

        this.isMatrixDirty = false;
    }
    
    public void InvalidateTransformationMatrix() {
        this.isMatrixDirty = true;
        this.RaiseRenderInvalidated();
    }

    internal static void InternalInvalidateTransformationMatrixFromTrack(VideoClip clip) {
        clip.isMatrixDirty = true;
    }
}

public sealed class BlankVideoClip : VideoClip {
    public BlankVideoClip() {
    }

    protected override void BeginRender(TimeSpan offset, PreRenderContext context) {
    }

    protected override void EndRender(TimeSpan offset, RenderContext rc, ref SKRect renderArea, CancellationToken cancellationToken) {
    }
}

public sealed class ShapeVideoClip : VideoClip {
    public static readonly DataParameterNumber<double> WidthParameter = DataParameter.Register(new DataParameterNumber<double>(typeof(ShapeVideoClip), nameof(Width), 0, ValueAccessors.Reflective<double>(typeof(ShapeVideoClip), nameof(width))));
    public static readonly DataParameterNumber<double> HeightParameter = DataParameter.Register(new DataParameterNumber<double>(typeof(ShapeVideoClip), nameof(Height), 0, ValueAccessors.Reflective<double>(typeof(ShapeVideoClip), nameof(height))));

    private double width, height;
    private RenderData renderData;

    public double Width {
        get => this.width;
        set => DataParameter.SetValueHelper(this, WidthParameter, ref this.width, value);
    }

    public double Height {
        get => this.height;
        set => DataParameter.SetValueHelper(this, HeightParameter, ref this.height, value);
    }

    public ShapeVideoClip() {
        this.width = WidthParameter.GetDefaultValue(this);
        this.height = HeightParameter.GetDefaultValue(this);
    }

    protected override void BeginRender(TimeSpan offset, PreRenderContext context) {
        this.renderData = new RenderData() {
            size = new Vector2((float) this.width, (float) this.height),
            colour = this.Track!.Colour
        };
    }
    
    protected override void EndRender(TimeSpan offset, RenderContext rc, ref SKRect renderArea, CancellationToken cancellationToken) {
        RenderData d = this.renderData;
        SKColor colour = RenderUtils.BlendAlpha(d.colour, this.RenderOpacity);
        using (SKPaint paint = new SKPaint()) {
            paint.Color = colour;
            paint.IsAntialias = true;
            paint.FilterQuality = SKFilterQuality.High;

            rc.Canvas.DrawRect(0, 0, d.size.X, d.size.Y, paint);
        }

        renderArea = rc.TranslateRect(new SKRect(0, 0, d.size.X, d.size.Y));
    }
    
    private struct RenderData {
        public Vector2 size;
        public SKColor colour;
    }
}