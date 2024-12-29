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

using FramePFX.Configurations;
using FramePFX.Utils;

namespace FramePFX.Themes.Configurations;

public delegate void ThemeConfigurationPageTargetThemeChangedEventHandler(ThemeConfigurationPage sender, Theme? oldTargetTheme, Theme? newTargetTheme);

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
    /// Gets or sets the theme that we want to edit
    /// </summary>
    public Theme? TargetTheme {
        get => this.targetTheme;
        set {
            Theme? oldTargetTheme = this.targetTheme;
            if (oldTargetTheme == value)
                return;

            this.targetTheme = value;
            this.TargetThemeChanged?.Invoke(this, oldTargetTheme, value);
        }
    }

    public event ThemeConfigurationPageTargetThemeChangedEventHandler? TargetThemeChanged;

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
        Application.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);

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
        if (this.originalBrushes != null && this.originalBrushes.Count > 0) {
            foreach (KeyValuePair<string, ISavedThemeEntry> brush in this.originalBrushes) {
                ThemeManager.Instance.ActiveTheme.RestoreThemeEntry(brush.Key, brush.Value);
            }

            this.originalBrushes = null;
        }

        return ValueTask.CompletedTask;
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        this.originalBrushes = null;

        // TODO: save theme
        return ValueTask.CompletedTask;
    }
}