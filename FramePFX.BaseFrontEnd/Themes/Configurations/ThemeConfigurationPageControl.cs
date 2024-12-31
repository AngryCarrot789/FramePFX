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
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.BaseFrontEnd.Configurations.Pages;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Themes.BrushFactories;
using FramePFX.BaseFrontEnd.Themes.Controls;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Themes;
using FramePFX.Themes.Configurations;
using SkiaSharp;
using IThemeConfigurationTreeElement = FramePFX.Configurations.UI.IThemeConfigurationTreeElement;

namespace FramePFX.BaseFrontEnd.Themes.Configurations;

public class ThemeConfigurationPageControl : BaseConfigurationPageControl {
    public new ThemeConfigurationPage? Page => (ThemeConfigurationPage?) base.Page;
    
    private ThemeConfigTreeView? PART_ThemeConfigTree;
    private TextBox? PART_ThemeKeyTextBox;
    private Grid? PART_SelectedItemGrid;
    private ColorPicker? PART_ColorPicker;
    private GroupBox? PART_GroupBox;
    private BindingExpressionBase? colourBinding;
    private TextBlock? PART_WarnEditingBuiltInTheme;
    private Button? PART_ResetButton;

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
        this.PART_WarnEditingBuiltInTheme = e.NameScope.GetTemplateChild<TextBlock>("PART_WarnEditingBuiltInTheme");
        this.PART_SelectedItemGrid = e.NameScope.GetTemplateChild<Grid>("PART_SelectedItemGrid");
        this.PART_ColorPicker = e.NameScope.GetTemplateChild<ColorPicker>("PART_ColorPicker");
        this.PART_GroupBox = e.NameScope.GetTemplateChild<GroupBox>("PART_GroupBox");
        this.PART_ResetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetButton");
        DataManager.GetContextData(this).Set(IThemeConfigurationTreeElement.TreeElementKey, this.PART_ThemeConfigTree);
        
        this.PART_ThemeConfigTree.SelectionChanged += this.OnSelectionChanged;
        this.PART_ColorPicker.ColorChanged += this.OnColourChanged;
        this.PART_ResetButton.Click += this.ResetValueClick;
    }

    private void UpdateCanResetValue() {
        ThemeConfigurationPage? page = this.Page;
        if (page != null && page.TargetTheme != null && this.activeThemeKey != null) {
            this.PART_ResetButton!.IsEnabled = page.HasThemeKeyChanged(page.TargetTheme, this.activeThemeKey);
        }
        else {
            this.PART_ResetButton!.IsEnabled = false;
        }
    }
    
    private void ResetValueClick(object? sender, RoutedEventArgs e) {
        ThemeConfigurationPage? page = this.Page;
        if (page != null && page.TargetTheme != null && this.activeThemeKey != null) {
            page.ReverseChangeFor(page.TargetTheme, this.activeThemeKey);
        }
    }
    
    private void OnTargetThemeChanged(ThemeConfigurationPage sender, Theme? oldTheme, Theme? newTheme) {
        this.UpdateGroupBoxAndWarningMessage();
        this.UpdateCanResetValue();
    }
    
    private void OnThemeEntryModified(ThemeConfigurationPage sender, string key, bool isAdded) {
        // we only need to update if the changed key is the one we're viewing.
        // It should be the one we're viewing anyway
        if (key == this.activeThemeKey)
            this.UpdateCanResetValue();
    }
    
    private void OnThemeModifiedThemeEntriesCleared(ThemeConfigurationPage sender, Dictionary<string, ISavedThemeEntry> oldItems) {
        this.UpdateCanResetValue();
    }
    
    private void UpdateGroupBoxAndWarningMessage() {
        Theme? theme = this.Page?.TargetTheme;
        if (theme != null) {
            this.PART_WarnEditingBuiltInTheme!.IsVisible = theme.IsBuiltIn;
            this.PART_GroupBox!.Header = $"Current theme: {theme.Name}";
            this.IsEnabled = true;
            if (this.activeThemeKey != null && ((ThemeManagerImpl.ThemeImpl) theme).TryFindBrushInHierarchy(this.activeThemeKey, out IBrush? brush)) {
                this.OnColourChangedInTheme(brush);
            }
        }
        else {
            this.PART_WarnEditingBuiltInTheme!.IsVisible = false;
            this.PART_GroupBox!.Header = "<No Theme Selected>";
            this.IsEnabled = false;
            this.OnColourChangedInTheme(null);
        }
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
        if (obj is ISolidColorBrush bruh) {
            this.PART_ColorPicker!.IsEnabled = true;
            
            this.ignoreSpectrumColourChange = true;
            this.PART_ColorPicker!.Color = bruh.Color;
            this.ignoreSpectrumColourChange = false;
        }
        else {
            this.PART_ColorPicker!.IsEnabled = false;
        }
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
            this.UpdateCanResetValue();
        }
        else {
            this.PART_ResetButton!.IsEnabled = false;
        }
    }

    public override void OnConnected() {
        base.OnConnected();
        this.PART_ThemeConfigTree!.ThemeConfigurationPage = this.Page!;
        this.Page!.TargetThemeChanged += this.OnTargetThemeChanged;
        this.Page!.ThemeEntryModified += this.OnThemeEntryModified;
        this.Page!.ModifiedThemeEntriesCleared += this.OnThemeModifiedThemeEntriesCleared;
        this.UpdateGroupBoxAndWarningMessage();
        this.UpdateCanResetValue();
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.PART_ThemeConfigTree!.ThemeConfigurationPage = null;
        this.Page!.TargetThemeChanged -= this.OnTargetThemeChanged;
        this.Page!.ThemeEntryModified -= this.OnThemeEntryModified;
        this.Page!.ModifiedThemeEntriesCleared -= this.OnThemeModifiedThemeEntriesCleared;
    }
}
