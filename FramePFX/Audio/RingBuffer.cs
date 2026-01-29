// 
// Copyright (c) 2026-2026 REghZy
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

namespace FramePFX.Audio;

public sealed class RingBuffer {
    private readonly Lock dataLock = new Lock();
    private readonly float[] buffer;
    private int head;
    private int tail;
    private int count;

    public int Count {
        get {
            lock (this.dataLock)
                return this.count;
        }
    }

    public RingBuffer(int capacity = 44100) {
        this.buffer = new float[capacity];
    }

    public int Enqueue(ReadOnlySpan<float> samples) {
        lock (this.dataLock) {
            int free = this.buffer.Length - this.count;
            if (free > 0) {
                int toWrite = Math.Min(samples.Length, free);
                int offset = Math.Min(toWrite, this.buffer.Length - this.head);
                samples.Slice(0, offset).CopyTo(this.buffer.AsSpan(this.head, offset));

                int tmp = toWrite - offset;
                if (tmp > 0) {
                    samples.Slice(offset, tmp).CopyTo(this.buffer.AsSpan(0, tmp));
                }

                this.head = (this.head + toWrite) % this.buffer.Length;
                this.count += toWrite;

                return toWrite;
            }

            return 0;
        }
    }

    public int Read(Span<float> dest) {
        Span<float> s;
        lock (this.dataLock) {
            int toRead = Math.Min(dest.Length, this.count);
            if (toRead > 0) {
                int offset = Math.Min(toRead, this.buffer.Length - this.tail);

                s = this.buffer.AsSpan(this.tail, offset);
                s.CopyTo(dest.Slice(0, offset));
                s.Clear();

                int tmp = toRead - offset;
                if (tmp > 0) {
                    s = this.buffer.AsSpan(0, tmp);
                    s.CopyTo(dest.Slice(offset, tmp));
                    s.Clear();
                }

                this.tail = (this.tail + toRead) % this.buffer.Length;
                this.count -= toRead;

                return toRead;
            }

            return 0;
        }
    }

    public int Peek(Span<float> dest) {
        lock (this.dataLock) {
            int toRead = Math.Min(dest.Length, this.count);
            if (toRead > 0) {
                int offset = Math.Min(toRead, this.buffer.Length - this.tail);
                this.buffer.AsSpan(this.tail, offset).CopyTo(dest.Slice(0, offset));

                int tmp = toRead - offset;
                if (tmp > 0) {
                    this.buffer.AsSpan(0, tmp).CopyTo(dest.Slice(offset, tmp));
                }

                return toRead;
            }

            return 0;
        }
    }

    public void Clear() {
        lock (this.dataLock) {
            if (this.count > 0) {
                int offset = Math.Min(this.count, this.buffer.Length - this.tail);
                this.buffer.AsSpan(this.tail, offset).Clear();
                int tmp = this.count - offset;
                if (tmp > 0) {
                    this.buffer.AsSpan(0, tmp).Clear();
                }

                this.tail = this.head = 0;
                this.count = 0;
            }
        }
    }
}