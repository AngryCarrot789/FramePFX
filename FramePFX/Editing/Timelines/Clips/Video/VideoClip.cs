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

using System.Numerics;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Editing.Timelines.Tracks;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Clips.Video;

/// <summary>
/// The base class for all clips that produce video data, whether that be an image, a video frame at some time, text, particles, etc.
/// <para>
/// When rendering a video clip, there are 3 phases:
/// </para>
/// <para>
/// Preparation on AMT (application main thread). This is where all tracks are processed (bottom to top), the
/// clip (or clips with a transition) are calculated and then those clips' setup functions are called.
/// The clip method is <see cref="PrepareRenderFrame"/>, which lets the clip store its current state in
/// a proxy object which is accessible via the render thread
/// </para>
/// <para>
/// Rendering (on a randomly assigned rendering thread). This is where the clip actually renders its contents.
/// Since this is done off the main thread on a render-specific thread, it's very important that the clip does
/// not access any un-synchronised data. The render data should be calculated in the preparation phase
/// </para>
/// <para>
/// Final frame assembly (on render thread). This is where all of the rendered data is assembled into a final
/// frame. An event (<see cref="RenderManager.FrameRendered"/>) is fired on the application main thread, and the view
/// port then presents the fully rendered frame to the user
/// </para>
/// </summary>
public abstract class VideoClip : Clip {
    public const double MinimumSpeed = 0.001;
    public const double MaximumSpeed = 1000.0;

    public static readonly ParameterDouble OpacityParameter =
        Parameter.RegisterDouble(
            typeof(VideoClip),
            nameof(VideoClip),
            nameof(Opacity),
            new ParameterDescriptorDouble(1, 0, 1),
            ValueAccessors.LinqExpression<double>(typeof(VideoClip), nameof(Opacity)),
            ParameterFlags.StandardProjectVisual);

    public static readonly ParameterVector2 MediaPositionParameter = Parameter.RegisterVector2(typeof(VideoClip), nameof(VideoClip), nameof(MediaPosition), ValueAccessors.LinqExpression<Vector2>(typeof(VideoClip), nameof(MediaPosition)), ParameterFlags.StandardProjectVisual);
    public static readonly ParameterVector2 MediaScaleParameter = Parameter.RegisterVector2(typeof(VideoClip), nameof(VideoClip), nameof(MediaScale), Vector2.One, ValueAccessors.LinqExpression<Vector2>(typeof(VideoClip), nameof(MediaScale)), ParameterFlags.StandardProjectVisual);
    public static readonly ParameterDouble MediaRotationParameter = Parameter.RegisterDouble(typeof(VideoClip), nameof(VideoClip), nameof(MediaRotation), ValueAccessors.LinqExpression<double>(typeof(VideoClip), nameof(MediaRotation)), ParameterFlags.StandardProjectVisual);
    public static readonly DataParameterVector2 MediaScaleOriginParameter = DataParameter.Register(new DataParameterVector2(typeof(VideoClip), nameof(MediaScaleOrigin), ValueAccessors.Reflective<Vector2>(typeof(VideoClip), nameof(mediaScaleOrigin))));
    public static readonly DataParameterVector2 MediaRotationOriginParameter = DataParameter.Register(new DataParameterVector2(typeof(VideoClip), nameof(MediaRotationOrigin), ValueAccessors.Reflective<Vector2>(typeof(VideoClip), nameof(mediaRotationOrigin))));

