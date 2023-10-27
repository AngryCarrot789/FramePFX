using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FramePFX.Utils;

namespace FramePFX.Editor.Rendering.PFXCE {
    /// <summary>
    /// A deferred render command list for the extensive composition engine, though more like TCE for tiny composition engine
    /// </summary>
    public class RenderCommandList {
        private byte[] buffer; // record entry buffer
        private int count;     // the number of bytes written

        public RenderCommandList() {
        }

        public unsafe void WriteCommandRecord(XCECMD cmdId, void* pbRecord, int cbRecordSize) {
            int cbEntry = checked(cbRecordSize + sizeof(CmdRecordHeader));
            int cbMinSize = checked(this.count + cbEntry);
            if (this.buffer == null || cbMinSize > this.buffer.Length)
                EnsureBufferCapacity(ref this.buffer, cbMinSize);
            CmdRecordHeader header;
            header.EntrySize = cbEntry;
            header.CmdId = cmdId;
            Marshal.Copy((IntPtr) (&header), this.buffer, this.count, sizeof(CmdRecordHeader));
            Marshal.Copy((IntPtr) pbRecord, this.buffer, this.count + sizeof(CmdRecordHeader), cbRecordSize);
            this.count += cbEntry;
        }

        public void TrimBuffer() {
            if (this.buffer != null && this.count < this.buffer.Length) {
                this.buffer = this.buffer.CloneArrayMin(this.count);
            }
        }

        private static void EnsureBufferCapacity(ref byte[] buffer, int cbMinSize) {
            if (buffer == null) {
                buffer = new byte[cbMinSize];
            }
            else {
                byte[] copy = new byte[Math.Max((buffer.Length << 1) - (buffer.Length >> 1), cbMinSize)];
                buffer.CopyTo(copy, 0);
                buffer = copy;
            }
        }

        public CommandIterator GetCommandIterator() => new CommandIterator(this);

        public struct CommandIterator {
            private readonly RenderCommandList list;
            private int offset;

            public CommandIterator(RenderCommandList list) {
                this.list = list;
                this.offset = 0;
            }

            public bool HasNext() {
                return this.offset < this.list.count;
            }

            public unsafe void Next(out XCECMD cmd, out Span<byte> record) {
                byte[] buf = this.list.buffer;
                CmdRecordHeader header = Unsafe.ReadUnaligned<CmdRecordHeader>(ref buf[this.offset]);
                record = new Span<byte>(buf, this.offset, header.EntrySize - sizeof(CmdRecordHeader));
                cmd = header.CmdId;
                this.offset += header.EntrySize;
            }
        }

        private struct CmdRecordHeader {
            public int EntrySize;
            public XCECMD CmdId;
        }
    }
}