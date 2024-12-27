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

using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using Fractions;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines;
using FramePFX.FFmpegWrapper;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Plugins.FFmpegMedia.Exporter;

public class FFmpegExportContext : BaseExportContext {
    public AVCodecID Codec { get; }

    public new FFmpegExporterInfo Exporter => (FFmpegExporterInfo) base.Exporter;

    public FFmpegExportContext(FFmpegExporterInfo exporter, ExportSetup setup) : base(exporter, setup) {
        this.Codec = exporter.CodecId;
    }

    public static long RNDTO2(long X) => (X) & 0xFFFFFFFE;
    public static long RNDTO32(long X) => (X % 32) != 0 ? ((X + 32) & 0xFFFFFFE0) : X;

    public static int RNDTO2(int X) => (int) ((X) & 0xFFFFFFFE);
    public static int RNDTO32(int X) => (int) ((X % 32) != 0 ? ((X + 32) & 0xFFFFFFE0) : X);

    public override unsafe void Export(IExportProgress progress, CancellationToken cancellation) {
        Task? renderTask = null;
        bool isRenderCancelled = false;
        FrameSpan duration = this.Setup.Span;
        SKSizeI resolution = this.Setup.Project.Settings.Resolution;
        Fraction frameRate = this.Setup.Project.Settings.FrameRate;
        AVCodecID codec_id = this.Codec;
        Exception? exception = null;

        AVFormatContext* oc = null; // output context
        AVCodec* codec; // idk codec i guess
        AVCodecContext* c = null; // idk
        AVFrame* videoFrame = null; // raw data
        AVFrame* audioFrame = null; // raw data
        AVPacket* pkt = null; // encoded data
        int ret;

        // initialize the AVFormatContext
        if ((ret = ffmpeg.avformat_alloc_output_context2(&oc, null, null, this.Setup.FilePath)) < 0) {
            exception = new Exception("Could not allocate output format context: " + ret);
            goto fail_or_end;
        }

        // set the output format for the AVFormatContext
        if (oc->oformat == null) {
            // Find the output format based on the file extension (e.g., ".mp4")
            oc->oformat = ffmpeg.av_guess_format(null, this.Setup.FilePath, null);
            if (oc->oformat == null) {
                exception = new Exception("Could not find output format");
                goto fail_or_end;
            }
        }

        /* find the mpeg1 video encoder */
        codec = ffmpeg.avcodec_find_encoder(codec_id);
        if (codec == null) {
            exception = new Exception("Codec not found");
            goto fail_or_end;
        }

        // Add a video stream to the output format
        AVStream* st = ffmpeg.avformat_new_stream(oc, null);
        if (st == null) {
            exception = new Exception("Could not create video stream");
            goto fail_or_end;
        }

        st->id = (int) (oc->nb_streams - 1);
        c = ffmpeg.avcodec_alloc_context3(codec);
        if (c == null) {
            exception = new Exception("Could not allocate video codec context");
            goto fail_or_end;
        }

        Fraction inverseFps = frameRate.Negate();
        if (!ExceptionUtils.TryExecute(inverseFps.Numerator, (b) => (int) b, out int num)) {
            exception = new Exception("Invalid FPS numerator");
            goto fail_or_end;
        }
        
        if (!ExceptionUtils.TryExecute(inverseFps.Denominator, (b) => (int) b, out int den)) {
            exception = new Exception("Invalid FPS denominator");
            goto fail_or_end;
        }

        c->bit_rate = this.Exporter.BitRate;
        c->width = resolution.Width;
        c->height = resolution.Height;
        c->time_base = new AVRational() { num = num, den = den };
        st->time_base = c->time_base;
        c->gop_size = (int) this.Exporter.Gop;
        c->max_b_frames = 1;
        // c->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
        c->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
        c->codec_id = codec_id;
        if (codec_id == AVCodecID.AV_CODEC_ID_H264) {
            ffmpeg.av_opt_set(c->priv_data, "preset", "slow", 0);
        }

        if ((oc->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) != 0)
            c->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;

        if (c->codec_id == AVCodecID.AV_CODEC_ID_MPEG2VIDEO)
            c->max_b_frames = 2; /* just for testing, we also add B-frames */

        if (c->codec_id == AVCodecID.AV_CODEC_ID_MPEG1VIDEO) {
            /* Needed to avoid using macroblocks in which some coeffs overflow.
             * This does not happen with normal video, it just happens here as
             * the motion of the chroma plane does not match the luma plane. */
            c->mb_decision = 2;
        }

        AVDictionary* opt = null;
        ret = ffmpeg.avcodec_open2(c, codec, &opt);
        ffmpeg.av_dict_free(&opt);
        if (ret < 0) {
            exception = new Exception("Could not open codec");
            goto fail_or_end;
        }

        videoFrame = ffmpeg.av_frame_alloc();
        if (videoFrame == null) {
            exception = new Exception("Could not allocate video frame");
            goto fail_or_end;
        }


        videoFrame->format = (int) c->pix_fmt;
        videoFrame->width = c->width;
        videoFrame->height = c->height;

        byte_ptrArray4 frame_data_arrays = new byte_ptrArray4();
        int_array4 frame_line_sizes = new int_array4();
        ret = ffmpeg.av_image_alloc(ref frame_data_arrays, ref frame_line_sizes, c->width, c->height, c->pix_fmt, 32);
        if (ret < 0) {
            exception = new Exception("Could not allocate raw picture buffer");
            goto fail_or_end;
        }

        videoFrame->data[0] = frame_data_arrays[0];
        videoFrame->data[1] = frame_data_arrays[1];
        videoFrame->data[2] = frame_data_arrays[2];
        videoFrame->data[3] = frame_data_arrays[3];
        videoFrame->linesize[0] = frame_line_sizes[0];
        videoFrame->linesize[1] = frame_line_sizes[1];
        videoFrame->linesize[2] = frame_line_sizes[2];
        videoFrame->linesize[3] = frame_line_sizes[3];

        // audioFrame = ffmpeg.av_frame_alloc();
        // if (audioFrame == null) {
        //     exception = new Exception("Could not allocate audio frame");
        //     goto fail_or_end;
        // }
        // audioFrame->format = (int) c->sample_fmt;
        // audioFrame->nb_samples = (int) c->sample_fmt;

        ret = ffmpeg.avcodec_parameters_from_context(st->codecpar, c);
        if (ret < 0) {
            exception = new Exception("Could not copy the stream parameters");
            goto fail_or_end;
        }

        ffmpeg.av_dump_format(oc, 0, this.Setup.FilePath, 1);

        // Open the output file
        if ((oc->oformat->flags & ffmpeg.AVFMT_NOFILE) == 0) {
            if ((ret = ffmpeg.avio_open(&oc->pb, this.Setup.FilePath, ffmpeg.AVIO_FLAG_WRITE)) < 0) {
                exception = new Exception("Could not open output file");
                goto fail_or_end;
            }
        }

        oc->video_codec = codec;
        oc->video_codec_id = codec_id;

        // Write the stream header
        if ((ret = ffmpeg.avformat_write_header(oc, &opt)) < 0) {
            exception = new Exception("Error writing stream header: " + ret);
            goto fail_or_end;
        }

        if (cancellation.IsCancellationRequested) {
            goto fail_or_end;
        }

        SKImageInfo imgInfo = new SKImageInfo(resolution.Width, resolution.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        SKBitmap bitmap = new SKBitmap(imgInfo);
        IntPtr ptr = bitmap.GetAddress(0, 0);
        SKPixmap pixmap = new SKPixmap(imgInfo, ptr, imgInfo.RowBytes);
        SKSurface surface = null;
        SuspendRenderToken? suspendRender = null;
        SwsContext* s = null;
        try {
            surface = SKSurface.Create(pixmap);
            if (surface == null) {
                throw new Exception("Failed to create SKSurface");
            }

            long exportFrame = duration.Begin; // frame index, relative to export duration
            long ptsFrame = 0; // frame index, relative to start of file
            long frameEnd = duration.EndIndex;

            Timeline timeline = this.Setup.Timeline;
            RenderManager renderManager = timeline.RenderManager;
            suspendRender = renderManager.SuspendRenderInvalidation();

            int width = RNDTO2(c->width);
            int height = RNDTO2(c->height);
            s = ffmpeg.sws_getContext(
                resolution.Width, resolution.Height, AVPixelFormat.AV_PIX_FMT_BGRA,
                width, height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                ffmpeg.SWS_BILINEAR, null, null, null);

            IDispatcher dispatcher = Application.Instance.Dispatcher;
            for (; exportFrame < frameEnd; exportFrame++, ptsFrame++) {
                if (cancellation.IsCancellationRequested) {
                    isRenderCancelled = true;
                    goto fail_or_end;
                }

                try {
                    long finalExportFrame = exportFrame;
                    renderTask = dispatcher.Invoke(() => {
                        AutomationEngine.UpdateValues(timeline, finalExportFrame);
                        return renderManager.RenderTimelineAsync(finalExportFrame, cancellation, EnumRenderQuality.High);
                    });

                    surface.Canvas.Clear(SKColors.Black);
                    renderTask.Wait(cancellation);
                    // while (!renderTask.IsCompleted) {
                    //     Thread.Sleep(1);
                    // }

                    renderManager.Draw(surface);
                }
                catch (TaskCanceledException) {
                    isRenderCancelled = true;
                    goto fail_or_end;
                }
                catch (OperationCanceledException) {
                    isRenderCancelled = true;
                    goto fail_or_end;
                }
                catch (Exception e) {
                    if (renderTask != null && renderTask.IsCanceled) {
                        isRenderCancelled = true;
                        goto fail_or_end;
                    }

                    Debugger.Break();
                    Debug.Assert(false);
                    // AppLogger.Instance.WriteLine("Exception while rendering project timeline: " + e.GetToString());
                }

                surface.Flush();
                byte* data = (byte*) pixmap.GetPixels();
                int stride = pixmap.RowBytes;
                {
                    // int width = RNDTO2(c->width);
                    // int height = RNDTO2(c->height);
                    // SwsContext* s = ffmpeg.sws_getContext(
                    //     resolution.Width, resolution.Height, AVPixelFormat.AV_PIX_FMT_BGRA,
                    //     width, height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                    //     ffmpeg.SWS_BILINEAR, null, null, null);

                    ffmpeg.sws_scale(s, new[] { data }, new[] { stride }, 0, c->height, frame_data_arrays, frame_line_sizes);
                    // ffmpeg.sws_freeContext(s);
                }

                videoFrame->pts = ptsFrame;

                // Encode frame
                if ((ret = ffmpeg.avcodec_send_frame(c, videoFrame)) == 0) {
                    pkt = ffmpeg.av_packet_alloc();
                    // ffmpeg.av_init_packet(&pkt);
                    ret = ffmpeg.avcodec_receive_packet(c, pkt);
                    if (ret != ffmpeg.AVERROR(ffmpeg.EAGAIN) && ret != ffmpeg.AVERROR_EOF) {
                        if (ret < 0) {
                            exception = new Exception("Error encoding");
                            goto fail_or_end;
                        }
                    }

                    // when ret == 0, a packet was decoded successfully (not necessarily the one we just send, in fact,
                    // not likely at all... encoding takes a bit longer than a few microseconds lol)
                    if (ret == 0) {
                        // required, otherwise the .mp4 file is like 20 milliseconds long
                        ffmpeg.av_packet_rescale_ts(pkt, c->time_base, st->time_base);
                        pkt->stream_index = st->index;

                        long ts = pkt->dts;
                        if (ts != ffmpeg.AV_NOPTS_VALUE) {
                            progress.OnFrameEncoded(ts);
                        }

                        // write encoded frame
                        ret = ffmpeg.av_interleaved_write_frame(oc, pkt);
                        // ffmpeg.av_packet_unref(&pkt);
                        if (ret < 0) {
                            exception = new Exception("Error writing frame to stream: " + ret);
                            goto fail_or_end;
                        }
                    }

                    ffmpeg.av_packet_free(&pkt);
                }
                else if (ret < 0) {
                    exception = new Exception("Error sending/encoding frame");
                    goto fail_or_end;
                }

                progress.OnFrameRendered(exportFrame);
            }

            // begin flush run
            ffmpeg.avcodec_send_frame(c, null);
            while (true) {
                pkt = ffmpeg.av_packet_alloc();
                // ffmpeg.av_init_packet(&pkt);
                ret = ffmpeg.avcodec_receive_packet(c, pkt);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF) // all frames fully encoded
                    break;

                ffmpeg.av_packet_rescale_ts(pkt, c->time_base, st->time_base);
                pkt->stream_index = st->index;

                long ts = pkt->dts;
                if (ts != ffmpeg.AV_NOPTS_VALUE)
                    progress.OnFrameEncoded(ts);

                // write encoded frame
                ret = ffmpeg.av_interleaved_write_frame(oc, pkt);
                // ffmpeg.av_packet_unref(&pkt);
                ffmpeg.av_packet_free(&pkt);
                if (ret < 0) {
                    exception = new Exception("Error writing frame to stream: " + ret);
                    goto fail_or_end;
                }
            }
        }
        finally {
            suspendRender?.Dispose();
            bitmap.Dispose();
            pixmap.Dispose();
            surface?.Dispose();
            if (s != null)
                ffmpeg.sws_freeContext(s);
        }

        ffmpeg.av_write_trailer(oc);
        fail_or_end:

        if (pkt != null) {
            ffmpeg.av_packet_free(&pkt);
        }

        if (oc != null) {
            if ((oc->oformat->flags & ffmpeg.AVFMT_NOFILE) == 0) {
                ffmpeg.avio_close(oc->pb);
            }

            ffmpeg.avformat_free_context(oc);
        }

        if (c != null) {
            ffmpeg.avcodec_close(c);
            ffmpeg.av_free(c);
        }

        if (videoFrame != null) {
            ffmpeg.av_frame_free(&videoFrame);
            // ffmpeg.av_frame_unref(frame);
        }

        if (audioFrame != null) {
            ffmpeg.av_frame_free(&audioFrame);
            // ffmpeg.av_frame_unref(frame);
        }

        if (exception != null) {
            byte[] buffer = new byte[4096]; // i highly doubt stackalloc would cause a stack overflow... but ya never know
            fixed (byte* strbuf = buffer) {
                ffmpeg.av_make_error_string(strbuf, 4096, ret);
                int i = 0;
                for (; i < 4096; i++) {
                    if (strbuf[i] == 0) {
                        break;
                    }
                }

                throw new Exception($"Exception exporting. Last ret = {(LavResult) ret} ({ret}) ({Marshal.PtrToStringAnsi((IntPtr) strbuf, i)})", exception);
            }
        }

        if (isRenderCancelled) {
            if (renderTask != null) {
                throw new TaskCanceledException(renderTask);
            }
            else {
                throw new TaskCanceledException("Export cancelled before rendering could begin");
            }
        }
    }
}