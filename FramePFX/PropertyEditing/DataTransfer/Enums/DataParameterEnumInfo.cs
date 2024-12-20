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

using System.Collections.ObjectModel;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.DataTransfer.Enums;

/// <summary>
/// Information about an enum set
/// </summary>
/// <typeparam name="TEnum">Type of enum</typeparam>
public class DataParameterEnumInfo<TEnum> where TEnum : struct, Enum
{
    public readonly ReadOnlyCollection<(TEnum, string)> AllowedEnumList;
    public readonly IReadOnlyDictionary<TEnum, string> EnumToText;
    public readonly IReadOnlyDictionary<string, TEnum> TextToEnum;

    public IEnumerable<TEnum> EnumList => this.AllowedEnumList.Select(x => x.Item1);
    public IEnumerable<string> TextList => this.AllowedEnumList.Select(x => x.Item2);

    public DataParameterEnumInfo() : this(null) {
    }

    public DataParameterEnumInfo(IEnumerable<TEnum>? allowedEnumValues)
    {
        if (allowedEnumValues == null)
            allowedEnumValues = typeof(TEnum).GetEnumValues().Cast<TEnum>();

        this.AllowedEnumList = allowedEnumValues.Select(x => (x, x.ToString())).ToList().AsReadOnly();
        this.EnumToText = new Dictionary<TEnum, string>(this.AllowedEnumList.Select(x => new KeyValuePair<TEnum, string>(x.Item1, x.Item2)));
        this.TextToEnum = new Dictionary<string, TEnum>(this.AllowedEnumList.Select(x => new KeyValuePair<string, TEnum>(x.Item2, x.Item1)));
    }

    public DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues, IReadOnlyDictionary<TEnum, string> enumToTextMap)
    {
        Validate.NotNull(allowedEnumValues);
        Validate.NotNull(enumToTextMap);

        this.AllowedEnumList = allowedEnumValues.Select(x => (x, enumToTextMap.TryGetValue(x, out string? value) ? value : x.ToString())).ToList().AsReadOnly();

        // Generate missing translations
        Dictionary<TEnum, string> fullEnumToTextMap = new Dictionary<TEnum, string>(enumToTextMap);
        foreach ((TEnum theEnum, string theName) t in this.AllowedEnumList)
        {
            if (!fullEnumToTextMap.ContainsKey(t.theEnum))
                fullEnumToTextMap[t.theEnum] = t.theName;
        }

        this.EnumToText = fullEnumToTextMap.AsReadOnly();
        this.TextToEnum = new Dictionary<string, TEnum>(this.EnumToText.Select(x => new KeyValuePair<string, TEnum>(x.Value, x.Key)));
    }
}