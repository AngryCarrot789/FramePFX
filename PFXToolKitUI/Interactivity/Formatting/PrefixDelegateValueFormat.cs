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

namespace PFXToolKitUI.Interactivity.Formatting;

public class PrefixDelegateValueFormat : IValueFormatter {
    public string Prefix { get; }

    public IValueFormatter Formatter { get; }

    public event EventHandler? InvalidateFormat {
        add => this.Formatter.InvalidateFormat += value;
        remove => this.Formatter.InvalidateFormat -= value;
    }

    public PrefixDelegateValueFormat(IValueFormatter formatter, string prefix) {
        this.Formatter = formatter;
        this.Prefix = prefix;
    }

    public string ToString(double value, bool isEditing) {
        return this.Prefix + this.Formatter.ToString(value, isEditing);
    }

    public bool TryConvertToDouble(string format, out double value) {
        int i = 0, j = format.Length;
        if (!string.IsNullOrEmpty(this.Prefix) && format.StartsWith(this.Prefix)) {
            i += this.Prefix.Length;
        }

        if (i >= j) {
            value = default;
            return false;
        }

        return this.Formatter.TryConvertToDouble(i == 0 ? format : format.Substring(i, j - i), out value);
    }
}

public class PlusMinusValueFormat : IValueFormatter {
    public IValueFormatter Formatter { get; }

    public event EventHandler? InvalidateFormat {
        add => this.Formatter.InvalidateFormat += value;
        remove => this.Formatter.InvalidateFormat -= value;
    }

    public PlusMinusValueFormat(IValueFormatter formatter) {
        this.Formatter = formatter;
    }

    public string ToString(double value, bool isEditing) {
        return (value < 0.0 ? "-" : "+") + this.Formatter.ToString(value, isEditing);
    }

    public bool TryConvertToDouble(string format, out double value) {
        int i = 0, j = format.Length;
        if (j > 0 && format[0] == '+' || format[0] == '-')
            i++;

        if (i >= j) {
            value = default;
            return false;
        }

        return this.Formatter.TryConvertToDouble(format.Substring(i, j - i), out value);
    }
}