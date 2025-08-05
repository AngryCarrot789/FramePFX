// 
// Copyright (c) 2024-2024 REghZy
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
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Utils;

namespace FramePFX.Plugins.FFmpegMedia.Exporter;

public class DataParameterAVCodecIDPropertyEditorSlot : DataParameterEnumPropertyEditorSlot<AVCodecID> {
    public static DataParameterEnumInfo<AVCodecID> CodedIdEnumInfo { get; }

    public DataParameterAVCodecIDPropertyEditorSlot(DataParameter<AVCodecID> parameter, Type applicableType, string displayName, IEnumerable<AVCodecID>? values = null, DataParameterEnumInfo<AVCodecID>? translationInfo = null) : base(parameter, applicableType, displayName, values, translationInfo) { }
    public DataParameterAVCodecIDPropertyEditorSlot(DataParameter<AVCodecID> parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName ?? "Codec ID", EnumInfo<AVCodecID>.EnumValuesOrderedByName, CodedIdEnumInfo) { }

    static DataParameterAVCodecIDPropertyEditorSlot() {
        Dictionary<AVCodecID, string> coded2name = new Dictionary<AVCodecID, string>();
        foreach (AVCodecID codec in EnumInfo<AVCodecID>.EnumValues)
            coded2name[codec] = codec.ToString().Substring(12);

        CodedIdEnumInfo = DataParameterEnumInfo<AVCodecID>.FromAllowed(EnumInfo<AVCodecID>.EnumValuesOrderedByName, coded2name);
    }
}