using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using FramePFX.Core.Editor;
using FramePFX.Core.FFmpegWrapper;
using FramePFX.Core.FFmpegWrapper.Containers;

namespace FramePFX.Core.FFmpeg
{
    public unsafe class FFmpegReader
    {
        private volatile bool isOpen;
        private bool tryUseHardwareDecode;
        private string filePath;

        // last seeked frame, used to determine whether to use efficient
        // decoding or a slow seek
        private long lastFrame;

        // demuxer
        private AVFormatContext* fmtCtx;
        private AVPacket* packet;

        // array of streams
        private MediaStream[] streams;
        private VideoStream[] videoStreams;

        private AudioStream[] audioStreams;

        // these arrays are used to map relative indices to absolute indices
        // e.g. VS, VS, AS, AS, AS, ST
        // videoStreamIdx = {0, 1}
        // audioStreamIdx = {2, 3, 4}
        // subtitleStreamIdx = {5}
        private int[] videoStreamIdx; // vs -- video streams
        private int[] audioStreamIdx; // as - audio streams
        private int[] subtitleStreamIdx; // st - subtitle streams

        public int VideoStreamCount => this.videoStreams.Length;
        public int AudioStreamCount => this.audioStreams.Length;
        public int SubtitleStreamCount => this.subtitleStreamIdx.Length;

        // locks
        private readonly object getFrameMutex;

        private Dictionary<string, string> metadata;

        public bool IsOpen => this.isOpen;

        public FFmpegReader()
        {
            this.getFrameMutex = new object();
        }

        #region Opening and close

        /// <summary>
        /// Opens the reader using the given file path. Does not open any decoders
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Open(string file, bool tryUseHwDecode = false)
        {
            lock (this.getFrameMutex)
            {
                if (this.isOpen)
                {
#if DEBUG
                    throw new InvalidOperationException("Reader is already open. Close it first");
#else
                    this.Close();
#endif
                }

                this.filePath = file;
                this.tryUseHardwareDecode = tryUseHwDecode;

                AVFormatContext* ctx;
                int err = ffmpeg.avformat_open_input(&ctx, file, null, null);
                if (FFUtils.GetException(err, "Could not open file", out Exception e))
                {
                    if (ctx != null)
                        ffmpeg.avformat_free_context(ctx);
                    throw e;
                }

                err = ffmpeg.avformat_find_stream_info(ctx, null);
                if (FFUtils.GetException(err, "File did not contain any streams", out e))
                {
                    if (ctx != null)
                        ffmpeg.avformat_free_context(ctx);
                    throw e;
                }

                this.fmtCtx = ctx;
                try
                {
                    // load stream wrappers, and cache useful stream indices
                    uint count = ctx->nb_streams;
                    this.streams = new MediaStream[count];
                    List<int> vsIdx = new List<int>(2), asIdx = new List<int>(2), stIdx = new List<int>();
                    List<VideoStream> vsLs = new List<VideoStream>(2);
                    List<AudioStream> asLs = new List<AudioStream>(2);
                    for (int i = 0; i < count; i++)
                    {
                        MediaStream stream = new MediaStream(ctx->streams[i]);
                        this.streams[i] = stream;
                        switch (stream.Type)
                        {
                            case AVMediaType.AVMEDIA_TYPE_VIDEO:
                            {
                                vsLs.Add(new VideoStream(stream));
                                vsIdx.Add(i);
                                break;
                            }
                            case AVMediaType.AVMEDIA_TYPE_AUDIO:
                            {
                                asLs.Add(new AudioStream(stream));
                                asIdx.Add(i);
                                break;
                            }
                            case AVMediaType.AVMEDIA_TYPE_SUBTITLE:
                            {
                                stIdx.Add(i);
                                break;
                            }
                        }
                    }

                    this.videoStreamIdx = vsIdx.ToArray();
                    this.audioStreamIdx = asIdx.ToArray();
                    this.subtitleStreamIdx = stIdx.ToArray();
                    this.videoStreams = vsLs.ToArray();
                    this.audioStreams = asLs.ToArray();
                }
                catch (Exception ex)
                {
                    if (ctx != null)
                        ffmpeg.avformat_close_input(&ctx);
                    this.fmtCtx = null;
                    throw new Exception("Exception while extracting stream info", ex);
                }

                this.metadata = new Dictionary<string, string>();
                AVDictionaryEntry* tag = null;
                while ((tag = ffmpeg.av_dict_get(ctx->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
                {
                    string key = Marshal.PtrToStringAnsi((IntPtr) tag->key);
                    string val = Marshal.PtrToStringAnsi((IntPtr) tag->value);
                    if (!string.IsNullOrEmpty(key))
                        this.metadata[key] = val?.Trim();
                }

                this.isOpen = true;
            }
        }

        public void Close()
        {
            lock (this.getFrameMutex)
            {
                if (!this.isOpen)
                {
#if DEBUG
                    throw new InvalidOperationException("Reader is not open. Open it first");
#else
                    return;
#endif
                }

                this.isOpen = false;

                AVPacket* lastPacket = this.packet;
                this.packet = null;

                if (lastPacket != null)
                    ffmpeg.av_packet_free(&lastPacket);

                foreach (VideoStream stream in this.videoStreams)
                    stream.Dispose();
                foreach (AudioStream stream in this.audioStreams)
                    stream.Dispose();

                this.streams = null;
                this.videoStreamIdx = null;
                this.audioStreamIdx = null;
                this.subtitleStreamIdx = null;
                this.videoStreams = null;
                this.audioStreams = null;
                this.metadata = null;
                if (this.fmtCtx != null)
                {
                    // avoids using fixed statement
                    AVFormatContext* ptr = this.fmtCtx;
                    ffmpeg.avformat_close_input(&ptr);
                    this.fmtCtx = null;
                }

                this.lastFrame = 0L;
            }
        }

        #endregion

        public static Rational GetAspectRatio(MediaStream stream, int width, int height)
        {
            int pr_num = 1, pr_den = 1;
            AVRational sar = stream.Handle->sample_aspect_ratio;
            if (sar.num != 0)
            {
                pr_num = sar.num;
                pr_den = sar.den;
            }
            else if ((sar = stream.Handle->codecpar->sample_aspect_ratio).num != 0)
            {
                pr_num = sar.num;
                pr_den = sar.den;
            }

            Rational size = new Rational(width * pr_num, height * pr_den);
            return size.Reduced;
        }

        #region Helper Stream Functions

        public MediaStream GetStream(int index) => this.streams[index];
        public VideoStream GetVideoStream(int index) => this.videoStreams[index];
        public AudioStream GetAudioStream(int index) => this.audioStreams[index];
        public MediaStream GetSubtitleStream(int index) => this.GetStream(this.subtitleStreamIdx[index]);

        public IEnumerable<MediaStream> GetStreams()
        {
            return this.streams;
        }

        public IEnumerable<VideoStream> GetVideoStreams()
        {
            return this.videoStreams;
        }

        public IEnumerable<AudioStream> GetAudioStreams()
        {
            return this.audioStreams;
        }

        public IEnumerable<MediaStream> GetSubtitleStreams()
        {
            foreach (int index in this.subtitleStreamIdx)
                yield return this.streams[index];
        }

        #endregion
    }
}