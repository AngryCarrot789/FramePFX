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
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Themes;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Themes.BrushFactories;

/// <summary>
/// The primary base class for all avalonia implementations of <see cref="IColourBrush"/>
/// </summary>
public abstract class AvaloniaColourBrush : IColourBrush {
    /// <summary>
    /// Gets the brush currently associated with this object
    /// </summary>
    public abstract IBrush? Brush { get; }
}

public sealed class ImmutableAvaloniaColourBrush : AvaloniaColourBrush {
    public override ImmutableSolidColorBrush Brush { get; }

    public ImmutableAvaloniaColourBrush(SKColor colour) {
        this.Brush = new ImmutableSolidColorBrush(new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue));
    }
}

public sealed class DynamicResourceAvaloniaColourBrush : AvaloniaColourBrush, IDynamicColourBrush {
    public string ThemeKey { get; }

    /// <summary>
    /// Gets the fully resolved brush
    /// </summary>
    public IBrush? CurrentBrush { get; private set; }

    public override IBrush? Brush => this.CurrentBrush;

    private int usageCounter;
    private List<Action<IBrush?>>? handlers;
    
    public event DynamicColourBrushChangedEventHandler? BrushChanged;

    public DynamicResourceAvaloniaColourBrush(string themeKey) {
        this.ThemeKey = themeKey;
    }

    /// <summary>
    /// Subscribe to this brush using the given brush handler. This is more like
    /// an "IncrementReferences" method, with an optional brush change handler.
    /// <para>
    /// The returned disposable unsubscribes and MUST be called when this brush is no longer
    /// in use, because otherwise this brush will always be listening to application
    /// theme and resource change events in order to notify listeners of brush changes 
    /// </para>
    /// </summary>
    /// <param name="onBrushChanged">An optional handler for when our internal brush changes for any reason</param>
    /// <param name="invokeHandlerImmediately">True to invoke the given handler in this method if we currently have a valid brush</param>
    /// <returns>A disposable to unsubscribe</returns>
    public IDisposable Subscribe(Action<IBrush?>? onBrushChanged, bool invokeHandlerImmediately = true) {
        Application.Instance.Dispatcher.VerifyAccess();
        
        if (onBrushChanged != null)
            (this.handlers ??= new List<Action<IBrush?>>()).Add(onBrushChanged);

        if (this.usageCounter++ == 0) {
            // We expect the handler list to be empty due to the logic in Unsubscribe
            Debug.Assert(onBrushChanged != null ? this.handlers?.Count == 1 : (this.handlers == null || this.handlers.Count < 1));
            
            Avalonia.Application.Current!.ActualThemeVariantChanged += this.OnApplicationThemeChanged;
            this.FindBrush(invokeHandlerImmediately);
        }
        else if (invokeHandlerImmediately && onBrushChanged != null && this.CurrentBrush != null) {
            onBrushChanged(this.CurrentBrush);
        }

        return new UsageToken(this, onBrushChanged);
    }

    private void OnApplicationThemeChanged(object? sender, EventArgs e) {
        this.FindBrush();
    }

    private void Unsubscribe(UsageToken token) {
        Application.Instance.Dispatcher.VerifyAccess();
        
        if (this.usageCounter == 0) {
            throw new InvalidOperationException("Excessive unsubscribe count");
        }

        if (token.invalidatedHandler != null) {
            Debug.Assert(this.handlers != null);
            this.handlers.Remove(token.invalidatedHandler);
        }

        if (--this.usageCounter == 0) {
            // Since token.invalidatedHandler cannot change and is readonly,
            // it should be impossible for it to not get removed
            Debug.Assert(this.handlers == null || this.handlers.Count < 1);
            
            Avalonia.Application.Current!.ActualThemeVariantChanged -= this.OnApplicationThemeChanged;
        }
    }

    private void FindBrush(bool notifyHandlers = true) {
        if (Avalonia.Application.Current!.TryGetResource(this.ThemeKey, Avalonia.Application.Current.ActualThemeVariant, out object? value)) {
            if (value is IBrush brush) {
                if (!ReferenceEquals(this.CurrentBrush, brush)) {
                    this.CurrentBrush = brush;
                    if (notifyHandlers)
                        this.NotifyHandlersBrushChanged();
                }

                return;
            }
            else {
                // Try to convert to an immutable brush in case the user specified a colour key
                if (value is Color colour) {
                    // Already have the same colour, and we have an immutable brush,
                    // so no need to notify handlers of changes
                    if (this.CurrentBrush is ImmutableSolidColorBrush currBrush && currBrush.Color == colour) {
                        return;
                    }

                    this.CurrentBrush = new ImmutableSolidColorBrush(colour);
                    if (notifyHandlers)
                        this.NotifyHandlersBrushChanged();
                }
            }
        }

        if (this.CurrentBrush != null) {
            this.CurrentBrush = null;
            if (notifyHandlers)
                this.NotifyHandlersBrushChanged();
        }
    }

    private void NotifyHandlersBrushChanged() {
        if (this.handlers != null) {
            foreach (Action<IBrush?> action in this.handlers) {
                action(this.CurrentBrush);
            }
        }
        
        this.BrushChanged?.Invoke(this);
    }

    private class UsageToken : IDisposable {
        private DynamicResourceAvaloniaColourBrush? brush;
        public readonly Action<IBrush?>? invalidatedHandler;

        public UsageToken(DynamicResourceAvaloniaColourBrush brush, Action<IBrush?>? invalidatedHandler) {
            this.brush = brush;
            this.invalidatedHandler = invalidatedHandler;
        }

        public void Dispose() {
            if (this.brush == null)
                throw new ObjectDisposedException("this", "Already disposed");

            this.brush.Unsubscribe(this);
            this.brush = null;
        }
    }
}