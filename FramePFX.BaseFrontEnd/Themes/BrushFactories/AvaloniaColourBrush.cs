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

using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Reactive;
using Avalonia.Styling;
using FramePFX.Themes;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Themes.BrushFactories;

/// <summary>
/// The primary base class for all avalonia implementations of <see cref="IColourBrush"/>
/// </summary>
public abstract class AvaloniaColourBrush : IColourBrush {
    
}

public class ImmutableAvaloniaColourBrush : AvaloniaColourBrush {
    public ImmutableSolidColorBrush Brush { get; }

    public ImmutableAvaloniaColourBrush(SKColor colour) {
        this.Brush = new ImmutableSolidColorBrush(new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue));
    }
}

public class DynamicResourceAvaloniaColourBrush : AvaloniaColourBrush {
    public string ThemeKey { get; set; }

    /// <summary>
    /// Gets the fully resolved
    /// </summary>
    public IBrush? CurrentBrush { get; private set; }

    private AnonymousObserver<AvaloniaPropertyChangedEventArgs<ThemeVariant>>? myThemeChangeHandler;
    private int usageCounter;
    private List<Action<IBrush?>>? handlers;

    public DynamicResourceAvaloniaColourBrush(string themeKey) {
        this.ThemeKey = themeKey;
    }

    public IDisposable Subscribe(Action<IBrush?>? onBrushChanged) {
        if (onBrushChanged != null)
            (this.handlers ??= new List<Action<IBrush?>>()).Add(onBrushChanged);

        if (this.usageCounter++ == 0) {
            global::Avalonia.Application.Current!.ActualThemeVariantChanged += this.OnApplicationThemeChanged;
            this.QueryBrushFromApplication();
        }

        return new UsageToken(this, onBrushChanged);
    }

    private void OnApplicationThemeChanged(object? sender, EventArgs e) {
        this.QueryBrushFromApplication();
    }

    private void Unsubscribe(Action<IBrush?>? invalidatedHandler) {
        if (this.usageCounter == 0) {
            throw new InvalidOperationException("Excessive unsubscribe count");
        }

        if (invalidatedHandler != null) {
            Debug.Assert(this.handlers != null);
            this.handlers.Remove(invalidatedHandler);
        }

        if (--this.usageCounter == 0) {
            global::Avalonia.Application.Current!.ActualThemeVariantChanged -= this.OnApplicationThemeChanged;
        }
    }

    private void QueryBrushFromApplication() {
        if (global::Avalonia.Application.Current!.TryGetResource(this.ThemeKey, global::Avalonia.Application.Current.ActualThemeVariant, out object? value)) {
            if (value is IBrush brush) {
                if (!ReferenceEquals(this.CurrentBrush, brush)) {
                    this.CurrentBrush = brush;
                    this.NotifyHandlersBrushChanged();
                }

                return;
            }
        }
        
        this.CurrentBrush = null;
        this.NotifyHandlersBrushChanged();
    }

    private void NotifyHandlersBrushChanged() {
        if (this.handlers != null) {
            foreach (Action<IBrush?> action in this.handlers) {
                action(this.CurrentBrush);
            }
        }
    }

    private class UsageToken : IDisposable {
        private DynamicResourceAvaloniaColourBrush? brush;
        private readonly Action<IBrush?>? invalidatedHandler;

        public UsageToken(DynamicResourceAvaloniaColourBrush brush, Action<IBrush?>? invalidatedHandler) {
            this.brush = brush;
            this.invalidatedHandler = invalidatedHandler;
        }

        public void Dispose() {
            if (this.brush == null)
                throw new ObjectDisposedException("this", "Already disposed");

            this.brush.Unsubscribe(this.invalidatedHandler);
            this.brush = null;
        }
    }
}