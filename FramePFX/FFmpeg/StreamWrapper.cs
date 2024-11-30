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
using FramePFX.FFmpegWrapper.Codecs;
using FramePFX.FFmpegWrapper.Containers;

namespace FramePFX.FFmpeg;

public abstract class StreamWrapper : IDisposable {
    public MediaStream Stream { get; }

    protected abstract MediaDecoder DecoderInternal { get; }

    protected StreamWrapper(MediaStream stream) {
        this.Stream = stream;
    }

    ~StreamWrapper() => this.Dispose(false);

    public virtual void DisposeDecoder(bool flushBuffers = true) {
        MediaDecoder decoder = this.DecoderInternal;
        if (decoder != null && (decoder.IsOpen || !decoder.IsDisposed)) {
            if (flushBuffers) {
                unsafe {
                    ffmpeg.avcodec_flush_buffers(decoder.Handle);
                }
            }

            decoder.Dispose();
        }
    }

    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        this.DisposeDecoder(disposing);
    }
}