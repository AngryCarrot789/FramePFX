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

using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Shortcuts;

/// <summary>
/// A collection of shortcuts
/// </summary>
public sealed class ShortcutGroupEntry : IKeyMapEntry {
    public const char SeparatorChar = '/';

    private readonly ObservableList<ShortcutGroupEntry> groups;
    private readonly ObservableList<ShortcutEntry> shortcuts;
    private readonly ObservableList<InputStateEntry> inputStates;
    private readonly Dictionary<string, object> mapToItem;
    private InputStateManager? localStateManager;

    public ShortcutManager Manager { get; }

    public ShortcutGroupEntry? Parent { get; }

    public string? FullPath { get; }

    public string? Name { get; }

    /// <summary>
    /// This group's display name, which is a more readable and user-friendly version of <see cref="Name"/>
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// A description of what this group contains
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this focus group runs globally (across the entire application). Global focus groups contain shortcuts that can be run, irregardless of what group is focused
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// Inherits shortcuts from the parent group
    /// </summary>
    public bool Inherit { get; set; }

    /// <summary>
    /// All shortcuts in this focus group
    /// </summary>
    public ReadOnlyObservableList<ShortcutEntry> Shortcuts { get; }

    /// <summary>
    /// All input states in this focus group
    /// </summary>
    public ReadOnlyObservableList<InputStateEntry> InputStates { get; }

    /// <summary>
    /// All child-groups in this focus group
    /// </summary>
    public ReadOnlyObservableList<ShortcutGroupEntry> Groups { get; }

