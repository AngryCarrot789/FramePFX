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

using System.Collections.Generic;
using Avalonia;
using FramePFX.Avalonia.Utils;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Interactivity;

/// <summary>
/// A tree, like a visual tree, but for controls that have contextual data.
/// This is used as an optimisation to reduce traversing DOWN the visual tree as much as
/// possible, since it can be extremely expensive for controls near the top level
/// </summary>
public class LinkedContextTree
{
    /// <summary>
    /// An entry for a control within a context tree
    /// </summary>
    public class ContextTreeEntry
    {
        public readonly AvaloniaObject Control;
        public readonly ContextData ContextData;

        public ContextTreeEntry(AvaloniaObject control, ContextData contextData)
        {
            this.Control = control;
            this.ContextData = contextData;
        }
    }
    
    private static readonly AttachedProperty<LinkedContextTree?> ContextTreeProperty =
        AvaloniaProperty.RegisterAttached<LinkedContextTree, AvaloniaObject, LinkedContextTree?>("ContextTree");
    
    // TODO: use a linked list approach. A class of type 'LinkedContextTree'
    // When ContextData is set:
    //   First scan element (whose context changed) and visual parents for a tree
    //   If tree found:
    //     Insert the element into the tree just after the closest parent with context set
    //   If not found:
    //     Scan visual children to find a tree.
    //     If one is found:
    //       Make element the new parent and the previous parent a child of it
    //     If not:
    //      Create a new tree it with element being the parent
    
    public ContextTreeEntry topLevel;       // the top-level
    public List<ContextTreeEntry> children; // a list of children with context data set

    public static void OnContextDataChanged(AvaloniaObject owner, ContextData newData, bool isCleared)
    {
        LinkedContextTree? tree = owner.GetValue(ContextTreeProperty);
        if (tree == null)
        {
            tree = FindTreeInParents(VisualTreeUtils.GetParent(owner), out AvaloniaObject? localParentOwner);

            // First scan parents
        }
    }

    /// <summary>
    /// Tries to find the context tree, scanning the control and its parent controls
    /// </summary>
    /// <param name="control">The control to start searching at</param>
    /// <param name="localOwner">The object in which the tree existed in</param>
    /// <returns></returns>
    private static LinkedContextTree FindTreeInParents(AvaloniaObject? control, out AvaloniaObject? localOwner)
    {
        localOwner = null;
        return null;
    }
}