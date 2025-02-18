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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Configurations;

/// <summary>
/// Represents an entry in the configuration tree. This may contain a page object which
/// is what will be presented when this entry is selected in the UI. If the page is null,
/// but we have child items, the first available child item's page will be shown instead
/// </summary>
public class ConfigurationEntry {
    private readonly List<ConfigurationEntry> items;
    private string? id;
    private string? fullIdPath;
    private ConfigurationEntry? myParent;

    /// <summary>
    /// Gets this entry's child items
    /// </summary>
    public IReadOnlyList<ConfigurationEntry> Items {
        get => this.items;
        init {
            foreach (ConfigurationEntry item in value)
                this.AddEntry(item);
        }
    }

    /// <summary>
    /// Gets this entry's readable display name. This is what is shown in the configuration
    /// tree nodes and also in the navigation panel on the top of the configuration dialog
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets a unique identifier for this entry. This is typically something along the lines of:
    /// <code>config.rootsection.subsection</code>
    /// </summary>
    public string? Id {
        get => this.id;
        init {
            if (this.myParent != null) {
                throw new InvalidOperationException("Id cannot be changed once the parent is set");
            }

            Validate.NotNullOrWhiteSpaces(value);
            if (value.Contains('/') || value.Contains('\\'))
                throw new InvalidOperationException("Id cannot contain a forward or back slash character");

            this.id = value;
            this.fullIdPath = null;
        }
    }

    /// <summary>
    /// Gets the full identifier path, updated when this entry is added to or removed
    /// from a parent configuration entry. This is unique across a configuration hierarchy, and
    /// is used to find a specific entry e.g. for plugins 
    /// </summary>
    public string? FullIdPath {
        get => this.fullIdPath ??= GetFullPath(this);
    }

    /// <summary>
    /// Gets the configuration page for this entry. May be null, in which case, the
    /// first available child item's page should be used in the UI.
    /// </summary>
    public ConfigurationPage? Page { get; init; }

    public ConfigurationEntry? Parent => this.myParent;

    /// <summary>
    /// Returns true when we are the root entry. This is a helper for checking if our parent is null
    /// </summary>
    public bool IsRoot => this.Parent == null;

    public ConfigurationEntry() {
        this.items = new List<ConfigurationEntry>();
    }

    public bool TryGetEntry(string id, [NotNullWhen(true)] out ConfigurationEntry? entry) {
        foreach (ConfigurationEntry theEntry in this.items) {
            if (theEntry.id == id) {
                entry = theEntry;
                return true;
            }
        }

        entry = null;
        return false;
    }

    public bool TryGetEntryByFullId(string fullId, [NotNullWhen(true)] out ConfigurationEntry? entry) {
        foreach (ConfigurationEntry theEntry in this.items) {
            if (theEntry.fullIdPath == fullId) {
                entry = theEntry;
                return true;
            }
        }

        entry = null;
        return false;
    }

    public bool TryFindEntry(string fullId, [NotNullWhen(true)] out ConfigurationEntry? entry) {
        if (this.TryGetEntryByFullId(fullId, out entry)) {
            return true;
        }

        foreach (ConfigurationEntry item in this.items) {
            if (item.TryGetEntryByFullId(fullId, out entry)) {
                return true;
            }
        }

        return false;
    }

    public void AddEntry(ConfigurationEntry entry) {
        if (entry.Parent != null)
            throw new InvalidOperationException("Entry already added to another entry");

        if (entry.Id == null)
            throw new InvalidOperationException("Entry has no ID");

        Debug.Assert(!this.items.Contains(entry), "Did not expect our list to contain the entry");
        this.items.Add(entry);
        entry.myParent = this;
        entry.fullIdPath = null;
    }

    private static string? GetFullPath(ConfigurationEntry entry) {
        if (entry.Parent == null) {
            return null;
        }
        else {
            if (entry.Id == null)
                return null;

            string? parentPath = entry.Parent.FullIdPath;
            return parentPath == null ? entry.id : (parentPath + '/' + entry.Id);
        }
    }
}