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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.Avalonia.Configurations.Pages;
using FramePFX.Avalonia.Configurations.Trees;
using FramePFX.Avalonia.Editing;
using FramePFX.Configurations;
using FramePFX.Services.Messaging;

namespace FramePFX.Avalonia.Configurations;

public delegate void ConfigurationPanelEditorActiveContextChangedEventHandler(ConfigurationPanelControl sender, ConfigurationContext? oldActiveContext, ConfigurationContext? newActiveContext);

public partial class ConfigurationPanelControl : UserControl
{
    public static readonly StyledProperty<ConfigurationManager?> ConfigurationManagerProperty = AvaloniaProperty.Register<ConfigurationPanelControl, ConfigurationManager?>(nameof(ConfigurationManager));

    public ConfigurationManager? ConfigurationManager
    {
        get => this.GetValue(ConfigurationManagerProperty);
        set => this.SetValue(ConfigurationManagerProperty, value);
    }

    private ConfigurationEntry? connectedEntry;
    private readonly ModelControlDictionary<ConfigurationManager, ConfigurationTreeViewItem> itemMap;
    private ConfigurationContext? activeContext;

    public ConfigurationContext? ActiveContext
    {
        get => this.activeContext;
        set
        {
            ConfigurationContext? oldActiveContext = this.activeContext; 
            if (oldActiveContext == value)
                return;
        
            this.activeContext = value;
            this.ActiveContextChanged?.Invoke(this, oldActiveContext, value);
        }
    }

    public event ConfigurationPanelEditorActiveContextChangedEventHandler? ActiveContextChanged;
    
    public ConfigurationPanelControl()
    {
        this.InitializeComponent();
        this.itemMap = new ModelControlDictionary<ConfigurationManager, ConfigurationTreeViewItem>();
        this.PART_ConfigurationTree.SelectionMode = SelectionMode.Single;
        this.PART_ConfigurationTree.SelectionChanged += this.OnTreeSelectionChanged;
    }

