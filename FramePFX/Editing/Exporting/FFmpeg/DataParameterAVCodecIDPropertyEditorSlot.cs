using System.Collections.ObjectModel;
using FFmpeg.AutoGen;
using FramePFX.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.Editing.Exporting.FFmpeg;

public class DataParameterAVCodecIDPropertyEditorSlot : DataParameterEnumPropertyEditorSlot<AVCodecID>
{
    public static DataParameterEnumInfo<AVCodecID> CodedIdEnumInfo { get; }
    
    public DataParameterAVCodecIDPropertyEditorSlot(DataParameter<AVCodecID> parameter, Type applicableType, string displayName, IEnumerable<AVCodecID>? values = null, DataParameterEnumInfo<AVCodecID>? translationInfo = null) : base(parameter, applicableType, displayName, values, translationInfo) { }
    public DataParameterAVCodecIDPropertyEditorSlot(DataParameter<AVCodecID> parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName ?? "Codec ID", EnumValues.OrderBy(x => x.ToString()), CodedIdEnumInfo) { }

    static DataParameterAVCodecIDPropertyEditorSlot()
    {
        Dictionary<AVCodecID, string> coded2name = new Dictionary<AVCodecID, string>();
        foreach (AVCodecID codec in EnumValues)
            coded2name[codec] = codec.ToString().Substring(12);

        CodedIdEnumInfo = new DataParameterEnumInfo<AVCodecID>(EnumValues.OrderBy(x => x.ToString()), coded2name);
    }
}