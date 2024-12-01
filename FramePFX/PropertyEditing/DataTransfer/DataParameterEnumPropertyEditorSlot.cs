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

namespace FramePFX.PropertyEditing.DataTransfer;

public class DataParameterEnumInfo<TEnum> where TEnum : Enum {
    public readonly ReadOnlyCollection<(TEnum, string)> AllowedEnumList;
    public readonly IReadOnlyDictionary<TEnum, string> EnumToText;
    public readonly IReadOnlyDictionary<string, TEnum> TextToEnum;

    public IEnumerable<TEnum> EnumList => this.AllowedEnumList.Select(x => x.Item1);
    public IEnumerable<string> TextList => this.AllowedEnumList.Select(x => x.Item2);

    public DataParameterEnumInfo() : this(typeof(TEnum).GetEnumValues().Cast<TEnum>()) {
    }

    public DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues) {
        Validate.NotNull(allowedEnumValues);

        this.AllowedEnumList = allowedEnumValues.Select(x => (x, x.ToString())).ToList().AsReadOnly();
        this.EnumToText = new Dictionary<TEnum, string>(this.AllowedEnumList.Select(x => new KeyValuePair<TEnum, string>(x.Item1, x.Item2)));
        this.TextToEnum = new Dictionary<string, TEnum>(this.AllowedEnumList.Select(x => new KeyValuePair<string, TEnum>(x.Item2, x.Item1)));
    }

    public DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues, IReadOnlyDictionary<TEnum, string> enumToTextMap) {
        Validate.NotNull(allowedEnumValues);
        Validate.NotNull(enumToTextMap);

        this.AllowedEnumList = allowedEnumValues.Select(x => (x, enumToTextMap.TryGetValue(x, out string? value) ? value : x.ToString())).ToList().AsReadOnly();

        // Generate missing translations
        Dictionary<TEnum, string> fullEnumToTextMap = new Dictionary<TEnum, string>(enumToTextMap);
        foreach ((TEnum theEnum, string theName) t in this.AllowedEnumList) {
            if (!fullEnumToTextMap.ContainsKey(t.theEnum))
                fullEnumToTextMap[t.theEnum] = t.theName;
        }

        this.EnumToText = fullEnumToTextMap.AsReadOnly();
        this.TextToEnum = new Dictionary<string, TEnum>(this.EnumToText.Select(x => new KeyValuePair<string, TEnum>(x.Value, x.Key)));
    }
}

public class DataParameterEnumPropertyEditorSlot<TEnum> : DataParameterPropertyEditorSlot where TEnum : Enum {
    public static readonly ReadOnlyCollection<TEnum> Values = typeof(TEnum).GetEnumValues().Cast<TEnum>().ToList().AsReadOnly();
    public static readonly IReadOnlySet<TEnum> ValuesSet = new HashSet<TEnum>(Values);

    public readonly DataParameterEnumInfo<TEnum>? TranslationInfo;

    /// <summary>
    /// An enumerable which returns the allowed enum values
    /// </summary>
    public ReadOnlyCollection<TEnum> ValueEnumerable { get; }

    private TEnum value;

    public TEnum Value {
        get => this.value;
        set {
            if (!ValuesSet.Contains(value))
                value = Values[0];

            if (EqualityComparer<TEnum>.Default.Equals(value, this.value))
                return;

            this.value = value;
            DataParameter<TEnum> parameter = this.Parameter;
            for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                parameter.SetValue((ITransferableData) this.Handlers[i], value);
            }

            this.OnValueChanged(false, true);
        }
    }

    public TEnum? DefaultValue => ReferenceEquals(this.ValueEnumerable, Values) ? Values[0] : this.ValueEnumerable.FirstOrDefault();

    public new DataParameter<TEnum> Parameter => (DataParameter<TEnum>) base.Parameter;

    public DataParameterEnumPropertyEditorSlot(DataParameter<TEnum> parameter, Type applicableType, string displayName, IEnumerable<TEnum>? values = null, DataParameterEnumInfo<TEnum>? translationInfo = null) : base(parameter, applicableType, displayName) {
        this.TranslationInfo = translationInfo;
        this.ValueEnumerable = values != null ? values.ToList().AsReadOnly() : Values;
    }

    public override void QueryValueFromHandlers() {
        TEnum? val = CollectionUtils.GetEqualValue(this.Handlers, (x) => this.Parameter.GetValue((ITransferableData) x), out TEnum? d) ? d : default;
        this.value = ValuesSet.Contains(val) ? val : Values[0];
    }
}