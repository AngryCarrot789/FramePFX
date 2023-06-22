using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FFmpeg.Wrapper;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Exporting.Exporters.FFMPEG {

    // https://github.com/aligrudi/fbff/blob/master/ffs.c

    public class FFmpegExporter : ExportService {
        public Resolution Resolution { get; set; }

        public Rational FrameRate { get; set; }

        public FFmpegExporter() {

        }

        public override unsafe void Export(ProjectModel project, IExportProgress progress, ExportProperties properties) {
            FrameSpan duration = properties.Span;
            Resolution resolution = this.Resolution;
            Rational frameRate = this.FrameRate;
            SKImageInfo frameInfo = new SKImageInfo(resolution.Width, resolution.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            PictureFormat format = new PictureFormat(frameInfo.Width, frameInfo.Height, AVPixelFormat.AV_PIX_FMT_YUV420P);
            using (SKSurface surface = SKSurface.Create(frameInfo)) {
                if (surface == null) {
                    throw new Exception("Failed to create SKSurface");
                }

                AVFormatContext* outputContext = null;
                ffmpeg.avformat_alloc_output_context2(&outputContext, null, null, properties.FilePath);
                AVCodec* videoCodec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
                AVCodecContext* codecContext = ffmpeg.avcodec_alloc_context3(videoCodec);
                codecContext->width = frameInfo.Width;
                codecContext->height = frameInfo.Height;
                codecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
                codecContext->framerate = frameRate;
                ffmpeg.avcodec_open2(codecContext, videoCodec, null);
                ffmpeg.avio_open(&outputContext->pb, "output.mp4", ffmpeg.AVIO_FLAG_WRITE);
                ffmpeg.avformat_write_header(outputContext, null);

                AVPacket packet = default;
                using (VideoFrame videoFrame = new VideoFrame(format)) {

                    AVFrame frameYuv = new AVFrame {
                        format = (int) AVPixelFormat.AV_PIX_FMT_YUV420P, width = frameInfo.Width, height = frameInfo.Height
                    };

                    int frameSize = ffmpeg.av_image_get_buffer_size(AVPixelFormat.AV_PIX_FMT_YUV420P, frameYuv.width, frameYuv.height, 1);
                    IntPtr frameBuffer = Marshal.AllocHGlobal(frameSize);

                    byte_ptrArray4 dst_data = new byte_ptrArray4();
                    dst_data[0] = (byte*) frameBuffer;
                    dst_data[1] = (byte*) IntPtr.Zero;
                    dst_data[2] = (byte*) IntPtr.Zero;
                    dst_data[3] = (byte*) IntPtr.Zero;

                    int_array4 linesizes = new int_array4();

                    ffmpeg.av_image_fill_arrays(ref dst_data, ref linesizes, (byte*)frameBuffer, AVPixelFormat.AV_PIX_FMT_YUV420P, frameInfo.Width, frameInfo.Height, 1);

                    RenderContext render_context = new RenderContext(surface, surface.Canvas, frameInfo);
                    for (long fidx = duration.Begin, end = duration.EndIndex; fidx < end; fidx++) {
                        render_context.Canvas.Clear(SKColors.Black);
                        project.AutomationEngine.TickProjectAtFrame(fidx);
                        project.Timeline.Render(render_context, fidx);
                        surface.Flush();

                        using (var pixmap = surface.PeekPixels()) {
                            byte* data = (byte*) pixmap.GetPixels();

                            byte_ptrArray4 srcSlice = new byte_ptrArray4();
                            srcSlice[0] = data;

                            SwsContext* swsContext = ffmpeg.sws_getContext(
                                pixmap.Width, pixmap.Height, AVPixelFormat.AV_PIX_FMT_RGBA,
                                frameInfo.Width, frameInfo.Height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                                ffmpeg.SWS_BILINEAR, null, null, null);

                            ffmpeg.sws_scale(swsContext, srcSlice, new[] {frameInfo.RowBytes}, 0, pixmap.Height, dst_data, linesizes);

                            ffmpeg.sws_freeContext(swsContext);
                        }

                        AVFrame* frame = videoFrame.Handle;
                        ffmpeg.avcodec_send_frame(codecContext, frame);
                        ffmpeg.avcodec_receive_packet(codecContext, &packet);

                        ffmpeg.av_interleaved_write_frame(outputContext, &packet);
                        ffmpeg.av_packet_unref(&packet);

                        videoFrame.Clear();

                        progress.OnFrameCompleted(fidx);
                    }
                }

                ffmpeg.av_write_trailer(outputContext);
                ffmpeg.avcodec_close(codecContext);
                ffmpeg.avformat_free_context(outputContext);
            }
            // flush and close FFmpeg streams, export complete... i guess?
        }

        // public override unsafe void Export(ProjectModel project, IExportProgress progress, ExportProperties properties) {
        //     FrameSpan duration = properties.Span;
        //     Resolution resolution = this.Resolution;
        //     Rational frameRate = this.FrameRate;
        //     // platform color type is typically SKColorType.Rgba8888... i think?
        //     SKImageInfo frameInfo = new SKImageInfo(resolution.Width, resolution.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        //
        //     FFS ffs = new FFS();
        //     FFS.AllocateFFS(&ffs, properties.FilePath);
        //
        //     PictureFormat format = new PictureFormat(frameInfo.Width, frameInfo.Height, AVPixelFormat.AV_PIX_FMT_RGBA);
        //     VideoEncoder encoder = new VideoEncoder(ffs.cc->codec, in format, frameRate.FPS, 5000);
        //     using (SKSurface surface = SKSurface.Create(frameInfo)) {
        //         if (surface == null) {
        //             throw new Exception("Failed to create SKSurface");
        //         }
        //
        //         using (VideoFrame videoFrame = new VideoFrame(ffs.dst, true, false)) {
        //             ffmpeg.avformat_alloc_output_context2(out AVFormatContext output, null, "", "";
        //
        //             RenderContext context = new RenderContext(surface, surface.Canvas, frameInfo);
        //             for (long frame = duration.Begin, end = duration.EndIndex; frame < end; frame++) {
        //                 context.Canvas.Clear(SKColors.Black);
        //                 project.AutomationEngine.TickProjectAtFrame(frame);
        //                 project.Timeline.Render(context, frame);
        //                 progress.OnFrameCompleted(frame);
        //                 surface.Flush();
        //
        //
        //                 // pass pixels to FFmpeg and encode
        //             }
        //         }
        //     }
        //
        //     encoder.Flush();
        //     // flush and close FFmpeg streams, export complete... i guess?
        // }
    }
}