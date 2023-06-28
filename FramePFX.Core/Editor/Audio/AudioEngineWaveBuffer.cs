using System;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

namespace FramePFX.Core.Editor.Audio {
    public class AudioEngineWaveBuffer : IDisposable {
        private readonly WaveHeader header;
        private readonly byte[] buffer;
        private readonly BufferedWaveProvider waveStream;
        private readonly object waveOutLock;
        private GCHandle hBuffer;
        private IntPtr hWaveOut;
        private GCHandle hHeader;
        private GCHandle hThis;

        public bool InQueue => (this.header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;

        public int BufferSize { get; }

        public BufferedWaveProvider Buffer => this.waveStream;

        public AudioEngineWaveBuffer(IntPtr hWaveOut, int bufferSize, BufferedWaveProvider waveStream, object waveOutLock) {
            this.BufferSize = bufferSize;
            this.buffer = new byte[bufferSize];
            this.hBuffer = GCHandle.Alloc(this.buffer, GCHandleType.Pinned);
            this.hWaveOut = hWaveOut;
            this.waveStream = waveStream;
            this.waveOutLock = waveOutLock;
            this.header = new WaveHeader();
            this.hHeader = GCHandle.Alloc(this.header, GCHandleType.Pinned);
            this.header.dataBuffer = this.hBuffer.AddrOfPinnedObject();
            this.header.bufferLength = bufferSize;
            this.header.loops = 1;
            this.hThis = GCHandle.Alloc(this);
            this.header.userData = (IntPtr) this.hThis;
            lock (waveOutLock) {
                MmException.Try(WaveInterop.waveOutPrepareHeader(hWaveOut, this.header, Marshal.SizeOf(this.header)), "waveOutPrepareHeader");
            }
        }

        ~AudioEngineWaveBuffer() => this.Dispose(false);

        public void Dispose() {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        public bool ProcessBuffer() {
            int num;
            lock (this.waveStream) {
                num = this.waveStream.Read(this.buffer, 0, this.buffer.Length);
            }

            if (num == 0)
                return false;
            for (int index = num; index < this.buffer.Length; ++index)
                this.buffer[index] = 0;
            this.WriteToWaveOut();
            return true;
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
            if (this.hBuffer.IsAllocated)
                this.hBuffer.Free();
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