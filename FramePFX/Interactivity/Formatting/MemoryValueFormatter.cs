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
using System.Globalization;
using System.Runtime.CompilerServices;
using FramePFX.Utils;

namespace FramePFX.Interactivity.Formatting;

public delegate void MemoryFormatFormatterEventHandler(MemoryValueFormatter sender);

/// <summary>
/// A value formatter that formats memory (e.g. bits, bytes, kbits, etc.) into another format
/// </summary>
public class MemoryValueFormatter : BaseSimpleValueFormatter
{
    /// <summary>
    /// Contains the bit, kilobit, megabit, gigabit and terabit formats
    /// </summary>
    public static readonly ImmutableHashSet<MemoryFormatType> Bits = new HashSet<MemoryFormatType>()
    {
        MemoryFormatType.Bit, MemoryFormatType.KiloBit, MemoryFormatType.MegaBit, MemoryFormatType.GigaBit, MemoryFormatType.TeraBit
    }.ToImmutableHashSet();

    /// <summary>
    /// Contains the byte, kilobyte, megabyte, gigabyte and terabyte formats. Kilo and above are 1000x the previous unit
    /// </summary>
    public static readonly ImmutableHashSet<MemoryFormatType> Bytes = new HashSet<MemoryFormatType>()
    {
        MemoryFormatType.Byte, MemoryFormatType.KiloByte1000, MemoryFormatType.MegaByte1000, MemoryFormatType.GigaByte1000, MemoryFormatType.TeraByte1000
    }.ToImmutableHashSet();

    /// <summary>
    /// Contains the byte, kibibyte, mebibyte, gibibyte and tebibyte formats. Kibi and above are 1024x the previous unit
    /// </summary>
    public static readonly ImmutableHashSet<MemoryFormatType> Bibis = new HashSet<MemoryFormatType>()
    {
        MemoryFormatType.Byte, MemoryFormatType.KibiByte1024, MemoryFormatType.MebiByte1024, MemoryFormatType.GibiByte1024, MemoryFormatType.TebiByte1024
    }.ToImmutableHashSet();

    public readonly struct MemoryFormatConversion
    {
        public readonly MemoryFormatType Format;
        public readonly double Bytes;

        public MemoryFormatConversion(MemoryFormatType format, double bytes)
        {
            this.Format = format;
            this.Bytes = bytes;
        }
    }

    public static readonly ImmutableList<MemoryFormatConversion> Conversions = new List<MemoryFormatConversion>()
    {
        new MemoryFormatConversion(MemoryFormatType.Bit, 1.0 / 8),
        new MemoryFormatConversion(MemoryFormatType.Byte, 1),
        new MemoryFormatConversion(MemoryFormatType.KiloBit, 1.0 / 8.0 * 1000.0),
        new MemoryFormatConversion(MemoryFormatType.KiloByte1000, 1 * 1000.0),
        new MemoryFormatConversion(MemoryFormatType.KibiByte1024, 1024.0),
        new MemoryFormatConversion(MemoryFormatType.MegaBit, 1.0 / 8.0 * 1000000.0),
        new MemoryFormatConversion(MemoryFormatType.MegaByte1000, 1 * 1000000.0),
        new MemoryFormatConversion(MemoryFormatType.MebiByte1024, 1024.0 * 1024.0),
        new MemoryFormatConversion(MemoryFormatType.GigaBit, 1.0 / 8.0 * 1000000000.0),
        new MemoryFormatConversion(MemoryFormatType.GigaByte1000, 1 * 1000000000.0),
        new MemoryFormatConversion(MemoryFormatType.GibiByte1024, 1024.0 * 1024.0 * 1024.0),
        new MemoryFormatConversion(MemoryFormatType.TeraBit, 1.0 / 8.0 * 1000000000.0),
        new MemoryFormatConversion(MemoryFormatType.TeraByte1000, 1 * 1000000000000.0),
        new MemoryFormatConversion(MemoryFormatType.TebiByte1024, 1024.0 * 1024.0 * 1024.0 * 1024.0)
    }.ToImmutableList();

    public static readonly Dictionary<MemoryFormatType, double> ConversionTable;

    private MemoryFormatType sourceFormat = MemoryFormatType.Byte;
    private MemoryFormatType targetFormat = MemoryFormatType.MegaByte1000;

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

