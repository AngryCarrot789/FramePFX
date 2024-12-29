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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Themes.BrushFactories;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Themes;
using FramePFX.Themes.Configurations;
using SkiaSharp;
using IThemeConfigurationTreeElement = FramePFX.Configurations.UI.IThemeConfigurationTreeElement;

namespace FramePFX.BaseFrontEnd.Configurations.Pages.Themes;

public class ThemeConfigurationPageControl : BaseConfigurationPageControl {
    public new ThemeConfigurationPage? Page => (ThemeConfigurationPage?) base.Page;
    
    private ThemeConfigTreeView? PART_ThemeConfigTree;
    private TextBox? PART_ThemeKeyTextBox;
    private Grid? PART_SelectedItemGrid;
    private ColorPicker? PART_ColorPicker;
    private BindingExpressionBase? colourBinding;

    private bool ignoreSpectrumColourChange;
    private string? activeThemeKey;
    private DynamicAvaloniaColourBrush? myActiveBrush;
    private IDisposable? disposeMyActiveBrush;

    public ThemeConfigurationPageControl() {
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ThemeConfigTree = e.NameScope.GetTemplateChild<ThemeConfigTreeView>("PART_ThemeConfigTree");
        this.PART_ThemeKeyTextBox = e.NameScope.GetTemplateChild<TextBox>("PART_ThemeKeyTextBox");
        this.PART_SelectedItemGrid = e.NameScope.GetTemplateChild<Grid>("PART_SelectedItemGrid");
        this.PART_ColorPicker = e.NameScope.GetTemplateChild<ColorPicker>("PART_ColorPicker");
        DataManager.GetContextData(this).Set(IThemeConfigurationTreeElement.TreeElementKey, this.PART_ThemeConfigTree);
        
        this.PART_ThemeConfigTree.SelectionChanged += this.OnSelectionChanged;
        this.PART_ColorPicker.ColorChanged += this.OnColourChanged;
    }

    private void OnColourChanged(object? sender, ColorChangedEventArgs e) {
        if (this.ignoreSpectrumColourChange) {
            return;
        }
        
        if (this.activeThemeKey != null && this.Page!.TargetTheme is Theme theme) {
            this.Page!.OnChangingThemeColour(this.activeThemeKey);
            
            Color c = e.NewColor;
            theme.SetThemeColour(this.activeThemeKey, new SKColor(c.R, c.G, c.B, c.A));
        }
    }
    
    private void OnColourChangedInTheme(IBrush? obj) {
        this.ignoreSpectrumColourChange = true;
        this.PART_ColorPicker!.Color = obj is ISolidColorBrush bruhsh ? bruhsh.Color : default;
        this.ignoreSpectrumColourChange = false;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.PART_ThemeConfigTree!.SelectedItem is ThemeConfigTreeViewItem item) {
            if (item.Entry is ThemeConfigEntry configEntry) {
                this.PART_ThemeKeyTextBox!.Text = configEntry.ThemeKey;
                this.PART_ThemeKeyTextBox.IsEnabled = true;
                this.PART_ColorPicker!.IsEnabled = true;
                this.SetActiveThemeKey(configEntry.ThemeKey);
            }
            else {
                this.PART_ThemeKeyTextBox!.Text = "-";
                this.PART_ThemeKeyTextBox.IsEnabled = false;
                this.PART_ColorPicker!.IsEnabled = false;
                this.SetActiveThemeKey(null);
            }
            
            this.PART_SelectedItemGrid!.IsEnabled = true;
        }
        else {
            this.PART_SelectedItemGrid!.IsEnabled = false;
            this.PART_ColorPicker!.IsEnabled = false;
            this.SetActiveThemeKey(null);
        }
        
        this.colourBinding?.Dispose();
        this.colourBinding = null;
    }

    private void SetActiveThemeKey(string? themeKey) {
        if (themeKey == this.activeThemeKey) {
            return;
        }

        if (this.disposeMyActiveBrush != null) {
            this.disposeMyActiveBrush.Dispose();
            this.disposeMyActiveBrush = null;
            this.myActiveBrush = null;
            this.activeThemeKey = null;
        }

        if (themeKey != null) {
            this.activeThemeKey = themeKey;
            this.myActiveBrush = ((BrushManagerImpl) BrushManager.Instance).GetDynamicThemeBrush(themeKey);
            this.disposeMyActiveBrush = this.myActiveBrush.Subscribe(this.OnColourChangedInTheme);
        }
    }

    public override void OnConnected() {
        base.OnConnected();
        this.PART_ThemeConfigTree!.ThemeConfigurationPage = this.Page!;
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.PART_ThemeConfigTree!.ThemeConfigurationPage = null;
    }
}