    public static readonly ParameterBool IsEnabledParameter = Parameter.RegisterBool(typeof(VideoClip), nameof(VideoClip), nameof(IsEnabled), true, ValueAccessors.LinqExpression<bool>(typeof(VideoClip), nameof(IsEnabled)), ParameterFlags.StandardProjectVisual);
    public static readonly DataParameterBool IsMediaScaleOriginAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(VideoClip), nameof(IsMediaScaleOriginAutomatic), true, ValueAccessors.Reflective<bool>(typeof(VideoClip), nameof(isMediaScaleOriginAutomatic)))); // DataParameterFlags.StandardProjectVisual
    public static readonly DataParameterBool IsMediaRotationOriginAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(VideoClip), nameof(IsMediaRotationOriginAutomatic), true, ValueAccessors.Reflective<bool>(typeof(VideoClip), nameof(isMediaRotationOriginAutomatic)))); // DataParameterFlags.StandardProjectVisual

    // Transformation data
    private Vector2 MediaPosition;
    private Vector2 MediaScale;
    private double MediaRotation;
    private Vector2 mediaScaleOrigin;
    private Vector2 mediaRotationOrigin;
    private bool isMediaScaleOriginAutomatic;
    private bool isMediaRotationOriginAutomatic;
    private SKMatrix myTransformationMatrix, myInverseTransformationMatrix;
    private SKMatrix myAbsoluteTransformationMatrix, myAbsoluteInverseTransformationMatrix;
    private bool isMatrixDirty;
    private bool isAdjustingFrameSpanForSpeedChange;
    private FrameSpan spanWithoutSpeed;
    private bool IsEnabled;

    // video clip stuff
    private double Opacity;

    /// <summary>
    /// Updated by the rendering engine when a clip begins rendering. This is a thread safe proxy of <see cref="Opacity"/>
    /// </summary>
    public double RenderOpacity;

    /// <summary>
    /// This is <see cref="RenderOpacity"/> converted to a byte
    /// </summary>
    public byte RenderOpacityByte;

    /// <summary>
    /// Returns true if this clip handles its own opacity calculations in order for a more
    /// efficient render. Returns false if it should be handled automatically using an offscreen buffer
    /// </summary>
    public bool UsesCustomOpacityCalculation { get; protected set; }

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

    public Vector2 MediaScaleOrigin {
        get => this.mediaScaleOrigin;
        set => DataParameter.SetValueHelper(this, MediaScaleOriginParameter, ref this.mediaScaleOrigin, value);
    }

    public Vector2 MediaRotationOrigin {
        get => this.mediaRotationOrigin;
        set => DataParameter.SetValueHelper(this, MediaRotationOriginParameter, ref this.mediaRotationOrigin, value);
    }

    public bool IsMediaScaleOriginAutomatic {
        get => this.isMediaScaleOriginAutomatic;
        set => DataParameter.SetValueHelper(this, IsMediaScaleOriginAutomaticParameter, ref this.isMediaScaleOriginAutomatic, value);
    }


    public bool IsMediaRotationOriginAutomatic {
        get => this.isMediaRotationOriginAutomatic;
        set => DataParameter.SetValueHelper(this, IsMediaRotationOriginAutomaticParameter, ref this.isMediaRotationOriginAutomatic, value);
    }

    /// <summary>
    /// Gets whether there is a playback speed set that is not 1.0
    /// </summary>
    public bool HasSpeedApplied { get; private set; }

    /// <summary>
    /// Gets the playback speed. 1.0 is the default
    /// </summary>
    public double PlaybackSpeed { get; private set; } = 1.0;

    /// <summary>
    /// Gets whether this clip is sensitive to the <see cref="PlaybackSpeed"/>. This is true for clips like media video clips, but is false for images (since they consist of a single frame)
    /// </summary>
    public virtual bool IsSensitiveToPlaybackSpeed => false;

    public bool IsEffectivelyVisible => this.IsEnabled && this.Opacity > 0.0;

    public SKRect LastRenderRect;

    protected VideoClip() {
        this.isMatrixDirty = true;
        this.Opacity = OpacityParameter.Descriptor.DefaultValue;
        this.IsEnabled = IsEnabledParameter.Descriptor.DefaultValue;
        this.MediaPosition = MediaPositionParameter.Descriptor.DefaultValue;
        this.MediaScale = MediaScaleParameter.Descriptor.DefaultValue;
        this.MediaRotation = MediaRotationParameter.Descriptor.DefaultValue;
        this.mediaScaleOrigin = MediaScaleOriginParameter.GetDefaultValue(this);
        this.mediaRotationOrigin = MediaRotationOriginParameter.GetDefaultValue(this);
        this.isMediaScaleOriginAutomatic = IsMediaScaleOriginAutomaticParameter.GetDefaultValue(this);
        this.isMediaRotationOriginAutomatic = IsMediaRotationOriginAutomaticParameter.GetDefaultValue(this);
    }

    static VideoClip() {
        SerialisationRegistry.Register<VideoClip>(0, (clip, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            clip.isMediaScaleOriginAutomatic = data.GetBool("IsMediaScaleOriginAutomatic");
            clip.isMediaRotationOriginAutomatic = data.GetBool("IsMediaRotationOriginAutomatic");
            clip.PlaybackSpeed = Maths.Clamp(data.GetDouble("PlaybackSpeed"), MinimumSpeed, MaximumSpeed);
            clip.spanWithoutSpeed = data.GetStruct<FrameSpan>("SpanWOSpeed").Clamp(new FrameSpan(0, long.MaxValue));
            clip.HasSpeedApplied = data.GetBool("HasSpeedApplied");
            clip.isMatrixDirty = true;
        }, (clip, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetBool("IsMediaScaleOriginAutomatic", clip.isMediaScaleOriginAutomatic);
            data.SetBool("IsMediaRotationOriginAutomatic", clip.isMediaRotationOriginAutomatic);
            data.SetDouble("PlaybackSpeed", clip.PlaybackSpeed);
            data.SetStruct("SpanWOSpeed", clip.spanWithoutSpeed);
            data.SetBool("HasSpeedApplied", clip.HasSpeedApplied);
        });

        Parameter.AddMultipleHandlers(s => ((VideoClip) s.AutomationData.Owner).InvalidateTransformationMatrix(), MediaPositionParameter, MediaScaleParameter, MediaRotationParameter);
        DataParameter.AddMultipleHandlers((p, o) => ((VideoClip) o).InvalidateTransformationMatrix(), MediaScaleOriginParameter, MediaRotationOriginParameter);
        IsMediaScaleOriginAutomaticParameter.PriorityValueChanged += (parameter, owner) => ((VideoClip) owner).UpdateAutomaticScaleOrigin(false);
        IsMediaRotationOriginAutomaticParameter.PriorityValueChanged += (parameter, owner) => ((VideoClip) owner).UpdateAutomaticRotationOrigin(false);
    }

    /// <summary>
    /// Multiplies the frame by our playback speed and floors the result as a long
    /// </summary>
    /// <param name="frame">The input frame</param>
    /// <returns>The output scaled frame</returns>
    public long GetRelativeFrameForPlaybackSpeed(long frame) => !this.HasSpeedApplied ? frame : (long) Math.Round(frame * this.PlaybackSpeed);

    protected override void LoadDataIntoClone(Clip clone, ClipCloneOptions options) {
        base.LoadDataIntoClone(clone, options);
        VideoClip clip = (VideoClip) clone;
        clip.PlaybackSpeed = this.PlaybackSpeed;
        clip.spanWithoutSpeed = this.spanWithoutSpeed;
        clip.HasSpeedApplied = this.HasSpeedApplied;
        clip.isMediaScaleOriginAutomatic = this.isMediaScaleOriginAutomatic;
        clip.isMediaRotationOriginAutomatic = this.isMediaRotationOriginAutomatic;
    }

    /// <summary>
    /// Sets the playback speed. This method also updates the <see cref="Clip.FrameSpan"/> to accomodate.
    /// If set to 1.0, <see cref="HasSpeedApplied"/> becomes 0 and our span is set back to the original
    /// </summary>
    /// <param name="speed"></param>
    public void SetPlaybackSpeed(double speed) {
        speed = Maths.Clamp(speed, MinimumSpeed, MaximumSpeed);
        double oldSpeed = this.PlaybackSpeed;
        if (DoubleUtils.AreClose(oldSpeed, speed)) {
            return;
        }

        this.PlaybackSpeed = speed;
        this.isAdjustingFrameSpanForSpeedChange = true;
        if (DoubleUtils.AreClose(speed, 1.0)) {
            this.HasSpeedApplied = false;
            this.FrameSpan = this.spanWithoutSpeed;
        }
        else {
            this.HasSpeedApplied = true;
            double newDuration = this.spanWithoutSpeed.Duration / speed;
            this.FrameSpan = new FrameSpan(this.FrameSpan.Begin, Math.Max((long) Math.Floor(newDuration), 1));
        }

        this.isAdjustingFrameSpanForSpeedChange = false;
    }

    /// <summary>
    /// Sets the playback speed back to 1.0. This is a helper method to calling <see cref="SetPlaybackSpeed"/> with 1.0
    /// </summary>
    public void ClearPlaybackSpeed() => this.SetPlaybackSpeed(1.0);

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

    protected virtual void UpdateAutomaticScaleOrigin(bool isInitialising) {
        if (this.IsMediaScaleOriginAutomatic) {
            SKSize size = this.GetSizeForAutomaticOrigins();
            Vector2 value = new Vector2(size.Width / 2, size.Height / 2);
            if (isInitialising) {
                this.MediaScaleOrigin = value;
            }
            else {
                MediaScaleOriginParameter.SetValue(this, value);
            }
        }
    }

    protected virtual void UpdateAutomaticRotationOrigin(bool isInitialising) {
        if (this.IsMediaRotationOriginAutomatic) {
            SKSize size = this.GetSizeForAutomaticOrigins();
            Vector2 value = new Vector2(size.Width / 2, size.Height / 2);
            if (isInitialising) {
                this.MediaRotationOrigin = value;
            }
            else {
                MediaRotationOriginParameter.SetValue(this, value);
            }
        }
    }

    protected void UpdateAutomaticScaleAndRotationOrigin(bool isInitialising) {
        this.UpdateAutomaticScaleOrigin(isInitialising);
        this.UpdateAutomaticRotationOrigin(isInitialising);
    }

    public virtual SKSize GetSizeForAutomaticOrigins() {
        Vector2 sz = this.GetRenderSize() ?? default;
        return new SKSize(sz.X, sz.Y);
    }

    protected override void OnTrackChanged(Track? oldTrack, Track? newTrack) {
        base.OnTrackChanged(oldTrack, newTrack);
        this.InvalidateTransformationMatrix();
    }

    /// <summary>
    /// Gets the raw amount of space this clip takes up on screen, unaffected by standard transformation matrices.
    /// If the value is unavailable, then typically the render viewport's width and height are used as a fallback
    /// <para>
    /// This value also may only be available after the clip has rendered at least once, and it may also be
    /// completely different after each render phase due to the nature of the clip itself, so this should just
    /// be treated as a general hint
    /// </para>
    /// </summary>
    /// <returns>The size, if applicable, otherwise null</returns>
    public virtual Vector2? GetRenderSize() => null;

    protected virtual void OnRenderSizeChanged() {
        this.UpdateAutomaticScaleAndRotationOrigin(false);
        this.InvalidateRender();
    }

    public override bool IsEffectTypeAccepted(Type effectType) => typeof(VideoEffect).IsAssignableFrom(effectType);

    /// <summary>
    /// Propagates the render invalidated state to our project's <see cref="RenderManager"/>
    /// </summary>
    public void InvalidateRender() {
        this.Timeline?.RenderManager.InvalidateRender();
    }

    protected override void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan) {
        if (!this.isAdjustingFrameSpanForSpeedChange) {
            if (this.HasSpeedApplied) {
                long change = newSpan.Duration - oldSpan.Duration;
                this.spanWithoutSpeed = new FrameSpan(newSpan.Begin, this.spanWithoutSpeed.Duration + change);
            }
            else {
                this.spanWithoutSpeed = newSpan;
            }
        }

        base.OnFrameSpanChanged(oldSpan, newSpan);
        this.InvalidateRender();
    }

    /// <summary>
    /// Prepares this clip for rendering. This is called on the main thread, and allows rendering data
    /// to be cached locally so that it can be accessed safely by a render thread in <see cref="RenderFrame"/>.
    /// </summary>
    /// <param name="rc">The pre-render setup context</param>
    /// <param name="frame">The play head frame, relative to this clip. This will always be within range of our span</param>
    /// <returns>True if this clip can be rendered (meaning <see cref="RenderFrame"/> may be called after this call)</returns>
    public abstract bool PrepareRenderFrame(PreRenderContext rc, long frame);

    /// <summary>
    /// Renders this clip using the given rendering context data. This is called on a randomly
    /// assigned rendering thread, therefore, this method should not access un-synchronised clip data
    /// </summary>
    /// <param name="rc">The rendering context, containing things such as the surface and canvas to draw to</param>
    /// <param name="renderArea">
    /// Used as an optimisation to know where this clip actually drew data, and only that area will be used.
    /// This defaults to the destination surface's frame size (calculated via the render context's image info),
    /// meaning it is unoptimised by default
    /// </param>
    public abstract void RenderFrame(RenderContext rc, ref SKRect renderArea);

    public void InvalidateTransformationMatrix() {
        this.isMatrixDirty = true;
        this.InvalidateRender();
    }

    internal static void InternalInvalidateTransformationMatrixFromTrack(VideoClip clip) {
        clip.isMatrixDirty = true;
    }
}