    public ShortcutGroupEntry(ShortcutManager manager, ShortcutGroupEntry? parent, string name, bool isGlobal = false, bool inherit = false) {
        if (name != null && string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be null or a non-empty string that does not consist of only whitespaces");
        this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        this.Name = name;
        this.FullPath = parent != null && name != null ? parent.GetPathForName(name) : name;
        this.Inherit = inherit;
        this.IsGlobal = isGlobal;
        this.Parent = parent;
        this.Groups = new ReadOnlyObservableList<ShortcutGroupEntry>(this.groups = new ObservableList<ShortcutGroupEntry>());
        this.Shortcuts = new ReadOnlyObservableList<ShortcutEntry>(this.shortcuts = new ObservableList<ShortcutEntry>());
        this.InputStates = new ReadOnlyObservableList<InputStateEntry>(this.inputStates = new ObservableList<InputStateEntry>());
        this.mapToItem = new Dictionary<string, object>();
    }

    public static ShortcutGroupEntry CreateRoot(ShortcutManager manager, bool isGlobal = true, bool inherit = false) {
        return new ShortcutGroupEntry(manager, null, null, isGlobal, inherit);
    }

    public string GetPathForName(string name) {
        ValidateName(name);
        return this.FullPath != null ? StringUtils.Join(this.FullPath, name, SeparatorChar) : name;
    }

    public ShortcutGroupEntry CreateGroupByName(string name, bool isGlobal = false, bool inherit = false) {
        ValidateName(name, "Group name cannot be null or consist of only whitespaces");
        this.ValidateNameNotInUse(name);
        ShortcutGroupEntry groupEntry = new ShortcutGroupEntry(this.Manager, this, name, isGlobal, inherit);
        this.mapToItem[name] = groupEntry;
        this.groups.Add(groupEntry);
        return groupEntry;
    }

    public void AddGroup(ShortcutGroupEntry groupEntry) {
        ValidateName(groupEntry.Name, "Group name cannot be null or consist of only whitespaces");
        this.ValidateNameNotInUse(groupEntry.Name);
        this.mapToItem[groupEntry.Name!] = groupEntry;
        this.groups.Add(groupEntry);
    }

    public ShortcutEntry AddShortcut(string name, IShortcut shortcut, bool isGlobal = false) {
        ValidateName(name, "Shortcut name cannot be null or consist of only whitespaces");
        this.ValidateNameNotInUse(name);
        ShortcutEntry managed = new ShortcutEntry(this, name, shortcut, isGlobal);
        this.mapToItem[name] = managed;
        this.shortcuts.Add(managed);
        return managed;
    }

    public InputStateEntry AddInputState(string name, IInputStroke activation, IInputStroke deactivation) {
        ValidateName(name, "Shortcut name cannot be null or consist of only whitespaces");
        this.ValidateNameNotInUse(name);
        InputStateEntry managed = new InputStateEntry(this, name, activation, deactivation);
        this.mapToItem[name] = managed;
        this.inputStates.Add(managed);
        return managed;
    }

    public void RemoveGroup(ShortcutGroupEntry entry) {
        if (entry.Name == null || !this.mapToItem.TryGetValue(entry.Name, out object? value) || entry.Parent != this || !ReferenceEquals(value, entry)) {
            throw new InvalidOperationException("Invalid entry");
        }

        this.mapToItem.Remove(entry.Name);
        this.groups.Remove(entry);
    }

    public void RemoveShortcut(ShortcutEntry entry) {
        if (!this.mapToItem.TryGetValue(entry.Name, out object? value) || entry.Parent != this || !ReferenceEquals(value, entry)) {
            throw new InvalidOperationException("Invalid entry");
        }

        this.mapToItem.Remove(entry.Name);
        this.shortcuts.Remove(entry);
    }

    public void RemoveInputState(InputStateEntry entry) {
        if (!this.mapToItem.TryGetValue(entry.Name, out object? value) || entry.Parent != this || !ReferenceEquals(value, entry)) {
            throw new InvalidOperationException("Invalid entry");
        }

        this.mapToItem.Remove(entry.Name);
        this.inputStates.Remove(entry);
    }

    public bool ContainsShortcutByName(string name) {
        return this.mapToItem.TryGetValue(name, out object? value) && value is ShortcutGroupEntry;
    }

    public bool ContainsGroupByName(string name) {
        return this.mapToItem.TryGetValue(name, out object? value) && value is ShortcutGroupEntry;
    }

    /// <summary>
    /// Old name: CollectShortcutsWithPrimaryStroke
    /// </summary>
    public void EvaulateShortcutsAndInputStates(ref ShortcutEvalArgs args, string? focus, bool allowDuplicateInheritedShortcuts = false) {
        this.CollectShortcutsInternal(ref args, string.IsNullOrWhiteSpace(focus) ? null : focus, allowDuplicateInheritedShortcuts);
    }

    private static bool FindPrimaryStroke(List<ShortcutEntry> list, IInputStroke stroke) {
        for (int i = 0, c = list.Count; i < c; i++) {
            if (list[i].Shortcut.IsPrimaryStroke(stroke)) {
                return true;
            }
        }

        return false;
    }

    private void CollectShortcutsInternal(ref ShortcutEvalArgs args, string? focus, bool allowDuplicateInheritedShortcuts = false) {
        // Searching groups first is what allows inheritance to work properly, because you search the deepest
        // levels first and make your way to the root. Similar to how bubble events work
        foreach (ShortcutGroupEntry group in this.groups) {
            group.CollectShortcutsInternal(ref args, focus);
        }

        bool requireGlobal = !this.IsGlobal && !IsFocusPathInScope(this.FullPath, focus, args.canInherit && this.Inherit);
        foreach (ShortcutEntry shortcut in this.shortcuts) {
            if (args.filter != null && !args.filter(shortcut)) {
                continue;
            }

            if (requireGlobal && !shortcut.IsGlobal) {
                // I actually can't remember if this.FullPath should be used here or shortcut.FullPath
                if ((shortcut.IsInherited && args.canInherit) && IsFocusPathInScope(this.FullPath, focus, true)) {
                    if (!allowDuplicateInheritedShortcuts && FindPrimaryStroke(args.shortcuts, shortcut.Shortcut.PrimaryStroke)) {
                        continue;
                    }
                }
                else {
                    continue;
                }
            }

            if (!shortcut.Shortcut.IsEmpty && shortcut.Shortcut.IsPrimaryStroke(args.stroke)) {
                args.shortcuts.Add(shortcut);
            }
        }

        if (args.canProcessInputStates) {
            foreach (InputStateEntry state in this.inputStates) {
                if (state.ActivationStroke.Equals(args.stroke)) {
                    // IsUsingToggleBehaviour ? !state.IsActive : true
                    args.inputStates.Add((state, !state.IsUsingToggleBehaviour || !state.IsActive));
                }
                else if (state.DeactivationStroke.Equals(args.stroke)) {
                    args.inputStates.Add((state, false));
                }
            }
        }
    }

    private static bool IsFocusPathInScope(string? path, string? focused, bool inherit) {
        return path != null && focused != null && (inherit ? focused.StartsWith(path) : focused.Equals(path));
    }

    public ShortcutEntry? FindFirstShortcutByCommandId(string cmdId) {
        foreach (ShortcutEntry shortcut in this.shortcuts) {
            if (cmdId.Equals(shortcut.CommandId)) {
                return shortcut;
            }
        }

        foreach (ShortcutGroupEntry group in this.Groups) {
            ShortcutEntry? result = group.FindFirstShortcutByCommandId(cmdId);
            if (result != null) {
                return result;
            }
        }

        return null;
    }

    public ShortcutGroupEntry? GetGroupByName(string name) {
        ValidateName(name);
        return this.mapToItem.TryGetValue(name, out object? value) ? value as ShortcutGroupEntry : null;
    }

    public ShortcutGroupEntry? GetGroupByPath(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return null;
        }

        int split = path.LastIndexOf(SeparatorChar);
        return split == -1 ? this.GetGroupByName(path) : this.GetGroupByPath(path.Split(SeparatorChar));
    }

