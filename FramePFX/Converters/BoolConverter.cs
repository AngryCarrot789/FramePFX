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
using System.Windows.Media;
using FramePFX.Utils;

namespace FramePFX.Converters {
    public class BoolConverter : IValueConverter {
        public object TrueValue { get; set; }

        public object FalseValue { get; set; }

        public object UnsetValue { get; set; }

        public object NonBoolValue { get; set; }

        public bool ThrowForUnset { get; set; }

        public bool ThrowForNonBool { get; set; }

        public BoolConverter() {
            this.UnsetValue = DependencyProperty.UnsetValue;
            this.NonBoolValue = DependencyProperty.UnsetValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolean) {
                return boolean ? this.TrueValue : this.FalseValue;
            }
            else if (value == DependencyProperty.UnsetValue) {
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
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : DependencyProperty.UnsetValue;
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

    public class BoolToVisibilityConverter : BoolConverter {
        public static BoolToVisibilityConverter BoolToVisibleOrCollapsed { get; } = new BoolToVisibilityConverter();
        public static BoolToVisibilityConverter BoolToVisibleOrHidden { get; } = new BoolToVisibilityConverter() {FalseValue = Visibility.Hidden};

        public new Visibility TrueValue {
            get => (Visibility) base.TrueValue;
            set => base.TrueValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility FalseValue {
            get => (Visibility) base.FalseValue;
            set => base.FalseValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility UnsetValue {
            // get will throw by default... cast to base type to set
            get => base.UnsetValue is Visibility v ? v : Visibility.Collapsed;
            set => base.UnsetValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility NonBoolValue {
            // get will throw by default... cast to base type to set
            get => base.NonBoolValue is Visibility v ? v : Visibility.Collapsed;
            set => base.NonBoolValue = NullToVisibilityConverter.Box(value);
        }

        public BoolToVisibilityConverter() {
            this.TrueValue = Visibility.Visible;
            this.FalseValue = Visibility.Collapsed;
        }
    }

    public class VisibilityToBoolConverter : IValueConverter {
        public static VisibilityToBoolConverter VisibleOrCollapsed { get; } = new VisibilityToBoolConverter() {VisibleValue = BoolBox.True, HiddenValue = null, CollapsedValue = BoolBox.False};
        public static VisibilityToBoolConverter VisibleOrHidden { get; } = new VisibilityToBoolConverter() {VisibleValue = BoolBox.False, HiddenValue = BoolBox.True, CollapsedValue = null};

        public object VisibleValue { get; set; }

        public object HiddenValue { get; set; }

        public object CollapsedValue { get; set; }

        public object UnsetValue { get; set; }

        public object NonVisibilityValue { get; set; }

        public bool ThrowForUnset { get; set; }

        public bool ThrowForNonVisibility { get; set; }

        public VisibilityToBoolConverter() {
            this.UnsetValue = DependencyProperty.UnsetValue;
            this.NonVisibilityValue = DependencyProperty.UnsetValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Visibility v) {
                switch (v) {
                    case Visibility.Visible: return this.VisibleValue;
                    case Visibility.Hidden: return this.HiddenValue;
                    case Visibility.Collapsed: return this.CollapsedValue;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            else if (value == DependencyProperty.UnsetValue) {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : this.UnsetValue;
            }
            else if (this.ThrowForNonVisibility) {
                throw new Exception("Expected visibility, got " + value);
            }
            else {
                return this.NonVisibilityValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (Equals(value, this.VisibleValue)) {
                return Visibility.Visible;
            }
            else if (Equals(value, this.HiddenValue)) {
                return Visibility.Hidden;
            }
            else if (Equals(value, this.CollapsedValue)) {
                return Visibility.Collapsed;
            }
            else if (Equals(value, this.UnsetValue)) {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : DependencyProperty.UnsetValue;
            }
            else if (this.ThrowForNonVisibility) {
                throw new Exception("Expected boolean, got " + value);
            }
            else {
                throw new Exception("Cannot convert back from " + value);
            }
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
}