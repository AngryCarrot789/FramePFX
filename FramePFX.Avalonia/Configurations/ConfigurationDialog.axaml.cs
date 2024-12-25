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

using System.Linq;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Configurations;
using FramePFX.Utils.Commands;

namespace FramePFX.Avalonia.Configurations;

public partial class ConfigurationDialog : WindowEx
{
    private readonly ConfigurationManager configManager;
    private readonly AsyncRelayCommand ApplyCommand;
    private readonly AsyncRelayCommand ApplyThenCloseCommand;

    public ConfigurationDialog(ConfigurationManager manager)
    {
        this.InitializeComponent();
        this.configManager = manager;
        
        this.PART_ApplyButton.Click += this.OnApplyButtonClicked;
        this.PART_ConfirmButton.Click += this.OnConfirmButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;

        this.PART_ApplyButton.IsEnabled = true;
        this.PART_ConfirmButton.IsEnabled = true;
        this.PART_CancelButton.IsEnabled = true;
        this.PART_EditorPanel.IsEnabled = true;
        this.ApplyCommand = new AsyncRelayCommand(async () =>
        {
            this.PART_ApplyButton.IsEnabled = false;
            this.PART_ConfirmButton.IsEnabled = false;
            this.PART_CancelButton.IsEnabled = false;
            this.PART_EditorPanel.IsEnabled = false;
            await this.configManager.ApplyHierarchyAsync();
            this.PART_ApplyButton.IsEnabled = true;
            this.PART_ConfirmButton.IsEnabled = true;
            this.PART_CancelButton.IsEnabled = true;
            this.PART_EditorPanel.IsEnabled = true;
        });
        this.ApplyThenCloseCommand = new AsyncRelayCommand(async () =>
        {
            this.PART_ApplyButton.IsEnabled = false;
            this.PART_ConfirmButton.IsEnabled = false;
            this.PART_CancelButton.IsEnabled = false;
            this.PART_EditorPanel.IsEnabled = false;
            await this.configManager.ApplyHierarchyAsync();
            this.Close(true);
        });
        
        this.PART_EditorPanel.ActiveContextChanged += this.OnEditorContextChanged;
        this.PART_EditorPanel.ConfigurationManager = manager;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Application.Instance.Dispatcher.InvokeAsync(() =>
        {
            this.PART_EditorPanel.SelectFirst();
        }, DispatchPriority.Loaded);
    }

    private void OnEditorContextChanged(ConfigurationPanelControl sender, ConfigurationContext? oldContext, ConfigurationContext? newContext)
    {
        if (oldContext != null)
            oldContext.ModifiedPagesUpdated -= this.OnModifiedPagesChanged;
        
        if (newContext != null)
            newContext.ModifiedPagesUpdated += this.OnModifiedPagesChanged;
    }

    private void OnModifiedPagesChanged(ConfigurationContext context)
    {
        this.PART_ConfirmButton.IsEnabled = context.ModifiedPages.Any();
    }

    private void OnApplyButtonClicked(object? sender, RoutedEventArgs e) => this.ApplyCommand.Execute(null);
    
    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) => this.TryCloseDialog(true);

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => this.TryCloseDialog(false);
    
    /// <summary>
    /// Tries to close the dialog
    /// </summary>
    /// <param name="result">The dialog result wanted</param>
    /// <returns>True if the dialog was closed, false if it could not be closed due to a validation error or other error</returns>
    public bool TryCloseDialog(bool result)
    {
        if (result)
        {
            // TODO: 'failure to apply' system; a list of FailedApplyConfigurationEntry which gets presented to the user?
            this.ApplyThenCloseCommand.Execute(null);
            return true;
        }
        else
        {
            this.Close(false);
            return true;
        }
    }
}