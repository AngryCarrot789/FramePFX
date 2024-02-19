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

namespace FramePFX.Editors.ResourceManaging.ResourceHelpers {
    // public delegate void EntryResourceChangedEventHandler(IBaseResourcePathKey key, ResourceItem oldItem, ResourceItem newItem);
    // public delegate void EntryResourceModifiedEventHandler(IBaseResourcePathKey key, ResourceItem resource, string property);
    // public delegate void EntryOnlineStateChangedEventHandler(IBaseResourcePathKey key);

    public delegate void EntryResourceChangedEventHandler<T>(IResourcePathKey<T> key, T oldItem, T newItem) where T : ResourceItem;
    public delegate void EntryResourceModifiedEventHandler<T>(IResourcePathKey<T> key, T resource, string property) where T : ResourceItem;
    public delegate void EntryOnlineStateChangedEventHandler<T>(IResourcePathKey<T> key) where T : ResourceItem;
}