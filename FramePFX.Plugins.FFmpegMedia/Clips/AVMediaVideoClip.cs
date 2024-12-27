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
using FFmpeg.AutoGen;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging.NewResourceHelper;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.FFmpegWrapper;
using FramePFX.Logging;
using FramePFX.Plugins.FFmpegMedia.Resources;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Plugins.FFmpegMedia.Clips;

public class AVMediaVideoClip : VideoClip {
    public static readonly ResourceSlot<ResourceAVMedia> MediaKey = ResourceSlot.Register<ResourceAVMedia>(typeof(AVMediaVideoClip), "AVMediaKey");

    private VideoFrame? renderFrameRgb, downloadedHwFrame;
    private unsafe SwsContext* scaler;
    private PictureFormat scalerInputFormat;
    private VideoFrame? lastReadyFrame;
    private long currentFrame = -1;
    private Task<VideoFrame?>? decodeFrameTask;
    private long decodeFrameBegin;
    private long lastDecodeFrameDuration;

    public AVMediaVideoClip() {
        this.IsMediaFrameSensitive = true;
        this.UsesCustomOpacityCalculation = true;
    }

    static AVMediaVideoClip() {
        SerialisationRegistry.Register<AVMediaVideoClip>(0, (clip, data, ctx) => {
            ctx.DeserialiseBaseType(data);
        }, (clip, data, ctx) => {
            ctx.SerialiseBaseType(data);
        });

        MediaKey.ResourceChanged += (slot, owner, oldResource, newResource) => {
            AVMediaVideoClip clip = (AVMediaVideoClip) owner;
            clip.renderFrameRgb?.Dispose();
            clip.renderFrameRgb = null;
            clip.downloadedHwFrame?.Dispose();
            clip.downloadedHwFrame = null;
            unsafe {
                if (clip.scaler != null) {
                    ffmpeg.sws_freeContext(clip.scaler);
                    clip.scaler = null;
                }
            }
            
            clip.OnRenderSizeChanged();
            // if (newResource != null && !clip.IsAutomated(MediaScaleParameter) && clip.Project?.Settings is ProjectSettings settings) {
            //     Vector2 scale = MediaScaleParameter.GetCurrentValue(clip);
            //     if (scale == new Vector2(1, 1)) {
            //         if (((ResourceAVMedia) newResource).GetResolution() is SKSizeI size) {
            //             Vector2 newScale = new Vector2((float) settings.Width / size.Width, (float) settings.Height / size.Height);
            //             clip.AutomationData[MediaScaleParameter].DefaultKeyFrame.SetVector2Value(newScale, MediaScaleParameter.Descriptor);
            //         }
            //     }
            // }
        };
    }

