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

namespace FramePFX.Interactivity;

public delegate void SelectionChangedEventHandler<T>(ISelectionManager<T> sender, IList<T>? oldItems, IList<T>? newItems);

public delegate void SelectionClearedEventHandler<T>(ISelectionManager<T> sender);

public delegate void LightSelectionChangedEventHandler<T>(ILightSelectionManager<T> sender);

public static class BaseSelectionManagerExtensions {
    /// <summary>
    /// A helper method to flip the selected state
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    public static void ToggleSelected<T>(this IBaseSelectionManager<T> manager, T item) {
        if (manager.IsSelected(item)) {
            manager.Unselect(item);
        }
        else {
            manager.Select(item);
        }
    }
}

/// <summary>
/// A base interface for general selection managers providing get/set/add/remove/clear/is support
/// </summary>
/// <typeparam name="T">The type of selected items supported</typeparam>
public interface IBaseSelectionManager<in T> {
    /// <summary>
    /// Returns true when the item is selected
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>See summary</returns>
    bool IsSelected(T item);

    /// <summary>
    /// Effectively makes the item the only selected item
    /// </summary>
    /// <param name="item">The item to be the only selected item</param>
    void SetSelection(T item);

    /// <summary>
    /// Effectively makes all of the given items the only selected items
    /// </summary>
    /// <param name="items">The items to be the only selected items</param>
    void SetSelection(IEnumerable<T> items);

    /// <summary>
    /// Adds the item as a selected item
    /// </summary>
    /// <param name="item">The item to become selected</param>
    void Select(T item);

    /// <summary>
    /// Adds the items as selected items
    /// </summary>
    /// <param name="items">The items to become selected</param>
    void Select(IEnumerable<T> items);

    /// <summary>
    /// Removes the item from being selected
    /// </summary>
    /// <param name="item">The item to become un-selected</param>
    void Unselect(T item);

    /// <summary>
    /// Removes the items from being selected
    /// </summary>
    /// <param name="items">The items to become un-selected</param>
    void Unselect(IEnumerable<T> items);

    /// <summary>
    /// Toggles the selected state of the item
    /// </summary>
    /// <param name="item">The item to toggle the selection of</param>
    void ToggleSelected(T item);

    /// <summary>
    /// Clears all selected items
    /// </summary>
    void Clear();
}

/// <summary>
/// An interface for an object that manages the selection state of items
/// </summary>
/// <typeparam name="T">The type of selected items supported</typeparam>
public interface ISelectionManager<T> : IBaseSelectionManager<T> {
    /// <summary>
    /// Gets an enumerable of the selected items. Ideally, create a list from
    /// this as soon as possible because the enumerable may be cached and become
    /// invalid at some point
    /// </summary>
    IEnumerable<T> SelectedItems { get; }

    /// <summary>
    /// Gets the number of selected items. Worst case scenario this involves fully
    /// enumerating <see cref="SelectedItems"/> unless implemented correctly
    /// </summary>
    int Count { get; }

    /// <summary>
    /// An event fired when the selection changes (item added or removed)
    /// </summary>
    public event SelectionChangedEventHandler<T>? SelectionChanged;

    /// <summary>
    /// An event fired when the selection is cleared
    /// </summary>
    public event SelectionClearedEventHandler<T>? SelectionCleared;
}

/// <summary>
/// An interface for a simple selection manager that has basic selection methods and a basic selection changed event
/// </summary>
/// <typeparam name="T">The type of selected items supported</typeparam>
public interface ILightSelectionManager<T> : IBaseSelectionManager<T> {
    /// <summary>
    /// Gets an enumerable of the selected items. Ideally, create a list from
    /// this as soon as possible because the enumerable may be cached and become
    /// invalid at some point
    /// </summary>
    IEnumerable<T> SelectedItems { get; }

    /// <summary>
    /// Gets the number of selected items. Worst case scenario this involves fully
    /// enumerating <see cref="SelectedItems"/> unless implemented correctly
    /// </summary>
    int Count { get; }

    /// <summary>
    /// An event fired when the selection changes. This does not contain what changes, it just marks a change
    /// happening (it could be selection cleared, item added, maybe multiple items being added or removed, etc.)
    /// </summary>
    event LightSelectionChangedEventHandler<T> SelectionChanged;
}