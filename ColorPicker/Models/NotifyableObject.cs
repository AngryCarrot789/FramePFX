﻿using System;
using System.ComponentModel;

namespace ColorPicker.Models {
    public class NotifyableObject : INotifyPropertyChanged {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate {
        };

        public void RaisePropertyChanged(string property) {
            if (property != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}