//
// MIT License
//
// Copyright (c) 2023 dubiousconst282
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper;

public static class FFUtils {
    public static readonly AVRational TimeSpanRational = new AVRational { num = 1, den = (int) TimeSpan.TicksPerSecond };

    public static unsafe string GetErrorString(int error) {
        byte* buf = stackalloc byte[ffmpeg.AV_ERROR_MAX_STRING_SIZE + 1]; // null terminator
        ffmpeg.av_strerror(error, buf, ffmpeg.AV_ERROR_MAX_STRING_SIZE);
        return Marshal.PtrToStringAnsi((IntPtr) buf);
    }

    /// <summary>
    /// Useful alternative to <see cref="CheckError"/> when you need to cleanup before throwing an exception
    /// </summary>
    public static bool GetException(int error, string msg, out Exception exception) {
        if (error < 0) {
            exception = GetException(error, msg);
            return true;
        }

        exception = null;
        return false;
    }

    public static int CheckError(int errno, string msg) {
        if (errno < 0 && errno != ffmpeg.EAGAIN && errno != ffmpeg.AVERROR_EOF) {
            throw GetException(errno, msg);
        }

        return errno;
    }

    public static Exception GetException(int errno, string msg = null) {
        return new InvalidOperationException($"{msg ?? "Operation failed"}: {GetErrorString(errno)}");
    }

    public static unsafe ReadOnlySpan<T> GetSpanFromSentinelTerminatedPtr<T>(T* ptr, T terminator) where T : unmanaged {
        int len = 0;

        while (ptr != null && !ptr[len].Equals(terminator)) {
            len++;
        }

        return new ReadOnlySpan<T>(ptr, len);
    }

    public static long? GetPTS(long pts) => pts != ffmpeg.AV_NOPTS_VALUE ? (long?) pts : null;
    public static void SetPTS(ref long pts, long? value) => pts = value ?? ffmpeg.AV_NOPTS_VALUE;

    public static TimeSpan? GetTimeSpan(long pts, AVRational timeBase) {
        if (pts == ffmpeg.AV_NOPTS_VALUE) {
            return null;
        }

        long ticks = ffmpeg.av_rescale_q(pts, timeBase, TimeSpanRational);
        return TimeSpan.FromTicks(ticks);
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
}