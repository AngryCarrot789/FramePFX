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
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.BaseFrontEnd.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Shortcuts.Trees.InputStrokeControls;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.CommandSystem;
using FramePFX.Configurations.Shortcuts;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.BaseFrontEnd.Shortcuts.Trees;

public class ShortcutTreeViewItem : TreeViewItem, IShortcutTreeOrNode {
    public ShortcutTreeView? ResourceTree { get; private set; }

    public ShortcutTreeViewItem? ParentNode { get; private set; }

    public IKeyMapEntry? Entry { get; private set; }

    public int GroupCounter { get; private set; }

    public int InputStateCounter { get; private set; }

    private ShortcutEntryHeaderControl? PART_HeaderControl;
    private StackPanel? PART_InputStrokeList;

    public ShortcutTreeViewItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_HeaderControl = e.NameScope.GetTemplateChild<ShortcutEntryHeaderControl>(nameof(this.PART_HeaderControl));
        this.PART_InputStrokeList = e.NameScope.GetTemplateChild<StackPanel>(nameof(this.PART_InputStrokeList));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        AdvancedContextMenu.SetContextRegistry(this, ShortcutContextRegistry.Registry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    #region Model Connection

    public virtual void OnAdding(ShortcutTreeView tree, ShortcutTreeViewItem? parentNode, IKeyMapEntry resource) {
        this.ResourceTree = tree;
        this.ParentNode = parentNode;
        this.Entry = resource;
        if (resource is ShortcutEntry entry)
            DataManager.GetContextData(this).Set(DataKeys.ShortcutEntryKey, entry);
    }

    public virtual void OnAdded() {
        if (this.Entry is ShortcutGroupEntry myGroup) {
            int i = 0;
            foreach (ShortcutGroupEntry entry in myGroup.Groups) {
                this.InsertGroup(entry, i++);
            }

            i = 0;
            foreach (InputStateEntry entry in myGroup.InputStates) {
                this.InsertInputState(entry, i++);
            }

            i = 0;
            foreach (ShortcutEntry entry in myGroup.Shortcuts) {
                this.InsertShortcut(entry, i++);
            }
        }
        else if (this.Entry is ShortcutEntry shortcut) {
            shortcut.ShortcutChanged += this.OnEntryShortcutChanged;
            this.OnEntryShortcutChanged(shortcut, null!, shortcut.Shortcut);
        }

        // rawName should not be <root> because the root object should never be visible technically.
        // But just in case...
        this.PART_HeaderControl!.KeyMapEntry = this.Entry!;
        this.SetupToolTip(this.Entry!);
    }

    private void SetupToolTip(IKeyMapEntry entry) {
        TextBlock textBlock = new TextBlock() {
            Inlines = new InlineCollection(),
            Padding = new Thickness(5)
        };

        bool insertSpacing = false;

        if (entry is ShortcutEntry shortcut && !string.IsNullOrWhiteSpace(shortcut.CommandId)) {
            textBlock.Inlines.Add(new Run("Target Command ID") { FontSize = 16, FontWeight = FontWeight.Bold, BaselineAlignment = BaselineAlignment.Center});
            textBlock.Inlines.Add(new LineBreak());

            if (CommandManager.Instance.GetCommandById(shortcut.CommandId) != null) {
                textBlock.Inlines.Add(new Run(shortcut.CommandId) { FontWeight = FontWeight.Normal });
            }
            else {
                textBlock.Inlines.Add(new Run(shortcut.CommandId) { TextDecorations = TextDecorations.Strikethrough, FontStyle = FontStyle.Italic, FontWeight = FontWeight.Normal });
                textBlock.Inlines.Add(new Run(" (does not exist)"));
            }
            
            insertSpacing = true;
        }
        
        if (!string.IsNullOrWhiteSpace(entry.Description)) {
            if (insertSpacing) {
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new LineBreak());
            }
            
            textBlock.Inlines.Add(new Run(entry.Description) { BaselineAlignment = BaselineAlignment.Center});
        }

        ToolTip.SetTip(this, textBlock);
    }

    private void OnEntryShortcutChanged(ShortcutEntry sender, IShortcut oldShortcut, IShortcut newShortcut) {
        this.PART_InputStrokeList!.Children.Clear();
        foreach (IInputStroke stroke in newShortcut.InputStrokes) {
            if (stroke is KeyStroke keyStroke) {
                this.PART_InputStrokeList.Children.Add(new KeyStrokeControl() { KeyStroke = keyStroke });
            }
            else if (stroke is MouseStroke mouseStroke) {
                this.PART_InputStrokeList.Children.Add(new MouseStrokeControl() { MouseStroke = mouseStroke });
            }
        }
    }

    public virtual void OnRemoving() {
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNodeInternal(i);
        }

        if (this.Entry is ShortcutEntry shortcut) {
            shortcut.ShortcutChanged -= this.OnEntryShortcutChanged;
            this.PART_InputStrokeList!.Children.Clear();
        }

        this.PART_HeaderControl!.KeyMapEntry = null;
    }

    public virtual void OnRemoved() {
        this.ResourceTree = null;
        this.ParentNode = null;
        this.Entry = null;
        DataManager.ClearContextData(this);
    }

    #endregion

    #region Model to Control objects

    public ShortcutTreeViewItem GetNodeAt(int index) => (ShortcutTreeViewItem) this.Items[index]!;

    public void InsertGroup(ShortcutGroupEntry entry, int index) {
        this.GroupCounter++;
        this.InsertNodeInternal(entry, index);
    }

    public void InsertInputState(InputStateEntry entry, int index) {
        this.InputStateCounter++;
        this.InsertNodeInternal(entry, index + this.GroupCounter);
    }

    public void InsertShortcut(ShortcutEntry entry, int index) {
        this.InsertNodeInternal(entry, index + this.GroupCounter + this.InputStateCounter);
    }

    public void RemoveGroup(int index, bool canCache = true) {
        this.GroupCounter--;
        this.RemoveNodeInternal(index, canCache);
    }

    public void RemoveInputState(int index, bool canCache = true) {
        this.InputStateCounter--;
        this.RemoveNodeInternal(index + this.GroupCounter, canCache);
    }

    public void RemoveShortcut(int index, bool canCache = true) {
        this.RemoveNodeInternal(index + this.GroupCounter + this.InputStateCounter, canCache);
    }

    public void InsertNodeInternal(IKeyMapEntry layer, int index) {
        ShortcutTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");

        ShortcutTreeViewItem control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, layer);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNodeInternal(int index, bool canCache = true) {
        ShortcutTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ShortcutTreeViewItem control = (ShortcutTreeViewItem) this.Items[index]!;
        IKeyMapEntry resource = control.Entry ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    #endregion

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
        if ((e.ClickCount % 2) == 0) {
            if (!isToggle) {
                this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                e.Handled = true;
            }
        }
        else if ((this.IsFocused || this.Focus())) {
            e.Pointer.Capture(this);
            this.ResourceTree?.SetSelection(this);
            e.Handled = true;
        }
    }
}