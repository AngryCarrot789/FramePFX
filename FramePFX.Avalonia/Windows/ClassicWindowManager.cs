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
using Avalonia.Media;
using FramePFX.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Themes.Controls;
using FramePFX.BaseFrontEnd.Windows;

namespace FramePFX.Avalonia.Windows;

/// <summary>
/// Classic desktop window manager, which supports standard windows and popups
/// </summary>
public class ClassicWindowManager : NewWindowManager {
    public ClassicWindowManager() {
    }

    public override IWindow CreateWindow() {
        return null;
    }

    public override IDialog CreateDialog() {
        return null;
    }

    private class ClassicWindowBaseImpl : IWindowBase {
        private readonly WindowEx window;
        
        public ContextRegistry ContextRegistry { get; set; }
        
        public TextAlignment TitleBarAlignment { get; set; }
        
        public string Title { get; set; }

        public bool IsVisible { get; private set; }
        
        public double MinWidth {
            get => this.window.MinWidth;
            set => this.window.MinWidth = value;
        }
        
        public double MinHeight {
            get => this.window.MinHeight;
            set => this.window.MinHeight = value;
        }
        
        public double MaxWidth {
            get => this.window.MaxWidth;
            set => this.window.MaxWidth = value;
        }
        
        public double MaxHeight {
            get => this.window.MaxHeight;
            set => this.window.MaxHeight = value;
        }
        
        public double Width {
            get => this.window.Width;
            set => this.window.Width = value;
        }
        
        public double Height {
            get => this.window.Height;
            set => this.window.Height = value;
        }

        public ClassicWindowBaseImpl() {
            this.window = new WindowEx();
            this.window.Width = 24;
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