    public ShortcutGroupEntry? GetGroupByPath(string[] path) {
        return this.GetGroupByPath(path, 0, path.Length);
    }

    public ShortcutGroupEntry? GetGroupByPath(string[]? path, int startIndex, int endIndex) {
        if (path == null || (endIndex - startIndex) == 0) {
            return null;
        }

        ValidatePathBounds(path, startIndex, endIndex);
        ShortcutGroupEntry? root = this;
        for (int i = startIndex; i < endIndex; i++) {
            if ((root = root.GetGroupByName(path[i])) == null) {
                return null;
            }
        }

        return root;
    }

    public ShortcutEntry GetShortcutByName(string name) {
        ValidateName(name);
        return this.mapToItem.TryGetValue(name, out object? value) ? value as ShortcutEntry : null;
    }

    public ShortcutEntry GetShortcutByPath(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return null;
        }

        int split = path.LastIndexOf(SeparatorChar);
        return split == -1 ? this.GetShortcutByName(path) : this.GetShortcutByPath(path.Split(SeparatorChar));
    }

    public ShortcutEntry GetShortcutByPath(string[] path) {
        return this.GetShortcutByPath(path, 0, path.Length);
    }

    public ShortcutEntry GetShortcutByPath(string[] path, int startIndex, int endIndex) {
        if (path == null || (endIndex - startIndex) == 0) {
            return null;
        }

        ValidatePathBounds(path, startIndex, endIndex);
        ShortcutGroupEntry root = this;
        int groupEndIndex = endIndex - 1;
        for (int i = startIndex; i < groupEndIndex; i++) {
            if ((root = root.GetGroupByName(path[i])) == null) {
                return null;
            }
        }

        return root.GetShortcutByName(path[groupEndIndex]);
    }

    private static void ValidatePathBounds(string[] path, int startIndex, int endIndex) {
        if (startIndex >= path.Length) {
            throw new IndexOutOfRangeException($"startIndex cannot be bigger than or equal to the path length ({startIndex} >= {path.Length})");
        }
        else if (startIndex > endIndex) {
            throw new IndexOutOfRangeException($"startIndex cannot be bigger than endIndex ({startIndex} > {endIndex})");
        }
    }

    private static void ValidateName(string? name, string message = "Name cannot be null or consist of only whitespaces") {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(message, name);
        }
    }

    private void ValidateNameNotInUse(string? name) {
        Validate.NotNull(name);
        if (this.mapToItem.ContainsKey(name)) {
            string path = this.FullPath != null ? StringUtils.Join(this.FullPath, name, SeparatorChar) : name;
            throw new Exception($"Group or shortcut already exists with name: '{path}'");
        }
    }

    public override string ToString() {
        return $"{nameof(ShortcutGroupEntry)} ({this.FullPath ?? "<root>"}{(!string.IsNullOrWhiteSpace(this.DisplayName) ? $" \"{this.DisplayName}\"" : "")})";
    }

    public InputStateManager GetInputStateManager() {
        return this.localStateManager ?? (this.localStateManager = this.Manager.GetInputStateManager(this.FullPath));
    }
}