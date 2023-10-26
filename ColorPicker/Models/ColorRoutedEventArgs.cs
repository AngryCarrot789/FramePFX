﻿using System.Windows;
using System.Windows.Media;

namespace ColorPicker.Models {
    public class ColorRoutedEventArgs : RoutedEventArgs {
        public ColorRoutedEventArgs(RoutedEvent routedEvent, Color color) : base(routedEvent) {
            this.Color = color;
        }

        public Color Color { get; private set; }
    }
}