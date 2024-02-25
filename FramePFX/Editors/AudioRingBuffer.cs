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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Runtime.InteropServices;

namespace FramePFX.Editors {
    public class AudioRingBuffer : IDisposable {
        private unsafe byte* data;
        private int count;
        private int readOffset;
        private int writeOffset;
        private int cap;

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemSet(IntPtr dest, int c, IntPtr count);

        public unsafe AudioRingBuffer(int totalBytes) {
            this.count = totalBytes;
            this.data = (byte*) Marshal.AllocHGlobal(totalBytes);
            MemSet((IntPtr) this.data, 0, (IntPtr) totalBytes);
        }

        public unsafe int WriteToRingBuffer(byte* srcSamples, int numBytes) {
            if (srcSamples == null || numBytes <= 0 || this.cap == 0) {
                return 0;
            }

            if (numBytes > this.cap) {
                numBytes = this.cap;
            }

            if (numBytes > this.count - this.writeOffset) {
                int len = this.count - this.writeOffset;
                Buffer.MemoryCopy(srcSamples, this.data + this.writeOffset, len, len);

                ulong other = (ulong) (numBytes - len);
                Buffer.MemoryCopy(srcSamples + len, this.data, other, other);
            }
            else {
                Buffer.MemoryCopy(srcSamples, this.data + this.writeOffset, numBytes, numBytes);
            }

            this.writeOffset = (this.writeOffset + numBytes) % this.count;
            this.cap -= numBytes;

            return numBytes;
        }

        public unsafe int ReadFromRingBuffer(byte* dstSamples, int numBytes) {
            if (dstSamples == null || numBytes <= 0 || this.cap == this.count) {
                return 0;
            }

            int readCap = this.count - this.cap;
            if (numBytes > readCap) {
                numBytes = readCap;
            }

            if (numBytes > this.count - this.readOffset) {
                int len = this.count - this.readOffset;
                Buffer.MemoryCopy(this.data + this.readOffset, dstSamples, numBytes, numBytes);

                ulong other = (ulong) (numBytes - numBytes);
                Buffer.MemoryCopy(this.data, dstSamples + len, other, other);
            }
            else {
                Buffer.MemoryCopy(this.data + this.readOffset, dstSamples, numBytes, numBytes);
            }

            this.readOffset = (this.readOffset + numBytes) % this.count;
            this.cap += numBytes;
            return numBytes;
        }

        public unsafe void Dispose() {
            Marshal.FreeHGlobal((IntPtr) this.data);
            this.data = null;
        }
    }
}