using System;
using System.IO;
using FFmpeg.AutoGen;
using FFmpeg.Wrapper;

namespace FramePFX.Core.Editor.Exporting.Exporters.FFMPEG {
    public static unsafe class FFUtils {
        /* check that a given sample format is supported by the encoder */
        static int check_sample_fmt(AVCodec* codec, AVSampleFormat sample_fmt) {
            AVSampleFormat* p = codec->sample_fmts;
            while (*p != AVSampleFormat.AV_SAMPLE_FMT_NONE) {
                if (*p == sample_fmt)
                    return 1;
                p++;
            }

            return 0;
        }

        /* just pick the highest supported samplerate */
        static int select_sample_rate(AVCodec* codec) {
            int best_samplerate = 0;

            if (codec->supported_samplerates == null)
                return 44100;

            int* p = codec->supported_samplerates;
            while (*p != 0) {
                // p is terminated with 0
                best_samplerate = Math.Max(*p, best_samplerate);
                p++;
            }

            return best_samplerate;
        }

        /* select layout with the highest channel count */
        static ulong select_channel_layout(AVCodec* codec) {
            ulong* p;
            ulong best_ch_layout = 0;
            int best_nb_channells = 0;

            if (codec->channel_layouts == null)
                return ffmpeg.AV_CH_LAYOUT_STEREO;

            p = codec->channel_layouts;
            while (*p != 0) {
                int nb_channels = ffmpeg.av_get_channel_layout_nb_channels(*p);

                if (nb_channels > best_nb_channells) {
                    best_ch_layout = *p;
                    best_nb_channells = nb_channels;
                }

                p++;
            }

            return best_ch_layout;
        }

        static void video_encode_example(string filename, AVCodecID codec_id) {
            Exception exception = null;
            AVCodec* codec;
            AVCodecContext* c = null;
            int i, ret, ret1, x, y, got_output;
            AVFrame* frame = null;
            AVPacket pkt;
            FileStream f = File.OpenWrite(filename);
            byte[] endcode = {
                0, 0, 1, 0xb7
            };

            /* find the mpeg1 video encoder */
            codec = ffmpeg.avcodec_find_encoder(codec_id);
            if (codec == null) {
                exception = new Exception("Codec not found");
                goto fail_or_end;
            }

            c = ffmpeg.avcodec_alloc_context3(codec);
            if (c == null) {
                exception = new Exception("Could not allocate video codec context");
                goto fail_or_end;
            }

            /* put sample parameters */
            c->bit_rate = 400000;
            /* resolution must be a multiple of two */
            c->width = 352;
            c->height = 288;
            /* frames per second */
            c->time_base = new Rational(25, 1);
            c->gop_size = 10; /* emit one intra frame every ten frames */
            c->max_b_frames = 1;
            c->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;

            if (codec_id == AVCodecID.AV_CODEC_ID_H264) {
                ffmpeg.av_opt_set(c->priv_data, "preset", "slow", 0);
            }

            /* open it */
            if (ffmpeg.avcodec_open2(c, codec, null) < 0) {
                exception = new Exception("Could not open codec");
                goto fail_or_end;
            }

            frame = ffmpeg.av_frame_alloc();
            if (frame == null) {
                exception = new Exception("Could not allocate video frame");
                goto fail_or_end;
            }

            frame->format = (int) c->pix_fmt;
            frame->width = c->width;
            frame->height = c->height;

            /* the image can be allocated by any means and av_image_alloc() is
             * just the most convenient way if av_malloc() is to be used */
            byte_ptrArray4 pointers = new byte_ptrArray4 {[0] = frame->data[0], [1] = frame->data[1], [2] = frame->data[2], [3] = frame->data[3]};
            int_array4 linesizes = new int_array4 {[0] = frame->linesize[0], [1] = frame->linesize[1], [2] = frame->linesize[2], [3] = frame->linesize[3]};
            ret = ffmpeg.av_image_alloc(ref pointers, ref linesizes, c->width, c->height, c->pix_fmt, 32);
            if (ret < 0) {
                exception = new Exception("Could not allocate raw picture buffer");
                goto fail_or_end;
            }

            /* encode 1 second of video */
            for (i = 0; i < 25; i++) {
                ffmpeg.av_init_packet(&pkt);
                pkt.data = null; // packet data will be allocated by the encoder
                pkt.size = 0;

                /* prepare a dummy image */
                /* Y */
                for (y = 0; y < c->height; y++) {
                    for (x = 0; x < c->width; x++) {
                        frame->data[0][y * frame->linesize[0] + x] = (byte) (x + y + i * 3);
                    }
                }

                /* Cb and Cr */
                for (y = 0; y < c->height / 2; y++) {
                    for (x = 0; x < c->width / 2; x++) {
                        frame->data[1][y * frame->linesize[1] + x] = (byte) (128 + y + i * 2);
                        frame->data[2][y * frame->linesize[2] + x] = (byte) (64 + x + i * 5);
                    }
                }

                frame->pts = i;

                /* encode the image */
                ret = ffmpeg.avcodec_send_frame(c, frame);
                if (ret < 0) {
                    exception = new Exception("Error sending/encoding frame");
                    goto fail_or_end;
                }

                if (ret == 0) {
                    got_output = ffmpeg.avcodec_receive_packet(c, &pkt);
                    if (got_output == ffmpeg.AVERROR(ffmpeg.EAGAIN)) {
                        exception = new Exception("Error receiving encoded packet: EAGAIN");
                        goto fail_or_end;
                    }
                    else if (got_output == ffmpeg.AVERROR_EOF) {
                        exception = new Exception("Error receiving encoded: EOF");
                        goto fail_or_end;
                    }
                    else if (got_output < 0) {
                        exception = new Exception("Error encoding");
                        goto fail_or_end;
                    }

                    // ffmpeg.av_interleaved_write_frame(c, &pkt);

                    // printf("Write frame %3d (size=%5d)", i, pkt.size);
                    f.Write(new Span<byte>(pkt.data, pkt.size).ToArray(), 0, pkt.size);
                    ffmpeg.av_packet_unref(&pkt);
                }
            }

            /* get the delayed frames */
            for (; ret == 0; i++) {
                /* encode the image */
                ret = ffmpeg.avcodec_send_frame(c, null);
                if (ret < 0) {
                    exception = new Exception("Error sending/encoding frame");
                    goto fail_or_end;
                }

                if (ret == 0) {
                    got_output = ffmpeg.avcodec_receive_packet(c, &pkt);
                    if (got_output == ffmpeg.AVERROR(ffmpeg.EAGAIN)) {
                        exception = new Exception("Error receiving encoded packet: EAGAIN");
                        goto fail_or_end;
                    }
                    else if (got_output == ffmpeg.AVERROR_EOF) {
                        exception = new Exception("Error receiving encoded: EOF");
                        goto fail_or_end;
                    }
                    else if (got_output < 0) {
                        exception = new Exception("Error encoding");
                        goto fail_or_end;
                    }

                    f.Write(new Span<byte>(pkt.data, pkt.size).ToArray(), 0, pkt.size);
                    ffmpeg.av_packet_unref(&pkt);
                }
            }

            f.Write(endcode, 0, endcode.Length);

            fail_or_end:

            f.Close();

            /* add sequence end code to have a real mpeg file */

            ffmpeg.avcodec_close(c);
            ffmpeg.av_free(c);
            if (frame != null) {
                ffmpeg.av_freep(frame->data[0]);
                ffmpeg.av_frame_unref(frame);
            }

            if (exception != null) {
                throw exception;
            }
        }
    }
}