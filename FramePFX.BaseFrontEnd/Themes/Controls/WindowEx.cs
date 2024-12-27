// 
// Copyright (c) 2024-2024 REghZy
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using FramePFX.BaseFrontEnd.Utils;

namespace FramePFX.BaseFrontEnd.Themes.Controls;

public class WindowEx : Window {
    public static readonly StyledProperty<IBrush?> TitleBarBrushProperty = AvaloniaProperty.Register<WindowEx, IBrush?>("TitleBarBrush");

    public IBrush? TitleBarBrush {
        get => this.GetValue(TitleBarBrushProperty);
        set => this.SetValue(TitleBarBrushProperty, value);
    }

    // Override it here so that any window using WindowEx gets the automatic WindowEx style
    protected override Type StyleKeyOverride => typeof(WindowEx);

    public WindowEx() {
        if (AvCore.TryGetService(out Win32PlatformOptions options)) {
            if (options.CompositionMode.Any(x => x == Win32CompositionMode.LowLatencyDxgiSwapChain)) {
                this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
                this.ExtendClientAreaToDecorationsHint = true;
                this.ExtendClientAreaTitleBarHeightHint = -1;
                return;
            }
        }

        this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
        this.ExtendClientAreaToDecorationsHint = true;
        this.ExtendClientAreaTitleBarHeightHint = -1;
    }

    static WindowEx() {
        // Window.ShowActivatedProperty
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild<Button>("PART_ButtonMinimize").Click += OnMinimizeButtonClick;
        e.NameScope.GetTemplateChild<Button>("PART_ButtonRestore").Click += OnRestoreButtonClick;
        e.NameScope.GetTemplateChild<Button>("PART_ButtonMaximize").Click += OnMaximizeButtonClick;
        e.NameScope.GetTemplateChild<Button>("PART_ButtonClose").Click += OnCloseButtonClick;
    }

    private static void OnMinimizeButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.WindowState = WindowState.Minimized;
    }

    private static void OnRestoreButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.WindowState = WindowState.Normal;
    }

    private static void OnMaximizeButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.WindowState = WindowState.Maximized;
    }

    private static void OnCloseButtonClick(object? sender, RoutedEventArgs e) {
        if (GetTopLevel(sender as Button) is WindowEx window)
            window.Close();
    }
}