    public override Vector2? GetRenderSize() {
        if (MediaKey.TryGetResource(this, out ResourceAVMedia? resource) && resource.GetResolution() is SKSizeI size)
            return new Vector2(size.Width, size.Height);

        return null;
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        if (!MediaKey.TryGetResource(this, out ResourceAVMedia? resource))
            return false;

        // if (!this.ResourceAVMediaKey.TryGetResource(out ResourceAVMedia? resource))
        //     return false;

        if (resource.stream == null || resource.Demuxer == null)
            return false;

        if (frame == this.currentFrame && this.renderFrameRgb != null) {
            return true;
        }

        if (this.renderFrameRgb == null) {
            unsafe {
                AVCodecParameters* pars = resource.stream.Handle->codecpar;
                this.renderFrameRgb = new VideoFrame(pars->width, pars->height, PixelFormats.BGRA);
            }
        }

        // TimeSpan timestamp = TimeSpan.FromTicks((long) ((frame - this.MediaFrameOffset) * this.targetFrameIntervalTicks));
        TimeSpan timestamp = TimeSpan.FromSeconds((frame - this.MediaFrameOffset) / (this.Project!.Settings.FrameRateDouble / this.PlaybackSpeed));

        // No need to dispose as the frames are stored in a frame buffer, which is disposed by the resource itself
        this.currentFrame = frame;
        this.decodeFrameTask = Task.Run(() => {
            this.decodeFrameBegin = Time.GetSystemTicks();
            try {
                VideoFrame? output = null;
                VideoFrame? ready = resource.GetFrameAt(timestamp, out _);
                if (ready != null && !ready.IsDisposed) {
                    // TODO: Maybe add an async frame fetcher that buffers the frames, or maybe add
                    // a project preview resolution so that decoding is lightning fast for low resolution?
                    if (ready.IsHardwareFrame) {
                        // As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                        // which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                        // hacking into the API specific device context (like D3D11VA).
                        ready.TransferTo(this.downloadedHwFrame ??= new VideoFrame());
                        ready = this.downloadedHwFrame;
                    }

                    this.ScaleFrame(ready);
                    output = ready;
                }

                this.lastDecodeFrameDuration = Time.GetSystemTicks() - this.decodeFrameBegin;
                return output;
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Decoder exception: " + e.GetToString());
                return null;
            }
        });

        return true;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        VideoFrame? ready;
        if (this.decodeFrameTask != null) {
            this.lastReadyFrame = ready = this.decodeFrameTask.Result;
            this.decodeFrameTask = null;
            // System.Diagnostics.Debug.WriteLine("Last decode time: " + Math.Round((double) this.lastDecodeFrameDuration) / Time.TICK_PER_MILLIS + " ms");
        }
        else {
            ready = this.lastReadyFrame;
        }

        if (ready == null || ready.IsDisposed) {
            renderArea = default;
            return;
        }

        unsafe {
            // long startA = Time.GetSystemTicks();
            byte* ptr;
            GetFrameData(this.renderFrameRgb!, 0, &ptr, out int rowBytes);
            SKImageInfo image = new SKImageInfo(this.renderFrameRgb!.Width, this.renderFrameRgb.Height, SKColorType.Bgra8888);
            using SKPixmap pixmap = new SKPixmap(image, (IntPtr) ptr, rowBytes);
            using SKImage img = SKImage.FromPixels(pixmap, null, null);
            if (img == null) {
                return;
            }

            using SKPaint? paint = rc.FilterQuality != SKFilterQuality.None || this.RenderOpacityByte != 255 ? new SKPaint() : null;
            if (paint != null) {
                paint.FilterQuality = rc.FilterQuality;
                paint.Color = SKColors.White.WithAlpha(this.RenderOpacityByte);
            }

            rc.Canvas.DrawImage(img, 0, 0, paint);

            renderArea = rc.TranslateRect(new SKRect(0, 0, image.Width, image.Height));

            // long time = Time.GetSystemTicks() - startA;
            // System.Diagnostics.Debug.WriteLine("Last render time: " + Math.Round((double) time) / Time.TICK_PER_MILLIS + " ms");
        }
    }

    private unsafe void ScaleFrame(VideoFrame ready) {
        if (this.scaler == null) {
            PictureFormat srcfmt = ready.Format;
            PictureFormat dstfmt = this.renderFrameRgb!.Format;
            this.scalerInputFormat = srcfmt;
            this.scaler = ffmpeg.sws_getContext(srcfmt.Width, srcfmt.Height, srcfmt.PixelFormat, dstfmt.Width, dstfmt.Height, dstfmt.PixelFormat, ffmpeg.SWS_BICUBIC, null, null, null);
        }

        AVFrame* src = ready.Handle;
        AVFrame* dst = this.renderFrameRgb!.Handle;

        // Workaround for when the frame size changes, which can be done with a well crafted video file.
        // Typically it would crash but now it just stretches the buffer
        int min = Math.Min(this.scalerInputFormat.Height, ready.Height);
        ffmpeg.sws_scale(this.scaler, src->data, src->linesize, 0, min, dst->data, dst->linesize);
    }

    public static unsafe void GetFrameData(VideoFrame frame, int plane, byte** data, out int stride) {
        int height = frame.GetPlaneSize(plane).Height;
        AVFrame* ptr = frame.Handle;
        *data = ptr->data[(uint) plane];
        int rowSize = ptr->linesize[(uint) plane];

        if (rowSize < 0) {
            *data += rowSize * (height - 1);
            rowSize = unchecked(rowSize * -1);
        }

        stride = rowSize / sizeof(byte);
    }
}