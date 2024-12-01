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

namespace FramePFX.Editing.ResourceManaging.Autoloading;

public delegate void InvalidResourceEntryEventHandler(InvalidResourceEntry entry);

public abstract class InvalidResourceEntry {
    private string? displayName;

    public ResourceItem Resource { get; }

    public ResourceLoader? ResourceLoader { get; private set; }

    public string? DisplayName {
        get => this.displayName;
        set {
            if (this.displayName == value)
                return;
            this.displayName = value;
            this.DisplayNameChanged?.Invoke(this);
        }
    }

    public event InvalidResourceEntryEventHandler? DisplayNameChanged;

    protected InvalidResourceEntry(ResourceItem resource) {
        this.Resource = resource;
    }

    public bool TryLoad() {
        return this.ResourceLoader?.TryLoadEntry(this) ?? throw new InvalidOperationException("No loader");
    }

    internal static void InternalSetLoader(InvalidResourceEntry resource, ResourceLoader? loader) {
        resource.ResourceLoader = loader;
    }
}