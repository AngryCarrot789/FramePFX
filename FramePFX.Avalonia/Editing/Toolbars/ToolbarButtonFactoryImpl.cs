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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FramePFX.BaseFrontEnd.AvControls;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.Icons;
using FramePFX.Interactivity.Contexts;
using FramePFX.Toolbars;
using FramePFX.Utils.Commands;

namespace FramePFX.Avalonia.Editing.Toolbars;

public class ToolbarButtonFactoryImpl : ToolbarButtonFactory {
    public override IButtonElement CreateButton() => new ButtonElementImpl();

    public override IToggleButtonElement CreateToggleButton() => new ToggleButtonElementImpl();
}

public abstract class AbstractAvaloniaButtonElement : IButtonElement {
    protected readonly Button button;
    protected bool isAttachedToVisualTree;
    protected Icon? myIcon;

    public IContextData ContextData => this.isAttachedToVisualTree ? DataManager.GetFullContextData(this.button) : EmptyContext.Instance;

    public AsyncRelayCommand? Command {
        get => this.button.Command as AsyncRelayCommand;
        set => this.button.Command = value;
    }

    public string? ToolTip {
        get => (string?) global::Avalonia.Controls.ToolTip.GetTip(this.button);
        set => global::Avalonia.Controls.ToolTip.SetTip(this.button, value);
    }

    public bool IsEnabled {
        get => this.button.IsEnabled;
        set => this.button.IsEnabled = value;
    }

    public bool IsVisible {
        get => this.button.IsVisible;
        set => this.button.IsVisible = value;
    }
    
    public string? Text { get; set; }

    public Icon? Icon {
        get => this.myIcon;
        set {
            if (!ReferenceEquals(this.myIcon, value)) {
                this.myIcon = value;
                if (this.isAttachedToVisualTree && this.button is IIconButton btn) {
                    btn.Icon = value;
                }
            }
        }
    }
    
    public Button Button => this.button;

    public event ButtonContextInvalidatedEventHandler? ContextInvalidated;

    public AbstractAvaloniaButtonElement(Button button) {
        this.button = button;
        this.button.AttachedToVisualTree += this.OnAttachedToVisualTree;
        this.button.DetachedFromVisualTree += this.OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        this.isAttachedToVisualTree = true;
        DataManager.AddInheritedContextChangedHandler(this.button, this.OnInheritedContextChangedImmediately);
        
        this.ContextInvalidated?.Invoke(this);

        if (this.myIcon != null && this.button is IIconButton btn) {
            btn.Icon = this.myIcon;
        }
    }
    
    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        this.isAttachedToVisualTree = false;
        DataManager.RemoveInheritedContextChangedHandler(this.button, this.OnInheritedContextChangedImmediately);
        
        this.ContextInvalidated?.Invoke(this);
        
        // Because icons may use dynamic resources allocated by the icon manager,
        // we need to tell the icon control to dereference it to prevent
        // unnecessary resource change handlers

        if (this.button is IIconButton btn) {
            btn.Icon = null;
        }
    }
    
    private void OnInheritedContextChangedImmediately(object sender, RoutedEventArgs e) {
        this.ContextInvalidated?.Invoke(this);
    }
}

public class ButtonElementImpl : AbstractAvaloniaButtonElement {
    public new IconButton Button => (IconButton) this.button;
    
    public ButtonElementImpl() : base(new IconButton()) {
    }
}

public class ToggleButtonElementImpl : AbstractAvaloniaButtonElement, IToggleButtonElement {
    public bool? IsChecked {
        get => this.Button.IsChecked;
        set => this.Button.IsChecked = value;
    }

    public bool IsThreeState {
        get => this.Button.IsThreeState;
        set => this.Button.IsThreeState = value;
    }
    
    public new IconToggleButton Button => (IconToggleButton) this.button;
    
    public ToggleButtonElementImpl() : base(new IconToggleButton()) {
    }
}