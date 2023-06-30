using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FFmpeg.AutoGen;
using FFmpeg.Wrapper;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Exporting.Exporters.FFMPEG {
    public static class FFUtils {
        // "Resolve" function info can be found at:
        // https://ffmpeg.org/ffmpeg-utils.html#Time-duration

        public static bool ResolveRate(string input, out Rational rational) {
            switch (input.ToLower()) {
                case "ntsc-film": rational = new Rational(24000, 1001); break;
                case "film":      rational = new Rational(24, 1); break;
                case "pal":
                case "qpal":
                case "spal":      rational = new Rational(25, 1); break;
                case "ntsc":
                case "qntsc":
                case "sntsc":     rational = new Rational(30000, 1001); break;
                default:
                    rational = default;
                    return false;
            }

            return true;
        }

        public static bool ResolveResolution(string input, out Resolution res) {
            switch (input.ToLower()) {
                case "ntsc":      res = new Resolution(720, 480); break;
                case "pal":       res = new Resolution(720, 576); break;
                case "qntsc":     res = new Resolution(352, 240); break;
                case "qpal":      res = new Resolution(352, 288); break;
                case "sntsc":     res = new Resolution(640, 480); break;
                case "spal":      res = new Resolution(768, 576); break;
                case "film":      res = new Resolution(352, 240); break;
                case "ntsc-film": res = new Resolution(352, 240); break;
                case "sqcif":     res = new Resolution(128, 96); break;
                case "qcif":      res = new Resolution(176, 144); break;
                case "cif":       res = new Resolution(352, 288); break;
                case "4cif":      res = new Resolution(704, 576); break;
                case "16cif":     res = new Resolution(1408, 1152); break;
                case "qqvga":     res = new Resolution(160, 120); break;
                case "qvga":      res = new Resolution(320, 240); break;
                case "vga":       res = new Resolution(640, 480); break;
                case "svga":      res = new Resolution(800, 600); break;
                case "xga":       res = new Resolution(1024, 768); break;
                case "uxga":      res = new Resolution(1600, 1200); break;
                case "qxga":      res = new Resolution(2048, 1536); break;
                case "sxga":      res = new Resolution(1280, 1024); break;
                case "qsxga":     res = new Resolution(2560, 2048); break;
                case "hsxga":     res = new Resolution(5120, 4096); break;
                case "wvga":      res = new Resolution(852, 480); break;
                case "wxga":      res = new Resolution(1366, 768); break;
                case "wsxga":     res = new Resolution(1600, 1024); break;
                case "wuxga":     res = new Resolution(1920, 1200); break;
                case "woxga":     res = new Resolution(2560, 1600); break;
                case "wqsxga":    res = new Resolution(3200, 2048); break;
                case "wquxga":    res = new Resolution(3840, 2400); break;
                case "whsxga":    res = new Resolution(6400, 4096); break;
                case "whuxga":    res = new Resolution(7680, 4800); break;
                case "cga":       res = new Resolution(320, 200); break;
                case "ega":       res = new Resolution(640, 350); break;
                case "hd480":     res = new Resolution(852, 480); break;
                case "hd720":     res = new Resolution(1280, 720); break;
                case "hd1080":    res = new Resolution(1920, 1080); break;
                case "2k":        res = new Resolution(2048, 1080); break;
                case "2kflat":    res = new Resolution(1998, 1080); break;
                case "2kscope":   res = new Resolution(2048, 858); break;
                case "4k":        res = new Resolution(4096, 2160); break;
                case "4kflat":    res = new Resolution(3996, 2160); break;
                case "4kscope":   res = new Resolution(4096, 1716); break;
                case "nhd":       res = new Resolution(640, 360); break;
                case "hqvga":     res = new Resolution(240, 160); break;
                case "wqvga":     res = new Resolution(400, 240); break;
                case "fwqvga":    res = new Resolution(432, 240); break;
                case "hvga":      res = new Resolution(480, 320); break;
                case "qhd":       res = new Resolution(960, 540); break;
                case "2kdci":     res = new Resolution(2048, 1080); break;
                case "4kdci":     res = new Resolution(4096, 2160); break;
                case "uhd2160":   res = new Resolution(3840, 2160); break;
                case "uhd4320":   res = new Resolution(7680, 4320); break;
                default:
                    res = default;
                    return false;
            }

            return true;
        }

        /* check that a given sample format is supported by the encoder */
        static unsafe int check_sample_fmt(AVCodec* codec, AVSampleFormat sample_fmt) {
            AVSampleFormat* p = codec->sample_fmts;
            while (*p != AVSampleFormat.AV_SAMPLE_FMT_NONE) {
                if (*p == sample_fmt)
                    return 1;
                p++;
            }

            return 0;
        }

        /* just pick the highest supported samplerate */
        static unsafe int select_sample_rate(AVCodec* codec) {
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
        static unsafe ulong select_channel_layout(AVCodec* codec) {
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

        static unsafe void video_encode_example(string filename, AVCodecID codec_id) {
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