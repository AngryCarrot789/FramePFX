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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.Editing.ResourceManaging.Autoloading;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Utils;

namespace FramePFX.BaseFrontEnd.ResourceManaging.Autoloading;

public partial class ResourceLoaderDialog : DesktopWindow {
    public static readonly StyledProperty<ResourceLoader?> ResourceLoaderProperty = AvaloniaProperty.Register<ResourceLoaderDialog, ResourceLoader?>(nameof(ResourceLoader));

    public ResourceLoader? ResourceLoader {
        get => this.GetValue(ResourceLoaderProperty);
        set => this.SetValue(ResourceLoaderProperty, value);
    }

    private readonly List<InvalidResourceEntryControl> controls;
    private bool isRemovingAllEntries;

    public ResourceLoaderDialog() {
        this.InitializeComponent();
        this.controls = new List<InvalidResourceEntryControl>();
        this.PART_ListBox.SelectionChanged += this.OnSelectedItemChanged;
    }

    static ResourceLoaderDialog() {
        ResourceLoaderProperty.Changed.AddClassHandler<ResourceLoaderDialog, ResourceLoader?>((d, e) => d.OnResourceLoaderChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnResourceLoaderChanged(ResourceLoader? oldLoader, ResourceLoader? newLoader) {
        if (oldLoader != null) {
            oldLoader.EntryAdded -= this.OnEntryAdded;
            oldLoader.EntryRemoved -= this.OnEntryRemoved;

            for (int i = this.PART_ListBox.Items.Count - 1; i >= 0; i--) {
                this.RemoveItemAt(i);
            }
        }

        if (newLoader != null) {
            newLoader.EntryAdded += this.OnEntryAdded;
            newLoader.EntryRemoved += this.OnEntryRemoved;

            int i = 0;
            foreach (InvalidResourceEntry entry in newLoader.Entries) {
                this.InsertItemAt(i++, entry);
            }

            if (i > 0) {
                this.PART_ListBox.SelectedIndex = 0;
            }
        }
    }

    private void OnEntryAdded(ResourceLoader loader, InvalidResourceEntry entry, int index) => this.InsertItemAt(index, entry);

    private void OnEntryRemoved(ResourceLoader loader, InvalidResourceEntry entry, int index) {
        this.RemoveItemAt(index);
        if (loader.Entries.Count < 1 && !this.isRemovingAllEntries) {
            this.Close(true);
        }
    }

    private void InsertItemAt(int index, InvalidResourceEntry entry) {
        InvalidResourceEntryControl control = InvalidResourceEntryControl.Registry.NewInstance(entry);
        this.controls.Insert(index, control);
        control.Attach(entry);

        // Too lazy to do anything clever so i'm just manually generating the content
        TextBlock textBlock = new TextBlock();
        textBlock.Inlines ??= new InlineCollection();
        textBlock.Inlines.Add(new Run(entry.Resource.GetType().Name) {
            FontSize = 16, TextDecorations = TextDecorations.Underline, FontWeight = FontWeight.SemiBold
        });

        if (!string.IsNullOrWhiteSpace(entry.DisplayName)) {
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new Run($"\"{entry.DisplayName}\"") {
                FontWeight = FontWeight.Medium, FontStyle = FontStyle.Italic
            });
        }

        this.PART_ListBox.Items.Insert(index, new ListBoxItem() { Content = textBlock });
        entry.DisplayNameChanged += this.OnEntryDisplayNameChanged;
    }

    private void RemoveItemAt(int index) {
        InvalidResourceEntry entry = this.controls[index].Entry!;
        entry.DisplayNameChanged -= this.OnEntryDisplayNameChanged;
        this.PART_ContentPresenter.Content = null;
        this.controls[index].Detach();
        this.controls.RemoveAt(index);
        this.PART_ListBox.Items.RemoveAt(index);
    }

    private void OnSelectedItemChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.PART_ListBox.Items.Count < 1) {
            return;
        }

        this.PART_ContentPresenter.Content = this.PART_ListBox.SelectedIndex == -1 ? null : this.controls[this.PART_ListBox.SelectedIndex];
    }

    private void OnEntryDisplayNameChanged(InvalidResourceEntry entry) {
        int index = this.ResourceLoader!.Entries.IndexOf(entry);
        if (index != -1) {
            ((ListBoxItem) this.PART_ListBox.Items[index]!).Content = entry.DisplayName;
        }
    }

    private void OfflineAll_Clicked(object? sender, RoutedEventArgs e) {
        ResourceLoader? loader = this.ResourceLoader;
        if (loader != null) {
            this.isRemovingAllEntries = true;
            for (int i = loader.Entries.Count - 1; i >= 0; i--) {
                loader.RemoveEntryAt(i);
            }
        }

        this.Close(true);
    }

    private void OfflineSelected_Click(object? sender, RoutedEventArgs e) {
        int index;
        if (this.PART_ListBox.Items.Count > 0 && (index = this.PART_ListBox.SelectedIndex) != -1) {
            this.ResourceLoader!.RemoveEntryAt(index);
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) {
        this.Close(false);
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        this.ResourceLoader = null;
    }
}