    static ConfigurationPanelControl()
    {
        ConfigurationManagerProperty.Changed.AddClassHandler<ConfigurationPanelControl, ConfigurationManager?>((d, e) => d.OnConfigurationManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (this.ActiveContext == null)
            return;
        
        this.UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (this.PART_ConfigurationTree.SelectedItem is ConfigurationTreeViewItem item && item.Entry != null)
        {
            if (this.connectedEntry == item.Entry)
            {
                return;
            }

            ConfigurationPage? page = item.Entry.Page;
            if (page == null)
            {
                ConfigurationEntry? firstChild = item.Entry.Items.FirstOrDefault(x => x.Page != null);
                if (firstChild != null)
                {
                    page = firstChild.Page;
                }
            }

            if (page != null)
            {
                this.DisconnectPage();
                BaseConfigurationPageControl control = ConfigurationPageRegistry.Registry.NewInstance(page, false);
                this.ConnectPage(page, control);
                this.connectedEntry = item.Entry;
                this.UpdateNavigationHeading();
            }
        }
        else
        {
            this.DisconnectPage();
            this.connectedEntry = null;
            this.UpdateNavigationHeading();
        }
    }

    private void UpdateNavigationHeading()
    {
        List<ConfigurationEntry> entries = new List<ConfigurationEntry>();
        for (ConfigurationEntry? entry = this.connectedEntry; entry != null && !entry.IsRoot; entry = entry.Parent)
        {
            entries.Add(entry);
        }

        if (entries.Count < 1)
        {
            this.PART_NavigationPathTextBlock.Inlines = null;
            return;
        }

        InlineCollection inlines = this.PART_NavigationPathTextBlock.Inlines ??= new InlineCollection();
        inlines.Clear();
        
        int i = entries.Count - 1;
        this.ApplyInline(inlines, entries[i--]);
        while (i >= 0)
        {
            ApplyInlineSeparator(inlines);
            this.ApplyInline(inlines, entries[i--]);
        }
    }

    private static void ApplyInlineSeparator(InlineCollection collection)
    {
        collection.Add(new Run(" / ") {BaselineAlignment = BaselineAlignment.Center});
    }

    private class HyperlinkTagInfo
    {
        private readonly WeakReference entry;
        private readonly WeakReference editor;

        public ConfigurationEntry? Entry => (ConfigurationEntry?) this.entry.Target;

        public ConfigurationPanelControl? Editor => (ConfigurationPanelControl?) this.editor.Target;

        public HyperlinkTagInfo(ConfigurationEntry entry, ConfigurationPanelControl editor)
        {
            this.entry = new WeakReference(entry);
            this.editor = new WeakReference(editor);
        }
    }

    private void ApplyInline(InlineCollection collection, ConfigurationEntry entry)
    {
        HyperlinkButton hyperlinkButton = new HyperlinkButton()
        {
            Content = entry.DisplayName ?? "Unnamed Configuration",
            ClickMode = ClickMode.Press,
            Tag = new HyperlinkTagInfo(entry, this)
        };

        ToolTip.SetTip(hyperlinkButton, "Navigate to " + (entry.DisplayName ?? entry.FullIdPath ?? "this unnamed entry"));
        hyperlinkButton.Click += OnHyperlinkClicked;
        collection.Add(hyperlinkButton);
    }

    private static void OnHyperlinkClicked(object? sender, RoutedEventArgs e)
    {
        if (((HyperlinkButton?) sender)?.Tag is HyperlinkTagInfo info)
        {
            if (info.Editor is ConfigurationPanelControl editor && info.Entry is ConfigurationEntry entry)
            {
                if (editor.PART_ConfigurationTree.ItemMap.TryGetControl(entry, out ConfigurationTreeViewItem? treeItem))
                {
                    treeItem.ResourceTree?.SetSelection(treeItem);
                }
            }
        }
    }

    private void DisconnectPage()
    {
        if (this.PART_PagePresenter.Content is BaseConfigurationPageControl page)
        {
            page.Disconnect();
        }

        this.PART_PagePresenter.Content = null;
        this.ActiveContext!.SetViewPage(null);
    }

    private void ConnectPage(ConfigurationPage configPage, BaseConfigurationPageControl page)
    {
        this.PART_PagePresenter.Content = page;
        page.ApplyStyling();
        page.ApplyTemplate();
        page.Connect(configPage);
        this.ActiveContext!.SetViewPage(configPage);
    }

    private async void OnConfigurationManagerChanged(ConfigurationManager? oldValue, ConfigurationManager? newValue)
    {
        this.SetLoadingState(true);
        await Application.Instance.Dispatcher.InvokeAsync(() => Task.CompletedTask, DispatchPriority.Loaded);

        try
        {
            if (oldValue != null)
            {
                Debug.Assert(this.ActiveContext != null, "Context could not be null if we have an old value");
                await oldValue.UnloadContextAsync(this.ActiveContext);
            }
        }
        catch (Exception ex)
        {
            await IMessageDialogService.Instance.ShowMessage("Error", "Error unloading settings properties. The editor will now crash", ex.ToString());
            Application.Instance.Dispatcher.Post(() => throw ex);
        }

        this.ActiveContext?.OnDestroyed();
        this.ActiveContext = null;
        this.PART_ConfigurationTree.RootConfigurationEntry = null;
        
        if (newValue != null)
        {
            try
            {
                this.ActiveContext = new ConfigurationContext();
                await newValue.LoadContextAsync(this.ActiveContext);
            }
            catch (Exception ex)
            {
                await IMessageDialogService.Instance.ShowMessage("Error", "Error loading settings properties. The editor will now crash", ex.ToString());
                Application.Instance.Dispatcher.Post(() => throw ex);
            }

            this.ActiveContext!.OnCreated();
            this.PART_ConfigurationTree.RootConfigurationEntry = newValue.RootEntry;
        }

        await Application.Instance.Dispatcher.InvokeAsync(() => Task.CompletedTask, DispatchPriority.Loaded);
        Application.Instance.Dispatcher.Post(() =>
        {
            this.SetLoadingState(false);
            this.UpdateSelection();
        }, DispatchPriority.Loaded);
    }

    private void SetLoadingState(bool isLoading)
    {
        if (isLoading)
        {
            this.PART_CoreContentGrid.Opacity = 0.5;
            this.PART_LoadingBorder.IsVisible = true;
        }
        else
        {
            this.PART_CoreContentGrid.Opacity = 1.0;
            this.PART_LoadingBorder.IsVisible = false;
        }
    }

    public void SelectFirst()
    {
        if (this.PART_ConfigurationTree.Items.Count > 0)
            this.PART_ConfigurationTree.SelectedItem = this.PART_ConfigurationTree.Items[0];
    }
}