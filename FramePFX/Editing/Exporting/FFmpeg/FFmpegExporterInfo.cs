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
using FramePFX.DataTransfer;
using FramePFX.Interactivity.Formatting;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Utils.Accessing;

namespace FramePFX.Editing.Exporting.FFmpeg;

public class FFmpegExporterInfo : BaseExporterInfo {
    public static readonly DataParameterLong BitRateParameter =
        DataParameter.Register(
            new DataParameterLong(
                typeof(FFmpegExporterInfo),
                nameof(BitRate),
                25000000, 100, 10_000_000_000,
                ValueAccessors.Reflective<long>(typeof(FFmpegExporterInfo), nameof(myBitRate))));

    public static readonly DataParameterLong GopParameter =
        DataParameter.Register(
            new DataParameterLong(
                typeof(FFmpegExporterInfo),
                // I have no idea what GOP is... is 1000 too big?
                nameof(Gop), defValue: 10, minValue: 0, maxValue: 1000,
                ValueAccessors.Reflective<long>(typeof(FFmpegExporterInfo), nameof(myGop))));

    public static readonly DataParameter<AVCodecID> CodecIdParameter =
        DataParameter.Register(
            new DataParameter<AVCodecID>(
                typeof(FFmpegExporterInfo),
                nameof(CodecId), AVCodecID.AV_CODEC_ID_H264,
                ValueAccessors.Reflective<AVCodecID>(typeof(FFmpegExporterInfo), nameof(codecId))));

    private AVCodecID codecId;

    public AVCodecID CodecId {
        get => this.codecId;
        set => DataParameter.SetValueHelper(this, CodecIdParameter, ref this.codecId, value);
    }

    private long myBitRate;
    private long myGop;

    public long BitRate {
        get => this.myBitRate;
        set => DataParameter.SetValueHelper(this, BitRateParameter, ref this.myBitRate, value);
    }

    public long Gop {
        get => this.myGop;
        set => DataParameter.SetValueHelper(this, GopParameter, ref this.myGop, value);
    }

    public FFmpegExporterInfo() {
        this.myBitRate = BitRateParameter.GetDefaultValue(this);
        this.myGop = GopParameter.GetDefaultValue(this);
        this.codecId = CodecIdParameter.GetDefaultValue(this);

        this.PropertyEditor.Root.AddItem(new DataParameterLongPropertyEditorSlot(BitRateParameter, typeof(FFmpegExporterInfo), "Bit Rate", DragStepProfile.UnitOne) {
            ValueFormatter = new AutoMemoryValueFormatter(MemoryValueFormatter.Bits) {
                SourceFormat = MemoryFormatType.Bit
            }
        });

        this.PropertyEditor.Root.AddItem(new DataParameterLongPropertyEditorSlot(GopParameter, typeof(FFmpegExporterInfo), "'GOP'", DragStepProfile.UnitOne));
        this.PropertyEditor.Root.AddItem(new DataParameterAVCodecIDPropertyEditorSlot(CodecIdParameter, typeof(FFmpegExporterInfo)));
    }

    public override void Reset() {
        base.Reset();
        BitRateParameter.Reset(this);
        GopParameter.Reset(this);
    }

    public override BaseExportContext CreateContext(ExportSetup setup) {
        return new FFmpegExportContext(this, setup);
    }
}