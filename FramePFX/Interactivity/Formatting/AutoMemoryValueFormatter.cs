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

using System.Collections.Immutable;
using FramePFX.Utils;

namespace FramePFX.Interactivity.Formatting;

public delegate void AutoMemoryFormatFormatterEventHandler(AutoMemoryValueFormatter sender);

/// <summary>
/// A value formatter that formats memory (e.g. bits, bytes, kbits, etc.) automatically
/// into an appropriate format based on the actual numerical value (e.g. 1000 bits into 1kbit)
/// </summary>
public class AutoMemoryValueFormatter : BaseSimpleValueFormatter
{
    private MemoryFormatType sourceFormat = MemoryFormatType.Byte;

    /// <summary>
    /// Gets the format that is provided as a double to the <see cref="ToString"/> method (e.g. byte to megabyte, this value is byte)
    /// </summary>
    public MemoryFormatType SourceFormat
    {
        get => this.sourceFormat;
        set
        {
            if (this.sourceFormat == value)
                return;

            MemoryValueFormatter.ValidateMemoryFormat(value);
            this.sourceFormat = value;
            this.SourceFormatChanged?.Invoke(this);
            this.OnInvalidateFormat();
        }
    }

    public ImmutableHashSet<MemoryFormatType>? AllowedFormats { get; set; }

    public event AutoMemoryFormatFormatterEventHandler? SourceFormatChanged;

    public AutoMemoryValueFormatter(int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6)
    {
        this.NonEditingRoundedPlaces = nonEditingRoundedPlaces;
        this.EditingRoundedPlaces = editingRoundedPlaces;
    }

    public AutoMemoryValueFormatter(IReadOnlySet<MemoryFormatType> allowedFormats, int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6) : this(nonEditingRoundedPlaces, editingRoundedPlaces)
    {
        this.AllowedFormats = allowedFormats as ImmutableHashSet<MemoryFormatType> ?? allowedFormats.ToImmutableHashSet();
    }

    // TODO: IDragStepProfileProvider
    // which needs to communicate with this formatter to read
    // the last optimalFormat in order to change the increment
    // value based on which unit is being displayed

    public override string ToString(double value, bool isEditing)
    {
        double sourceFormatBytes = MemoryValueFormatter.ConversionTable[this.sourceFormat];
        double valueInBytes = value * sourceFormatBytes;
        MemoryFormatType optimalFormat = GetOptimalFormat(valueInBytes, this.AllowedFormats);

        double optimalFormatBytes = MemoryValueFormatter.ConversionTable[optimalFormat];
        double outputValue = valueInBytes / optimalFormatBytes;
        string formatted = outputValue.ToString(isEditing ? this.EditingRoundedPlacesFormat : this.NonEditingRoundedPlacesFormat);
        return $"{formatted} {MemoryValueFormatter.GetFormatLabel(optimalFormat, DoubleUtils.AreClose(outputValue, 1.0))}";
    }

    public override bool TryConvertToDouble(string format, out double value)
    {
        ReadOnlySpan<char> valueText;

        // Try and parse a custom targetFormat from the string
        if (MemoryValueFormatter.ParseFormatFromLabel(format, out MemoryFormatType memoryFormat, out int suffixLength))
        {
            valueText = format.AsSpan(0, format.Length - suffixLength).Trim(); // remove whitespaces
        }
        else
        {
            // If the format is just a plain number, assume our sourceFormat
            memoryFormat = this.sourceFormat;
            valueText = format.Trim();
        }

        if (!double.TryParse(valueText, out double theOriginalOutput))
        {
            value = default;
            return false;
        }

        // Skip pointless calculations if we can
        if (memoryFormat == this.sourceFormat)
        {
            value = theOriginalOutput;
            return true;
        }

        double valueInBytes = theOriginalOutput * MemoryValueFormatter.ConversionTable[memoryFormat];
        value = valueInBytes / MemoryValueFormatter.ConversionTable[this.sourceFormat];
        return true;
    }

    public static MemoryFormatType GetOptimalFormat(double valueInBytes, IReadOnlySet<MemoryFormatType>? allowedFormats)
    {
        // Go back to front because if valueInBytes is 1000, ideally we want to select 1kbit, not something else
        foreach (MemoryValueFormatter.MemoryFormatConversion conversion in MemoryValueFormatter.Conversions)
        {
            if (allowedFormats != null && !allowedFormats.Contains(conversion.Format))
            {
                continue;
            }

            double thing = conversion.Bytes * 1000;
            if (valueInBytes < thing) // Stay within three digits
                return conversion.Format;
        }

        return MemoryFormatType.TebiByte1024; // Default to the largest unit if value is huuge
    }
}