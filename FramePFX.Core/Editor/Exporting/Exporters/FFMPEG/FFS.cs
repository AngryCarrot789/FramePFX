using FFmpeg.AutoGen;

namespace FramePFX.Core.Editor.Exporting.Exporters.FFMPEG {
    public unsafe struct FFS {
        public AVCodecContext* cc;
        public AVFormatContext* fc;
        public AVOutputFormat* of;
        public AVStream* st;
        public AVPacket pkt; /* used in ffs_pkt() */
        public int si; /* stream index */
        public long ts; /* frame timestamp (ms) */
        public long pts; /* last decoded packet pts in milliseconds */
        public long dur; /* last decoded packet duration */
        public SwsContext* swsc;
        public SwrContext* swrc;
        public AVFrame* dst;
        public AVFrame* tmp;

        public static void AllocateFFS(FFS* ffs, string filePath) {
            int err;
            AVDictionary* opt = null;
            AVCodec* enc = null;
            ffs->si = -1;
            if ((err = ffmpeg.avformat_alloc_output_context2(&ffs->fc, ffs->of, null, filePath)) < 0)
                goto failed;
            if ((err = ffmpeg.avformat_find_stream_info(ffs->fc, null)) < 0)
                goto failed;
            ffs->si = ffmpeg.av_find_best_stream(ffs->fc, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0);
            if ((err = ffs->si) < 0)
                goto failed;
            enc = ffmpeg.avcodec_find_encoder(ffs->fc->streams[ffs->si]->codecpar->codec_id);
            if (enc == null)
                goto failed;
            ffs->cc = ffmpeg.avcodec_alloc_context3(enc);
            if (ffs->cc == null)
                goto failed;
            ffmpeg.avcodec_parameters_to_context(ffs->cc, ffs->fc->streams[ffs->si]->codecpar);
            if ((err = ffmpeg.avcodec_open2(ffs->cc, ffmpeg.avcodec_find_encoder(ffs->cc->codec_id), &opt)) != 0)
                goto failed;
            ffs->st = ffs->fc->streams[ffs->si];
            ffs->tmp = ffmpeg.av_frame_alloc();
            ffs->dst = ffmpeg.av_frame_alloc();
            return;
            failed:
            FreeFFS(ffs);
        }

        private static void FreeFFS(FFS* ffs) {
            if (ffs->swrc != null)
                ffmpeg.swr_free(&ffs->swrc);
            if (ffs->swsc != null)
                ffmpeg.sws_freeContext(ffs->swsc);
            if (ffs->dst != null)
                ffmpeg.av_free(ffs->dst);
            if (ffs->tmp != null)
                ffmpeg.av_free(ffs->tmp);
            if (ffs->cc != null)
                ffmpeg.avcodec_close(ffs->cc);
            if (ffs->fc != null)
                ffmpeg.avformat_close_input(&ffs->fc);
        }
    }
}