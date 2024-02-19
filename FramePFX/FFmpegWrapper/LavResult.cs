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

namespace FramePFX.FFmpegWrapper {
    public enum LavResult {
        Success = 0,

        /// <summary>AVERROR(EAGAIN)</summary>
        TryAgain = -11,

        /// <summary>AVERROR(ENOMEM)</summary>
        OutOfMemory = -12,

        /// <summary>AVERROR(EINVAL)</summary>
        InvalidArgument = -22,

        /// <summary>AVERROR_BSF_NOT_FOUND</summary>
        BsfNotFound = -1179861752,

        /// <summary>AVERROR_BUFFER_TOO_SMALL</summary>
        BufferTooSmall = -1397118274,

        /// <summary>AVERROR_BUG</summary>
        Bug = -558323010,

        /// <summary>AVERROR_BUG2</summary>
        Bug2 = -541545794,

        /// <summary>AVERROR_DECODER_NOT_FOUND</summary>
        DecoderNotFound = -1128613112,

        /// <summary>AVERROR_DEMUXER_NOT_FOUND</summary>
        DemuxerNotFound = -1296385272,

        /// <summary>AVERROR_ENCODER_NOT_FOUND</summary>
        EncoderNotFound = -1129203192,

        /// <summary>AVERROR_EOF</summary>
        EndOfFile = -541478725,

        /// <summary>AVERROR_EXIT</summary>
        Exit = -1414092869,

        /// <summary>AVERROR_EXTERNAL</summary>
        External = -542398533,

        /// <summary>AVERROR_FILTER_NOT_FOUND</summary>
        FilterNotFound = -1279870712,

        /// <summary>AVERROR_HTTP_BAD_REQUEST</summary>
        HttpBadRequest = -808465656,

        /// <summary>AVERROR_HTTP_FORBIDDEN</summary>
        HttpForbidden = -858797304,

        /// <summary>AVERROR_HTTP_NOT_FOUND</summary>
        HttpNotFound = -875574520,

        /// <summary>AVERROR_HTTP_OTHER_4XX</summary>
        HttpOther4xx = -1482175736,

        /// <summary>AVERROR_HTTP_SERVER_ERROR</summary>
        HttpServerError = -1482175992,

        /// <summary>AVERROR_HTTP_UNAUTHORIZED</summary>
        HttpUnauthorized = -825242872,

        /// <summary>AVERROR_INVALIDDATA</summary>
        InvalidData = -1094995529,

        /// <summary>AVERROR_MUXER_NOT_FOUND</summary>
        MuxerNotFound = -1481985528,

        /// <summary>AVERROR_OPTION_NOT_FOUND</summary>
        OptionNotFound = -1414549496,

        /// <summary>AVERROR_PATCHWELCOME</summary>
        PatchWelcome = -1163346256,

        /// <summary>AVERROR_PROTOCOL_NOT_FOUND</summary>
        ProtocolNotFound = -1330794744,

        /// <summary>AVERROR_STREAM_NOT_FOUND</summary>
        StreamNotFound = -1381258232,

        /// <summary>AVERROR_UNKNOWN</summary>
        Unknown = -1313558101,

        /// <summary>AVERROR_EXPERIMENTAL</summary>
        Experimental = -733130664,

        /// <summary>AVERROR_INPUT_CHANGED</summary>
        InputChanged = -1668179713,

        /// <summary>AVERROR_OUTPUT_CHANGED</summary>
        OutputChanged = -1668179714
    }

    public static class LavResultEx {
        public static bool IsSuccess(this LavResult result) {
            return result >= LavResult.Success;
        }

        public static void ThrowIfError(this LavResult result, string msg = null) {
            if (result < LavResult.Success) {
                throw FFUtils.GetException((int) result, msg);
            }
        }
    }
}