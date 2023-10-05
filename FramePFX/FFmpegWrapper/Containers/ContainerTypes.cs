using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Containers {
    public static class ContainerTypes {
        public const string
            //General
            Mp4 = "mp4",
            Mov = "mov",
            Matroska = "mkv",
            WebM = "webm",
            //Audio
            Mp3 = "mp3",
            Ogg = "ogg",
            Flac = "flac",
            M4a = "m4a",
            Wav = "wav";

        public static unsafe AVOutputFormat* GetOutputFormat(string extension) {
            AVOutputFormat* fmt = ffmpeg.av_guess_format(null, "dummy." + extension, null);
            if (fmt == null) {
                throw new NotSupportedException();
            }

            return fmt;
        }
    }
}