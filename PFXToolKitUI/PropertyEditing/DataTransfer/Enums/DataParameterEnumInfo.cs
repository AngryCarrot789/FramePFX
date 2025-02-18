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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

/// <summary>
/// Information about an enum set
/// </summary>
/// <typeparam name="TEnum">Type of enum</typeparam>
public class DataParameterEnumInfo<TEnum> where TEnum : struct, Enum {
    public static readonly ReadOnlyCollection<TEnum> EnumValues = Enum.GetValues<TEnum>().ToList().AsReadOnly();
    public static readonly IReadOnlySet<TEnum> EnumValuesSet = new HashSet<TEnum>(EnumValues);
    public static readonly ReadOnlyCollection<TEnum> EnumValuesOrderedByName = Enum.GetValues<TEnum>().OrderBy(x => x.ToString()).ToList().AsReadOnly();
    public static readonly TEnum DefaultValue = default(TEnum);

    /// <summary>
    /// Returns a list of allowed enum values which are also mapped to a readable string name
    /// </summary>
    public readonly ReadOnlyCollection<(TEnum, string)> AllowedEnumList;

    /// <summary>
    /// Returns a dictionary which maps enum values to a string
    /// </summary>
    public readonly IReadOnlyDictionary<TEnum, string> EnumToText;

    /// <summary>
    /// Returns a dictionary which maps a string to an enum
    /// </summary>
    public readonly IReadOnlyDictionary<string, TEnum> TextToEnum;

    /// <summary>
    /// Returns a list of allowed enum values
    /// </summary>
    public IEnumerable<TEnum> EnumList => this.AllowedEnumList.Select(x => x.Item1);

    /// <summary>
    /// Returns a list of allowed enums, as their readable string name
    /// </summary>
    public IEnumerable<string> TextList => this.AllowedEnumList.Select(x => x.Item2);

    private DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues) {
        this.AllowedEnumList = allowedEnumValues.Select(x => (x, x.ToString())).ToList().AsReadOnly();
        this.EnumToText = new Dictionary<TEnum, string>(this.AllowedEnumList.Select(x => new KeyValuePair<TEnum, string>(x.Item1, x.Item2)));
        this.TextToEnum = new Dictionary<string, TEnum>(this.AllowedEnumList.Select(x => new KeyValuePair<string, TEnum>(x.Item2, x.Item1)));
    }

    private DataParameterEnumInfo(IEnumerable<TEnum> allowedEnumValues, IReadOnlyDictionary<TEnum, string> enumToTextMap) {
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

    /// <summary>
    /// Returns enum info for all enum constants of the enum type
    /// </summary>
    public static DataParameterEnumInfo<TEnum> All() {
        return new DataParameterEnumInfo<TEnum>(typeof(TEnum).GetEnumValues().Cast<TEnum>());
    }

    /// <summary>
    /// Returns enum info for all enum constants of the enum type, with enum to readable name mappings
    /// </summary>
    /// <param name="enumToTextMap"></param>
    public static DataParameterEnumInfo<TEnum> All(IReadOnlyDictionary<TEnum, string> enumToTextMap) {
        return new DataParameterEnumInfo<TEnum>(typeof(TEnum).GetEnumValues().Cast<TEnum>(), enumToTextMap);
    }

    /// <summary>
    /// Returns enum info for the list of allowed enum values
    /// </summary>
    /// <param name="allowedEnumValues">The allowed enums</param>
    public static DataParameterEnumInfo<TEnum> FromAllowed(IEnumerable<TEnum> allowedEnumValues) {
        return new DataParameterEnumInfo<TEnum>(allowedEnumValues.Distinct());
    }

    /// <summary>
    /// Returns enum info for the list of allowed enum values, with enum to readable name mappings
    /// </summary>
    /// <param name="allowedEnumValues">The allowed enums</param>
    public static DataParameterEnumInfo<TEnum> FromAllowed(IEnumerable<TEnum> allowedEnumValues, IReadOnlyDictionary<TEnum, string> enumToTextMap) {
        return new DataParameterEnumInfo<TEnum>(allowedEnumValues.Distinct(), enumToTextMap);
    }
}