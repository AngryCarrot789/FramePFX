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

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Converters;

public class BoolConverter : IValueConverter {
    public object TrueValue { get; set; }

    public object FalseValue { get; set; }

    public object UnsetValue { get; set; }

    public object NonBoolValue { get; set; }

    public bool ThrowForUnset { get; set; }

    public bool ThrowForNonBool { get; set; }

    public BoolConverter() {
        this.UnsetValue = AvaloniaProperty.UnsetValue;
        this.NonBoolValue = AvaloniaProperty.UnsetValue;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is bool boolean) {
            return boolean ? this.TrueValue : this.FalseValue;
        }
        else if (value == AvaloniaProperty.UnsetValue) {
            return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : this.UnsetValue;
        }
        else if (this.ThrowForNonBool) {
            throw new Exception("Expected boolean, got " + value);
        }
        else {
            return this.NonBoolValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        if (Equals(value, this.TrueValue)) {
            return true;
        }
        else if (Equals(value, this.FalseValue)) {
            return false;
        }
        else if (Equals(value, this.UnsetValue)) {
            return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : AvaloniaProperty.UnsetValue;
        }
        else if (this.ThrowForNonBool) {
            throw new Exception("Expected boolean, got " + value);
        }
        else {
            throw new Exception("Cannot convert back from " + value);
        }
    }
}

public class InvertBoolConverter : BoolConverter {
    public static InvertBoolConverter Instance { get; } = new InvertBoolConverter();

    public InvertBoolConverter() {
        this.TrueValue = BoolBox.False;
        this.FalseValue = BoolBox.True;
    }
}

public class BoolToBrushConverter : BoolConverter {
    public new Brush TrueValue {
        get => (Brush) base.TrueValue;
        set => base.TrueValue = value;
    }

    public new Brush FalseValue {
        get => (Brush) base.FalseValue;
        set => base.FalseValue = value;
    }

    public BoolToBrushConverter() {
        this.TrueValue = null;
        this.FalseValue = null;
    }
}

public class BoolToColourConverter : BoolConverter {
    public new Color TrueValue {
        get => (Color) base.TrueValue;
        set => base.TrueValue = value;
    }

    public new Color FalseValue {
        get => (Color) base.FalseValue;
        set => base.FalseValue = value;
    }

    public BoolToColourConverter() {
        this.TrueValue = Colors.Black;
        this.FalseValue = Colors.Black;
    }
}

public class BoolToDoubleConverter : BoolConverter {
    public new double TrueValue {
        get => (double) base.TrueValue;
        set => base.TrueValue = value;
    }

    public new double FalseValue {
        get => (double) base.FalseValue;
        set => base.FalseValue = value;
    }

    public BoolToDoubleConverter() {
        this.TrueValue = 1.0d;
        this.FalseValue = 0.0d;
    }
}