using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

namespace FramePFX.Core.Editor.Audio {
    public class AudioEngineWaveBuffer : IDisposable {
        private readonly WaveHeader header;
        private readonly object waveOutLock;
        private IntPtr hBuffer;
        private IntPtr hWaveOut;
        private GCHandle hHeader;
        private GCHandle hThis;
        private int currentOffset;

        public bool InQueue => (this.header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;

        public int BufferSize { get; }

        public BufferedWaveProvider WaveStream { get; }

        public AudioEngineWaveBuffer(IntPtr hWaveOut, int bufferSize, BufferedWaveProvider waveStream, object waveOutLock) {
            this.BufferSize = bufferSize;
            this.hBuffer = Marshal.AllocHGlobal(bufferSize);
            this.hWaveOut = hWaveOut;
            this.WaveStream = waveStream;
            this.waveOutLock = waveOutLock;
            this.header = new WaveHeader();
            this.hHeader = GCHandle.Alloc(this.header, GCHandleType.Pinned);
            this.header.dataBuffer = this.hBuffer;
            this.header.bufferLength = bufferSize;
            this.header.loops = 1;
            this.hThis = GCHandle.Alloc(this);
            this.header.userData = (IntPtr) this.hThis;
            lock (waveOutLock) {
                MmResult result = WaveInterop.waveOutPrepareHeader(hWaveOut, this.header, Marshal.SizeOf(this.header));
                if (result != MmResult.NoError) {
                    if (this.hHeader.IsAllocated)
                        this.hHeader.Free();
                    Marshal.FreeHGlobal(this.hBuffer);
                    if (this.hThis.IsAllocated)
                        this.hThis.Free();
                    throw new MmException(result, "waveOutPrepareHeader");
                }
            }
        }

        ~AudioEngineWaveBuffer() => this.Dispose(false);

        public void Dispose() {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        public unsafe void WriteSamplesAndWriteWaveOut(byte* srcSamples, int count) {
            int overshoot = this.currentOffset + count;
            if (overshoot >= this.BufferSize) {
                count -= overshoot - this.BufferSize;
            }

            byte* dst = (byte*) this.hBuffer + this.currentOffset;
            Unsafe.CopyBlock(dst, srcSamples, (uint) count);
            uint tail = (uint) (this.BufferSize - this.currentOffset - count);
            if (tail > 0) {
                Unsafe.InitBlock(dst + count, 0, tail);
            }

            this.currentOffset += count;
        }

        public bool WriteBuffer() {
            if (this.currentOffset > 0) {
                this.WriteToWaveOut();
                this.currentOffset = 0;
                return true;
            }

            this.currentOffset = 0;
            return false;
        }

        private void WriteToWaveOut() {
            MmResult result;
            lock (this.waveOutLock) {
                result = WaveInterop.waveOutWrite(this.hWaveOut, this.header, Marshal.SizeOf(this.header));
            }

            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutWrite");
            GC.KeepAlive(this);
        }

        private void Dispose(bool disposing) {
            if (this.hHeader.IsAllocated)
                this.hHeader.Free();
            Marshal.FreeHGlobal(this.hBuffer);
            if (this.hThis.IsAllocated)
                this.hThis.Free();
            if (this.hWaveOut != IntPtr.Zero) {
                lock (this.waveOutLock) {
                    int num2 = (int) WaveInterop.waveOutUnprepareHeader(this.hWaveOut, this.header, Marshal.SizeOf(this.header));
                    this.hWaveOut = IntPtr.Zero;
                }
            }
        }
    }
}