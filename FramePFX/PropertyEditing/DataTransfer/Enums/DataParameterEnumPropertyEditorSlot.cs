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

using System.Collections.ObjectModel;
using FramePFX.DataTransfer;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.DataTransfer.Enums;

public class DataParameterEnumPropertyEditorSlot<TEnum> : DataParameterPropertyEditorSlot where TEnum : struct, Enum
{
    public static readonly ReadOnlyCollection<TEnum> EnumValues = Enum.GetValues<TEnum>().ToList().AsReadOnly();
    public static readonly IReadOnlySet<TEnum> EnumValuesSet = new HashSet<TEnum>(EnumValues);

    public readonly DataParameterEnumInfo<TEnum>? TranslationInfo;

    /// <summary>
    /// An enumerable which returns the allowed enum values
    /// </summary>
    public ReadOnlyCollection<TEnum> ValueEnumerable { get; }

    private TEnum value;

    public TEnum Value
    {
        get => this.value;
        set
        {
            if (!EnumValuesSet.Contains(value))
                value = EnumValues[0];

            if (EqualityComparer<TEnum>.Default.Equals(value, this.value))
                return;

            this.value = value;
            DataParameter<TEnum> parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++)
            {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    public TEnum? DefaultValue => ReferenceEquals(this.ValueEnumerable, EnumValues) ? EnumValues[0] : this.ValueEnumerable.FirstOrDefault();

    public new DataParameter<TEnum> Parameter => (DataParameter<TEnum>) base.Parameter;

    public DataParameterEnumPropertyEditorSlot(DataParameter<TEnum> parameter, Type applicableType, string displayName, IEnumerable<TEnum>? values = null, DataParameterEnumInfo<TEnum>? translationInfo = null) : base(parameter, applicableType, displayName)
    {
        this.TranslationInfo = translationInfo;
        this.ValueEnumerable = values != null ? values.ToList().AsReadOnly() : EnumValues;
    }

    public override void QueryValueFromHandlers()
    {
        TEnum? val = CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out TEnum? d) ? d : default;
        this.value = val.HasValue && EnumValuesSet.Contains(val.Value) ? val.Value : EnumValues[0];
    }
}