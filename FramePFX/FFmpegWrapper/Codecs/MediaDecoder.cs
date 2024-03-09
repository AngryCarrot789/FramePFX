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

using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Codecs
{
    public abstract unsafe class MediaDecoder : CodecBase
    {
        public MediaDecoder(AVCodecContext* ctx, AVMediaType expectedType, bool takeOwnership = true) : base(ctx, expectedType, takeOwnership)
        {
        }

        public void SendPacket(MediaPacket pkt)
        {
            LavResult result = (LavResult) ffmpeg.avcodec_send_packet(this.Handle, pkt == null ? null : pkt.Handle);
            if (result != LavResult.Success && !(result == LavResult.EndOfFile && pkt == null))
            {
                result.ThrowIfError("Could not decode packet (hints: check if the decoder is open, try receiving frames first)");
            }
        }

        public bool ReceiveFrame(MediaFrame frame)
        {
            LavResult result = (LavResult) ffmpeg.avcodec_receive_frame(this.Handle, frame.Handle);
            if (result != LavResult.Success && result != LavResult.TryAgain && result != LavResult.EndOfFile)
            {
                result.ThrowIfError("Could not decode frame");
            }

            return result == 0;
        }
    }
}