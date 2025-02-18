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

using System.Globalization;
using Avalonia.Data.Converters;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Converters;

public class NullConverter : IValueConverter {
    public object NullValue { get; set; }
    public object NonNullValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value == null ? this.NullValue : this.NonNullValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}

public class NullToBoolConverter : NullConverter {
    public static NullToBoolConverter NullToFalse { get; } = new NullToBoolConverter();
    public static NullToBoolConverter NullToTrue { get; } = new NullToBoolConverter() { NullValue = true, NonNullValue = false };

    public new bool NullValue {
        get => (bool) base.NullValue;
        set => base.NullValue = value.Box();
    }

    public new bool NonNullValue {
        get => (bool) base.NonNullValue;
        set => base.NonNullValue = value.Box();
    }

    public NullToBoolConverter() {
        base.NullValue = BoolBox.False;
        base.NonNullValue = BoolBox.True;
    }
}