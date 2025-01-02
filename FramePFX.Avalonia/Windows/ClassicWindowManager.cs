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
using Avalonia.Controls;
using Avalonia.Media;
using FramePFX.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Windows;

namespace FramePFX.Avalonia.Windows;

/// <summary>
/// Classic desktop window manager, which supports standard windows and popups
/// </summary>
public class ClassicWindowManager : WindowManager {
    public ClassicWindowManager(){
    }

    public override IWindow CreateWindow() {
        return null;
    }

    public override IDialog CreateDialog() {
        return null;
    }

    private class ClassicWindowBaseImpl : IWindowBase {
        private Window? window;
        private double? MyMinWidth;
        private double? MyMinHeight;
        private double? MyMaxWidth;
        private double? MyMaxHeight;
        private double? MyWidth;
        private double? MyHeight;
        
        public ContextRegistry ContextRegistry { get; set; }
        
        public TextAlignment TitleBarAlignment { get; set; }
        
        public string Title { get; set; }

        public bool IsVisible { get; private set; }
        
        public double MinWidth {
            get => this.window?.MinWidth ?? this.MyMinWidth ?? double.NaN;
            set { if (this.window != null) this.window.MinWidth = value; else this.MyMinWidth = value; }
        }
        
        public double MinHeight {
            get => this.window?.MinHeight ?? this.MyMinHeight ?? double.NaN;
            set { if (this.window != null) this.window.MinHeight = value; else this.MyMinHeight = value; }
        }
        
        public double MaxWidth {
            get => this.window?.MaxWidth ?? this.MyMaxWidth ?? double.NaN;
            set { if (this.window != null) this.window.MaxWidth = value; else this.MyMaxWidth = value; }
        }
        
        public double MaxHeight {
            get => this.window?.MaxHeight ?? this.MyMaxHeight ?? double.NaN;
            set { if (this.window != null) this.window.MaxHeight = value; else this.MyMaxHeight = value; }
        }
        
        public double Width {
            get => this.window?.Width ?? this.MyWidth ?? double.NaN;
            set { if (this.window != null) this.window.Width = value; else this.MyWidth = value; }
        }
        
        public double Height {
            get => this.window?.Height ?? this.MyHeight ?? double.NaN;
            set { if (this.window != null) this.window.Height = value; else this.MyHeight = value; }
        }

        public ClassicWindowBaseImpl() {
        }

        public void CreateWindow() {
            if (this.window != null) {
                throw new InvalidOperationException("Window already exists");
            }
        }

        public void Close() {
            if (this.window == null)
                throw new InvalidOperationException("Window not shown yet");
        }
    }
}