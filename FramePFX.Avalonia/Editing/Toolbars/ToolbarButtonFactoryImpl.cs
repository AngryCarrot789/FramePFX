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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

    public override IToggleButtonElement CreateToggleButton(ToggleButtonStyle style) {
        switch (style) {
            case ToggleButtonStyle.Button:   return new IconToggleButtonElementImpl();
            case ToggleButtonStyle.CheckBox: return new CheckBoxToggleButtonElementImpl();
            case ToggleButtonStyle.Switch:   return new SwitchToggleButtonElementImpl();
            default:                         throw new ArgumentOutOfRangeException(nameof(style), style, null);
        }
    }
}

public abstract class AbstractAvaloniaButtonElement : AbstractAvaloniaToolBarElement, IButtonElement {
    protected readonly Button myButton;
    protected bool isAttachedToVisualTree;
    protected Icon? myIcon;

    public IContextData ContextData => this.isAttachedToVisualTree ? DataManager.GetFullContextData(this.Button) : EmptyContext.Instance;

    public AsyncRelayCommand? Command {
        get => this.myButton.Command as AsyncRelayCommand;
        set => this.myButton.Command = value;
    }

    public string? ToolTip {
        get => (string?) global::Avalonia.Controls.ToolTip.GetTip(this.myButton);
        set => global::Avalonia.Controls.ToolTip.SetTip(this.myButton, value);
    }

    public bool IsEnabled {
        get => this.myButton.IsEnabled;
        set => this.myButton.IsEnabled = value;
    }

    public bool IsVisible {
        get => this.myButton.IsVisible;
        set => this.myButton.IsVisible = value;
    }
    
    public abstract string? Text { get; set; }

    public Icon? Icon {
        get => this.myIcon;
        set {
            if (!ReferenceEquals(this.myIcon, value)) {
                this.myIcon = value;
                if (this.isAttachedToVisualTree && this.myButton is IIconButton btn) {
                    btn.Icon = value;
                }
            }
        }
    }

    public Button Button => this.myButton;
    
    public override Control Control => this.myButton;
    
    public event ButtonContextInvalidatedEventHandler? ContextInvalidated;

    public AbstractAvaloniaButtonElement(Button Button) {
        this.myButton = Button;
        this.myButton.AttachedToVisualTree += this.OnAttachedToVisualTree;
        this.myButton.DetachedFromVisualTree += this.OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        this.isAttachedToVisualTree = true;
        DataManager.AddInheritedContextChangedHandler(this.Button, this.OnInheritedContextChangedImmediately);
        
        this.ContextInvalidated?.Invoke(this);

        if (this.myIcon != null && this.Button is IIconButton btn) {
            btn.Icon = this.myIcon;
        }
    }
    
    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        this.isAttachedToVisualTree = false;
        DataManager.RemoveInheritedContextChangedHandler(this.Button, this.OnInheritedContextChangedImmediately);
        
        this.ContextInvalidated?.Invoke(this);
        
        // Because icons may use dynamic resources allocated by the icon manager,
        // we need to tell the icon control to dereference it to prevent
        // unnecessary resource change handlers

        if (this.Button is IIconButton btn) {
            btn.Icon = null;
        }
    }
    
    private void OnInheritedContextChangedImmediately(object sender, RoutedEventArgs e) {
        this.ContextInvalidated?.Invoke(this);
    }
}

public class ButtonElementImpl : AbstractAvaloniaButtonElement {
    public new IconButton Button => (IconButton) base.Button;

    public override string? Text {
        get => (string?) this.Button.Content;
        set => this.Button.Content = value;
    }
    
    public ButtonElementImpl() : base(new IconButton()) {
    }
}

public abstract class AbstractAvaloniaToggleButtonElement : AbstractAvaloniaButtonElement, IToggleButtonElement {
    public new ToggleButton Button => (ToggleButton) base.Button;
    
    public bool? IsChecked {
        get => this.Button.IsChecked;
        set => this.Button.IsChecked = value;
    }

    public bool IsThreeState {
        get => this.Button.IsThreeState;
        set => this.Button.IsThreeState = value;
    }
    
    public AbstractAvaloniaToggleButtonElement(Button Button) : base(Button) {
    }
}

public class IconToggleButtonElementImpl : AbstractAvaloniaToggleButtonElement, IToggleButtonElement {
    public override string? Text {
        get => (string?) this.Button.Content;
        set => this.Button.Content = value;
    }
    
    public new IconToggleButton Button => (IconToggleButton) base.Button;
    
    public IconToggleButtonElementImpl() : base(new IconToggleButton()) {
    }
}

public class CheckBoxToggleButtonElementImpl : AbstractAvaloniaToggleButtonElement, IToggleButtonElement {
    public new CheckBox Button => (CheckBox) base.Button;
    
    public override string? Text {
        get => (string?) this.Button.Content;
        set => this.Button.Content = value;
    }
    
    public CheckBoxToggleButtonElementImpl() : base(new CheckBox()) {
    }
}

public class SwitchToggleButtonElementImpl : AbstractAvaloniaToggleButtonElement, IToggleButtonElement {
    public new ToggleSwitch Button => (ToggleSwitch) base.Button;
    
    public override string? Text {
        get => (string?) this.Button.Content;
        set => this.Button.Content = value;
    }
    
    public SwitchToggleButtonElementImpl() : base(new ToggleSwitch()) {
    }
}