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
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.AdvancedMenuService;

namespace PFXToolKitUI.Avalonia.Themes.Controls;

public class WindowEx : Window {
    public static readonly StyledProperty<IBrush?> TitleBarBrushProperty = AvaloniaProperty.Register<WindowEx, IBrush?>(nameof(TitleBarBrush));
    public static readonly StyledProperty<TextAlignment> TitleBarTextAlignmentProperty = AvaloniaProperty.Register<WindowEx, TextAlignment>(nameof(TitleBarTextAlignment));
    public static readonly StyledProperty<TopLevelMenuRegistry?> TitleBarMenuRegistryProperty = AvaloniaProperty.Register<WindowEx, TopLevelMenuRegistry?>(nameof(TitleBarMenuRegistry));

    public IBrush? TitleBarBrush {
        get => this.GetValue(TitleBarBrushProperty);
        set => this.SetValue(TitleBarBrushProperty, value);
    }

    public TextAlignment TitleBarTextAlignment {
        get => this.GetValue(TitleBarTextAlignmentProperty);
        set => this.SetValue(TitleBarTextAlignmentProperty, value);
    }

    public TopLevelMenuRegistry? TitleBarMenuRegistry {
        get => this.GetValue(TitleBarMenuRegistryProperty);
        set => this.SetValue(TitleBarMenuRegistryProperty, value);
    }

    // Override it here so that any window using WindowEx gets the automatic WindowEx style
    protected override Type StyleKeyOverride => typeof(WindowEx);

    private bool isAwaitingClose, isStillAwaiting, userCancelledClose, doFinalClose;

    public WindowEx() {
        if (AvUtils.TryGetService(out Win32PlatformOptions options)) {
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
    }

    protected sealed override void OnClosing(WindowClosingEventArgs e) {
        base.OnClosing(e);
        if (e.Cancel || this.doFinalClose) {
            return;
        }

        if (!this.isStillAwaiting) {
            if (this.isAwaitingClose) {
                // Someone tried to close the window during OnClosingAsync.
                e.Cancel = true;
            }
            else {
                this.OnClosingAsyncImpl(e.CloseReason);

                // If OnClosingAsync is still running, then we cancel the
                // close event, and we set a flag to close on task completed
                if (this.isAwaitingClose) {
                    this.isStillAwaiting = true;
                    e.Cancel = true;
                }
            }
        }

        this.userCancelledClose = false;
    }

    private async void OnClosingAsyncImpl(WindowCloseReason reason) {
        this.isAwaitingClose = true;
        try {
            this.userCancelledClose = await this.OnClosingAsync(reason);
        }
        catch (Exception e) {
            Dispatcher.UIThread.Post(() => throw e);
        }
        finally {
            this.isAwaitingClose = false;
        }

        bool postClose = this.isStillAwaiting && !this.userCancelledClose; 
        this.isStillAwaiting = false;
        this.userCancelledClose = false;
        
        if (postClose) {
            this.doFinalClose = true;
            Dispatcher.UIThread.Post(this.Close);
        }
    }

    /// <summary>
    /// Invoked when this window tries to close. This method supports full async
    /// </summary>
    /// <param name="reason">The close reason</param>
    /// <returns>True to cancel closing (do not close). False to allow the window to close (default value)</returns>
    protected virtual Task<bool> OnClosingAsync(WindowCloseReason reason) {
        return Task.FromResult(false);
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