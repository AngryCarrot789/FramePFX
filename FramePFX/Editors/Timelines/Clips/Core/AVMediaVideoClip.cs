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

using System;
using System.Numerics;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.FFmpegWrapper;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips.Core {
    public class AVMediaVideoClip : VideoClip {
        private VideoFrame renderFrameRgb, downloadedHwFrame;
        private unsafe SwsContext* scaler;
        private PictureFormat scalerInputFormat;
        private VideoFrame lastReadyFrame;
        private long currentFrame = -1;
        private Task<VideoFrame> decodeFrameTask;
        private long decodeFrameBegin;
        private long lastDecodeFrameDuration;

        public IResourcePathKey<ResourceAVMedia> ResourceAVMediaKey { get; }

        public AVMediaVideoClip() {
            this.IsMediaFrameSensitive = true;
            this.UsesCustomOpacityCalculation = true;
            this.ResourceAVMediaKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceAVMedia>();
            this.ResourceAVMediaKey.ResourceChanged += this.OnResourceChanged;
        }

        public override Vector2? GetRenderSize() {
            if (this.ResourceAVMediaKey.TryGetResource(out ResourceAVMedia resource) && resource.GetResolution() is Vec2i size) {
                return new Vector2(size.X, size.Y);
            }

            return null;
        }

        private void OnResourceChanged(IResourcePathKey<ResourceAVMedia> key, ResourceAVMedia olditem, ResourceAVMedia newitem) {
            this.renderFrameRgb?.Dispose();
            this.renderFrameRgb = null;
            this.downloadedHwFrame?.Dispose();
            this.downloadedHwFrame = null;
            unsafe {
                if (this.scaler != null) {
                    ffmpeg.sws_freeContext(this.scaler);
                    this.scaler = null;
                }
            }
        }

        public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
            if (!this.ResourceAVMediaKey.TryGetResource(out ResourceAVMedia resource))
                return false;
            if (resource.stream == null || resource.Demuxer == null)
                return false;

            if (frame == this.currentFrame && this.renderFrameRgb != null) {
                return true;
            }

            if (this.renderFrameRgb == null) {
                unsafe {
                    AVCodecParameters* pars = resource.stream.Handle->codecpar;
                    this.renderFrameRgb = new VideoFrame(pars->width, pars->height, PixelFormats.RGBA);
                }
            }

            // TimeSpan timestamp = TimeSpan.FromTicks((long) ((frame - this.MediaFrameOffset) * this.targetFrameIntervalTicks));
            TimeSpan timestamp = TimeSpan.FromSeconds((frame - this.MediaFrameOffset) / this.Project.Settings.FrameRate.AsDouble);

            // No need to dispose as the frames are stored in a frame buffer, which is disposed by the resource itself
            this.currentFrame = frame;
            this.decodeFrameTask = Task.Run(() => {
                this.decodeFrameBegin = Time.GetSystemTicks();
                VideoFrame output = null;
                VideoFrame ready = resource.GetFrameAt(timestamp, out _);
                if (ready != null && !ready.IsDisposed) {
                    // TODO: Maybe add an async frame fetcher that buffers the frames, or maybe add
                    // a project preview resolution so that decoding is lightning fast for low resolution?
                    if (ready.IsHardwareFrame) {
                        // As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                        // which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                        // hacking into the API specific device context (like D3D11VA).
                        ready.TransferTo(this.downloadedHwFrame ?? (this.downloadedHwFrame = new VideoFrame()));
                        ready = this.downloadedHwFrame;
                    }

                    this.ScaleFrame(ready);
                    output = ready;
                }

                this.lastDecodeFrameDuration = Time.GetSystemTicks() - this.decodeFrameBegin;
                return output;
            });

            return true;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            VideoFrame ready;
            if (this.decodeFrameTask != null) {
                this.lastReadyFrame = ready = this.decodeFrameTask.Result;
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
                long startA = Time.GetSystemTicks();
                byte* ptr;
                GetFrameData(this.renderFrameRgb, 0, &ptr, out int rowBytes);
                SKImageInfo image = new SKImageInfo(this.renderFrameRgb.Width, this.renderFrameRgb.Height, SKColorType.Rgba8888);
                using (SKImage img = SKImage.FromPixels(image, (IntPtr) ptr, rowBytes)) {
                    if (img == null) {
                        return;
                    }

                    using (SKPaint paint = new SKPaint() {FilterQuality = rc.FilterQuality, ColorF = new SKColorF(1f, 1f, 1f, (float) this.RenderOpacity)}) {
                        rc.Canvas.DrawImage(img, 0, 0, paint);
                    }

                    renderArea = rc.TranslateRect(new SKRect(0, 0, img.Width, img.Height));
                    // renderArea = new SKRect(0, 0, img.Width, img.Height);
                }

                long time = Time.GetSystemTicks() - startA;
                // System.Diagnostics.Debug.WriteLine("Last render time: " + Math.Round((double) time) / Time.TICK_PER_MILLIS + " ms");
            }
        }

        private unsafe void ScaleFrame(VideoFrame ready) {
            if (this.scaler == null) {
                PictureFormat srcfmt = ready.Format;
                PictureFormat dstfmt = this.renderFrameRgb.Format;
                this.scalerInputFormat = srcfmt;
                this.scaler = ffmpeg.sws_getContext(srcfmt.Width, srcfmt.Height, srcfmt.PixelFormat, dstfmt.Width, dstfmt.Height, dstfmt.PixelFormat, ffmpeg.SWS_BICUBIC, null, null, null);
            }

            AVFrame* src = ready.Handle;
            AVFrame* dst = this.renderFrameRgb.Handle;
            ffmpeg.sws_scale(this.scaler, src->data, src->linesize, 0, this.scalerInputFormat.Height, dst->data, dst->linesize);
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
}