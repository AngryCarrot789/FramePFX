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

using PFXToolKitUI.Configurations;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Themes.Configurations;

public delegate void ThemeConfigurationPageTargetThemeChangedEventHandler(ThemeConfigurationPage sender, Theme? oldTargetTheme, Theme? newTargetTheme);

public delegate void ThemeConfigurationPageEntryModifiedEventHandler(ThemeConfigurationPage sender, string themeKey, bool isAdded);

public delegate void ThemeConfigurationPageEventHandler(ThemeConfigurationPage sender, Dictionary<string, ISavedThemeEntry> oldItems);

/// <summary>
/// A folder/entry hierarchy for a theme for 
/// </summary>
public class ThemeConfigurationPage : ConfigurationPage {
    private Dictionary<string, ISavedThemeEntry>? originalBrushes;
    private Theme? targetTheme;

    /// <summary>
    /// Gets our root entry group
    /// </summary>
    public ThemeConfigEntryGroup Root { get; }

    /// <summary>
    /// Gets or sets the theme that we want to edit. This is set to the application's
    /// active theme when the page is loaded
    /// </summary>
    public Theme? TargetTheme {
        get => this.targetTheme;
        set {
            Theme? oldTargetTheme = this.targetTheme;
            if (oldTargetTheme == value) {
                return;
            }

            if (this.originalBrushes != null && this.originalBrushes.Count > 0) {
                throw new InvalidOperationException("un-applied changes");
            }

            this.targetTheme = value;
            this.TargetThemeChanged?.Invoke(this, oldTargetTheme, value);
        }
    }

    public event ThemeConfigurationPageTargetThemeChangedEventHandler? TargetThemeChanged;

    /// <summary>
    /// An event fired when a theme entry is added to our internal set of modified entries
    /// </summary>
    public event ThemeConfigurationPageEntryModifiedEventHandler? ThemeEntryModified;

    /// <summary>
    /// An event fired just before our internal collection of modified theme entries is cleared
    /// </summary>
    public event ThemeConfigurationPageEventHandler? ModifiedThemeEntriesCleared;

    public ThemeConfigurationPage() {
        this.Root = new ThemeConfigEntryGroup("<root>");
    }

    /// <summary>
    /// Creates a theme configuration tree entry that controls the given theme key
    /// </summary>
    /// <param name="fullPath">The full path of the configuration entry</param>
    /// <param name="themeKey">The theme key</param>
    /// <param name="description">An optional description of what this theme key is used for</param>
    public ThemeConfigEntry AssignMapping(string fullPath, string themeKey, string? description = null) {
        Validate.NotNullOrWhiteSpaces(fullPath);
        Validate.NotNullOrWhiteSpaces(themeKey);
        ApplicationPFX.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);

        ThemeConfigEntryGroup parent = this.Root;
        int i, j = 0; // i = index, j = last index
        while ((i = fullPath.IndexOf('/', j)) != -1) {
            if (i == j)
                throw new ArgumentException("Full path contained a double forward slash");

            parent = parent.GetOrCreateGroupByName(fullPath.JSubstring(j, i));
            j = i + 1;
        }

        ThemeConfigEntry entry = parent.CreateEntry(fullPath.Substring(j), themeKey);
        if (!string.IsNullOrWhiteSpace(description))
            entry.Description = description;

        return entry;
    }

    public void OnChangingThemeColour(string themeKey) {
        if (this.targetTheme == null) {
            throw new InvalidOperationException("TargetTheme is not set");
        }

        if (this.originalBrushes == null) {
            this.originalBrushes = new Dictionary<string, ISavedThemeEntry>();
        }
        else if (this.originalBrushes.ContainsKey(themeKey)) {
            return;
        }

        this.originalBrushes.Add(themeKey, this.targetTheme.SaveThemeEntry(themeKey));
        this.ThemeEntryModified?.Invoke(this, themeKey, true);
    }

    public override ValueTask OnContextCreated(ConfigurationContext context) {
        this.TargetTheme = ThemeManager.Instance.ActiveTheme;
        return base.OnContextCreated(context);
    }

    public override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.TargetTheme = null;
        return base.OnContextDestroyed(context);
    }

    protected override void OnActiveContextChanged(ConfigurationContext? oldContext, ConfigurationContext? newContext) {
        base.OnActiveContextChanged(oldContext, newContext);
        this.MarkModified();
    }

    public override ValueTask RevertLiveChanges(List<ApplyChangesFailureEntry>? errors) {
        if (this.targetTheme == null) {
            throw new InvalidOperationException("TargetTheme is not set. "
                                                + nameof(this.OnContextDestroyed) +
                                                " should have been called AFTER this, since that is what clears the target theme");
        }

        this.ReverseChanges(this.targetTheme);
        return ValueTask.CompletedTask;
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        this.ClearOriginalBrushesInternal();

        ThemeConfigurationOptions options = ThemeConfigurationOptions.Instance;
        options.SaveThemesToModels(ThemeManager.Instance);
        options.StorageManager.SaveArea(options);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Applies our changes to a new theme and reverts them in a previous theme (used when creating a copy of a theme)
    /// </summary>
    /// <param name="revert">The theme to revert the changes of</param>
    /// <param name="apply">The theme to apply changes to</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ApplyAndRevertChanges(Theme revert, Theme apply) {
        if (this.targetTheme == null)
            throw new InvalidOperationException("This page has no target theme");

        if (this.originalBrushes != null && this.originalBrushes.Count > 0) {
            foreach (KeyValuePair<string, ISavedThemeEntry> brush in this.originalBrushes) {
                // Get current theme entry and apply to the new theme
                apply.ApplyThemeEntry(brush.Key, revert.SaveThemeEntry(brush.Key));

                // Revert the entry back to the old one
                revert.ApplyThemeEntry(brush.Key, brush.Value);
            }

            this.ClearOriginalBrushesInternal();
        }
    }

    public void ReverseChanges(Theme? target) {
        target ??= this.targetTheme;
        if (target == null)
            throw new InvalidOperationException("The target theme is null and this page has no target theme");

        if (this.originalBrushes != null && this.originalBrushes.Count > 0) {
            foreach (KeyValuePair<string, ISavedThemeEntry> brush in this.originalBrushes) {
                target.ApplyThemeEntry(brush.Key, brush.Value);
            }

            this.ClearOriginalBrushesInternal();
        }
    }

    public bool ReverseChangeFor(Theme theme, string themeKey) {
        Validate.NotNull(theme);
        Validate.NotNullOrWhiteSpaces(themeKey);

        if (this.originalBrushes != null && this.originalBrushes.Remove(themeKey, out ISavedThemeEntry? entry)) {
            this.ThemeEntryModified?.Invoke(this, themeKey, false);
            theme.ApplyThemeEntry(themeKey, entry);
            return true;
        }

        return false;
    }

    public bool HasThemeKeyChanged(Theme theme, string themeKey) {
        Validate.NotNull(theme);
        Validate.NotNullOrWhiteSpaces(themeKey);
        return this.originalBrushes != null && this.originalBrushes.ContainsKey(themeKey);
    }

    private void ClearOriginalBrushesInternal() {
        if (this.originalBrushes?.Count > 0) {
            Dictionary<string, ISavedThemeEntry> items = this.originalBrushes.ToDictionary();
            this.originalBrushes = null;
            this.ModifiedThemeEntriesCleared?.Invoke(this, items);
        }
    }
}