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

using FFmpeg.AutoGen;
using FramePFX.FFmpegWrapper;
using FramePFX.FFmpegWrapper.Codecs;
using FramePFX.FFmpegWrapper.Containers;

namespace FramePFX.FFmpeg;

public class AudioStream : StreamWrapper
{
    private AudioDecoder decoder;

    protected override MediaDecoder DecoderInternal => this.decoder;

    public AudioStream(MediaStream stream) : base(stream) {
    }

    public unsafe AudioDecoder GetDecoder(bool open = true)
    {
        if (this.decoder == null || !this.decoder.IsOpen)
        {
            AVCodecID codecId = this.Stream.Handle->codecpar->codec_id;
            this.decoder = new AudioDecoder(codecId);
            int err = ffmpeg.avcodec_parameters_to_context(this.decoder.Handle, this.Stream.Handle->codecpar);
            if (FFUtils.GetException(err, "Could not copy stream parameters to the audio decoder.", out Exception e))
            {
                this.decoder.Dispose();
                this.decoder = null;
                throw e;
            }

            if (open)
            {
                try
                {
                    this.decoder.Open();
                }
                catch
                {
                    this.decoder.Dispose();
                    this.decoder = null;
                    throw;
                }
            }
        }

        return this.decoder;
    }

    public override void DisposeDecoder(bool flushBuffers = true)
    {
        base.DisposeDecoder(flushBuffers);
        this.decoder = null;
    }
}