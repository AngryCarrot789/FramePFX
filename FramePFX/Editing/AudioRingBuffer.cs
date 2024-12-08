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

using System.Runtime.InteropServices;

namespace FramePFX.Editing;

public class AudioRingBuffer : IDisposable
{
    private unsafe float* data;
    private readonly int capacity;
    private readonly int capacity_bytes;
    private int readOffset; // the SAMPLE read offset
    private int writeOffset; // the SAMPLE write offset
    private int free; // number of samples free to be written

    public unsafe Span<float> Data => new Span<float>(this.data, this.capacity);

    public unsafe AudioRingBuffer(int capacitySamples)
    {
        this.capacity = capacitySamples;
        this.capacity_bytes = capacitySamples * sizeof(float);
        this.data = (float*) Marshal.AllocHGlobal(this.capacity_bytes);
        this.free = capacitySamples;
        SetMemory(this.data, 0, this.capacity_bytes);
    }

    public void OffsetWrite(int numSamples)
    {
        if (numSamples <= 0)
        {
            return;
        }

        // the total number of written samples in this ring buffer
        int numSamplesWritten = this.capacity - this.free;
        if (numSamplesWritten > 0)
        {
            if (numSamples > numSamplesWritten)
            {
                numSamples = numSamplesWritten;
            }

            this.writeOffset = (this.writeOffset - numSamples) % this.capacity;
            this.free += numSamples;
        }
    }

    public unsafe int WriteToRingBuffer(float* src, int numSamples)
    {
        if (src == null || numSamples <= 0)
        {
            return 0;
        }

        if (numSamples > this.free)
        {
            numSamples = this.free;
        }

        int availableSamplesToWrite = this.capacity - this.writeOffset;
        if (numSamples > availableSamplesToWrite)
        {
            CopyMemory(src, this.data + this.writeOffset, availableSamplesToWrite * sizeof(float));
            CopyMemory(src + availableSamplesToWrite, this.data, (numSamples - availableSamplesToWrite) * sizeof(float));
        }
        else
        {
            CopyMemory(src, this.data + this.writeOffset, numSamples * sizeof(float));
        }

        this.writeOffset = (this.writeOffset + numSamples) % this.capacity;
        this.free -= numSamples;
        return numSamples;
    }

    public unsafe int ReadFromRingBuffer(float* dst, int numSamples)
    {
        if (dst == null || numSamples <= 0)
        {
            return 0;
        }

        // the total number of written samples in this ring buffer
        int numSamplesWritten = this.capacity - this.free;
        if (numSamplesWritten < 1)
        {
            return 0;
        }

        if (numSamples > numSamplesWritten)
        {
            numSamples = numSamplesWritten;
        }

        int availableSamplesToRead = this.capacity - this.readOffset;
        if (numSamples > availableSamplesToRead)
        {
            CopyMemory(this.data + this.readOffset, dst, availableSamplesToRead * sizeof(float));
            CopyMemory(this.data, dst + availableSamplesToRead, (numSamples - availableSamplesToRead) * sizeof(float));
        }
        else
        {
            CopyMemory(this.data + this.readOffset, dst, numSamples * sizeof(float));
        }

        this.readOffset = (this.readOffset + numSamples) % this.capacity;
        this.free += numSamples;
        return numSamples;
    }

    public unsafe void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr) this.data);
        this.data = null;
    }

    [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    private static extern unsafe IntPtr _memset(void* dst, int val, IntPtr count);

    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    private static extern unsafe IntPtr _memcpy(void* dst, void* src, IntPtr n);

    private static unsafe void SetMemory(void* buffer, int value, int count) => _memset(buffer, value, new IntPtr(count));
    private static unsafe void SetMemory(void* buffer, int value, IntPtr count) => _memset(buffer, value, count);
    private static unsafe void CopyMemory(void* src, void* dst, int count) => _memcpy(dst, src, new IntPtr(count));
    private static unsafe void CopyMemory(void* src, void* dst, IntPtr count) => _memcpy(dst, src, count);
}