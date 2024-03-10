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
using System.Windows;
using System.Windows.Data;

namespace FramePFX.Converters
{
    public class EmptyStringToBoolConverter : IValueConverter
    {
        public object NullValue { get; set; }

        public object EmptyValue { get; set; }

        public object NonEmptyValue { get; set; }

        public object NonStringValue { get; set; }
        public object UnsetValue { get; set; }

        public bool ThrowForUnset { get; set; }
        public bool ThrowForNonString { get; set; }

        public EmptyStringToBoolConverter()
        {
            this.UnsetValue = DependencyProperty.UnsetValue;
            this.NonStringValue = DependencyProperty.UnsetValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.Length < 1 ? this.EmptyValue : this.NonEmptyValue;
            }
            else if (value == DependencyProperty.UnsetValue)
            {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : this.UnsetValue;
            }
            else if (value == null)
            {
                return this.NullValue;
            }
            else if (this.ThrowForNonString)
            {
                throw new Exception("Expected string, got " + value);
            }
            else
            {
                return this.NonStringValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStringToVisibilityConverter : EmptyStringToBoolConverter
    {
        public static EmptyStringToVisibilityConverter Instance { get; } = new EmptyStringToVisibilityConverter();

        public new Visibility NullValue
        {
            get => (Visibility) base.NullValue;
            set => base.NullValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility EmptyValue
        {
            get => (Visibility) base.EmptyValue;
            set => base.EmptyValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility NonEmptyValue
        {
            get => (Visibility) base.NonEmptyValue;
            set => base.NonEmptyValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility NonStringValue
        {
            get => (Visibility) base.NonStringValue;
            set => base.NonStringValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility UnsetValue
        {
            get => (Visibility) base.UnsetValue;
            set => base.UnsetValue = NullToVisibilityConverter.Box(value);
        }

        public EmptyStringToVisibilityConverter()
        {
            base.NullValue = NullToVisibilityConverter.CollapsedBox;
            base.EmptyValue = NullToVisibilityConverter.CollapsedBox;
            base.NonEmptyValue = NullToVisibilityConverter.VisibleBox;
        }
    }
}