            ValidateMemoryFormat(value);
            this.sourceFormat = value;
            this.SourceFormatChanged?.Invoke(this);
            this.OnInvalidateFormat();
        }
    }

    /// <summary>
    /// Gets the target format that is the output of the <see cref="ToString"/> method (e.g. byte to megabyte, this value is megabyte)
    /// </summary>
    public MemoryFormatType TargetFormat
    {
        get => this.targetFormat;
        set
        {
            if (this.targetFormat == value)
                return;

            ValidateMemoryFormat(value);
            this.targetFormat = value;
            this.TargetFormatChanged?.Invoke(this);
            this.OnInvalidateFormat();
        }
    }

    public ImmutableHashSet<MemoryFormatType>? AllowedFormats { get; set; }

    public event MemoryFormatFormatterEventHandler? SourceFormatChanged;
    public event MemoryFormatFormatterEventHandler? TargetFormatChanged;

    public MemoryValueFormatter(int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6)
    {
        this.NonEditingRoundedPlaces = nonEditingRoundedPlaces;
        this.EditingRoundedPlaces = editingRoundedPlaces;
    }

    public MemoryValueFormatter(IReadOnlySet<MemoryFormatType> allowedFormats, int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6) : this(nonEditingRoundedPlaces, editingRoundedPlaces)
    {
        this.AllowedFormats = allowedFormats as ImmutableHashSet<MemoryFormatType> ?? allowedFormats.ToImmutableHashSet();
    }

    static MemoryValueFormatter()
    {
        ConversionTable = new Dictionary<MemoryFormatType, double>();
        foreach (MemoryFormatConversion conversion in Conversions)
        {
            ConversionTable.Add(conversion.Format, conversion.Bytes);
        }
    }

    public override string ToString(double value, bool isEditing)
    {
        double valueInBytes = value * ConversionTable[this.sourceFormat];
        double outputValue = valueInBytes / ConversionTable[this.targetFormat];
        string formatted = outputValue.ToString(isEditing ? this.EditingRoundedPlacesFormat : this.NonEditingRoundedPlacesFormat);
        return $"{formatted} {GetFormatLabel(this.targetFormat, DoubleUtils.AreClose(outputValue, 1.0))}";
    }

    public override bool TryConvertToDouble(string format, out double value)
    {
        ReadOnlySpan<char> valueText;

        // Try and parse a custom targetFormat from the string
        if (ParseFormatFromLabel(format, out MemoryFormatType memoryFormat, out int suffixLength))
        {
            valueText = format.AsSpan(0, suffixLength).Trim(); // remove whitespaces
        }
        else
        {
            // If the format is just a plain number, assume our targetFormat as per usualy
            memoryFormat = this.targetFormat;
            valueText = format.Trim();
        }

        if (!double.TryParse(valueText, out double theOriginalOutput))
        {
            value = default;
            return false;
        }

        double valueInBytes = theOriginalOutput * ConversionTable[memoryFormat];
        value = valueInBytes / ConversionTable[this.sourceFormat];
        return true;
    }

    public static void ValidateMemoryFormat(MemoryFormatType format)
    {
        if (format < MemoryFormatType.Bit || format > MemoryFormatType.TebiByte1024)
        {
            throw new ArgumentOutOfRangeException(nameof(format), format, "Format is unrecognized");
        }
    }

    public static string GetFormatLabel(MemoryFormatType format, bool singular)
    {
        ValidateMemoryFormat(format);
        switch (format)
        {
            case MemoryFormatType.Bit: return singular ? "bit" : "bits";
            case MemoryFormatType.Byte: return singular ? "byte" : "bytes";
            case MemoryFormatType.KiloBit: return "KBit";
            case MemoryFormatType.KiloByte1000: return "KB";
            case MemoryFormatType.KibiByte1024: return "KiB";
            case MemoryFormatType.MegaBit: return "MBit";
            case MemoryFormatType.MegaByte1000: return "MB";
            case MemoryFormatType.MebiByte1024: return "MiB";
            case MemoryFormatType.GigaBit: return "GBit";
            case MemoryFormatType.GigaByte1000: return "GB";
            case MemoryFormatType.GibiByte1024: return "GiB";
            case MemoryFormatType.TeraBit: return "TBit";
            case MemoryFormatType.TeraByte1000: return "TB";
            case MemoryFormatType.TebiByte1024: return "TiB";
            default: throw new SwitchExpressionException(format);
        }
    }

    public static bool ParseFormatFromLabel(string input, out MemoryFormatType format, out int suffixLength)
    {
        // Maybe I've overdone this, especially on the goto usage... muahahah
        if (string.IsNullOrWhiteSpace(input))
        {
            format = default;
            suffixLength = 0;
            return false;
        }

        const StringComparison comparison = StringComparison.CurrentCultureIgnoreCase;
        if (input.Length > 1)
        {
            suffixLength = 2;
            switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
            {
                case "KB": format = MemoryFormatType.KiloByte1000; break;
                case "MB": format = MemoryFormatType.MegaByte1000; break;
                case "GB": format = MemoryFormatType.GigaByte1000; break;
                case "TB": format = MemoryFormatType.TeraByte1000; break;
                default: goto ScanIEC1; // Skip past 'return true'
            }

            return true;

            ScanIEC1:
            if (input.Length > 2)
            {
                suffixLength = 3;
                switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
                {
                    case "KIB": format = MemoryFormatType.KibiByte1024; break;
                    case "MIB": format = MemoryFormatType.MebiByte1024; break;
                    case "GIB": format = MemoryFormatType.GibiByte1024; break;
                    case "TIB": format = MemoryFormatType.TebiByte1024; break;
                    default: goto ScanIEC2; // Skip past 'return true'
                }

                return true;
            }

            ScanIEC2:
            if (input.Length > 3)
            {
                suffixLength = 4;
                switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
                {
                    case "KIBS": format = MemoryFormatType.KibiByte1024; break;
                    case "MIBS": format = MemoryFormatType.MebiByte1024; break;
                    case "GIBS": format = MemoryFormatType.GibiByte1024; break;
                    case "TIBS": format = MemoryFormatType.TebiByte1024; break;
                    case "KBIT": format = MemoryFormatType.KiloBit; break;
                    case "MBIT": format = MemoryFormatType.MegaBit; break;
                    case "GBIT": format = MemoryFormatType.GigaBit; break;
                    case "TBIT": format = MemoryFormatType.TeraBit; break;
                    default: goto ScanIEC3; // Skip past 'return true'
                }

                return true;
            }

            ScanIEC3:
            if (input.Length > 4)
            {
                suffixLength = 5;
                switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
                {
                    case "KBITS": format = MemoryFormatType.KiloBit; break;
                    case "MBITS": format = MemoryFormatType.MegaBit; break;
                    case "GBITS": format = MemoryFormatType.GigaBit; break;
                    case "TBITS": format = MemoryFormatType.TeraBit; break;
                    case "KIBIS": format = MemoryFormatType.KibiByte1024; break;
                    case "MIBIS": format = MemoryFormatType.MebiByte1024; break;
                    case "GIBIS": format = MemoryFormatType.GibiByte1024; break;
                    case "TIBIS": format = MemoryFormatType.TebiByte1024; break;
                    default: goto ScanIEC4; // Skip past 'return true'
                }

                return true;
            }

            ScanIEC4:
            if (input.Length > 6)
            {
                suffixLength = 7;
                switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
                {
                    case "KIBIBIT": format = MemoryFormatType.KibiByte1024; break;
                    case "MIBIBIT": format = MemoryFormatType.MebiByte1024; break;
                    case "GIBIBIT": format = MemoryFormatType.GibiByte1024; break;
                    case "TIBIBIT": format = MemoryFormatType.TebiByte1024; break;
                    case "KILOBIT": format = MemoryFormatType.KiloBit; break;
                    case "MEGABIT": format = MemoryFormatType.MegaBit; break;
                    case "GIGABIT": format = MemoryFormatType.GigaBit; break;
                    case "TERABIT": format = MemoryFormatType.TeraBit; break;
                    default: goto ScanIEC5; // Skip past 'return true'
                }

                return true;
            }

            ScanIEC5:
            if (input.Length > 7)
            {
                suffixLength = 8;
                switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
                {
                    case "KIBIBITS": format = MemoryFormatType.KibiByte1024; break;
                    case "MIBIBITS": format = MemoryFormatType.MebiByte1024; break;
                    case "GIBIBITS": format = MemoryFormatType.GibiByte1024; break;
                    case "TIBIBITS": format = MemoryFormatType.TebiByte1024; break;
                    case "KILOBITS": format = MemoryFormatType.KiloBit; break;
                    case "MEGABITS": format = MemoryFormatType.MegaBit; break;
                    case "GIGABITS": format = MemoryFormatType.GigaBit; break;
                    case "TERABITS": format = MemoryFormatType.TeraBit; break;
                    default: goto ScanIEC6; // Skip past 'return true'
                }

                return true;
            }

            ScanIEC6:
            if (input.Length > 8)
            {
                suffixLength = 9;
                switch (input.Substring(input.Length - suffixLength, suffixLength).ToUpper(CultureInfo.CurrentCulture))
                {
                    case "KILOBYTES": format = MemoryFormatType.KiloByte1000; break;
                    case "MEGABYTES": format = MemoryFormatType.MegaByte1000; break;
                    case "GIGABYTES": format = MemoryFormatType.GigaByte1000; break;
                    case "TERABYTES": format = MemoryFormatType.TeraByte1000; break;
                    default: goto ScanDirect; // Skip past 'return true'
                }

                return true;
            }
        }

        ScanDirect:
        // Scan these last since "KBit" or "gbits" may be matched as "bit" or "bits"
        if (input.EndsWith("bit", comparison))
        {
            format = MemoryFormatType.Bit;
            suffixLength = 3;
        }
        else if (input.EndsWith("bits", comparison))
        {
            format = MemoryFormatType.Bit;
            suffixLength = 4;
        }
        else if (input.EndsWith("byte", comparison))
        {
            format = MemoryFormatType.Byte;
            suffixLength = 4;
        }
        else if (input.EndsWith("bytes", comparison))
        {
            format = MemoryFormatType.Byte;
            suffixLength = 5;
        }
        else
        {
            suffixLength = 0;
            format = default;
            return false;
        }

        return true;
    }
}