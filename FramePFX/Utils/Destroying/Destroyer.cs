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

using System;
using System.Collections.Generic;

namespace FramePFX.Utils.Destroying
{
    public static class Destroyer
    {
        private static readonly HashSet<IDestroy> rootNodes;
        private static readonly Dictionary<IDestroy, Node> map;
        private static readonly object locker;

        static Destroyer()
        {
            ReferenceEqualityComparer<IDestroy> refEquality = new ReferenceEqualityComparer<IDestroy>();
            rootNodes = new HashSet<IDestroy>(refEquality);
            map = new Dictionary<IDestroy, Node>(refEquality);
            locker = new object();
        }

        private class destroyableDummy : IDestroy
        {
            private readonly string debugName;

            public destroyableDummy(string debugName) => this.debugName = debugName ?? "destroyableDummy";

            public void Destroy()
            {
            }

            public override string ToString() => this.debugName;
        }

        public static IDestroy CreateDummy(string debugName = "destroyableDummy") => new destroyableDummy(debugName);

        public static IDestroy CreateDummy(IDestroy parent, string debugName = null)
        {
            IDestroy dummy = CreateDummy(debugName);
            Register(parent, dummy);
            return dummy;
        }

        /// <summary>
        /// Registers a child destroyable object with the given parent object
        /// </summary>
        /// <param name="parent">The parent object which will contain the child after this call</param>
        /// <param name="child">The child object which will be added to the parent</param>
        /// <exception cref="ArgumentNullException">Parent or child were null</exception>
        /// <exception cref="InvalidOperationException">Parent was a descendent of child</exception>
        public static void Register(IDestroy parent, IDestroy child)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            lock (locker)
            {
                Node parentNode = GetNodeOrCreate(parent, null);
                Node childNode = GetNodeOrCreate(child, parentNode);

                for (Node node = parentNode; node != null; node = node.parent)
                {
                    if (node == childNode)
                    {
                        throw new InvalidOperationException("Parent was a descendent of Child (basically, tried to add child as its own child). This is not allowed");
                    }
                }

                parentNode.Add(childNode);
            }
        }

        /// <summary>
        /// Tries to remove the child from the parent. Returns true if this was successful,
        /// otherwise false (no node for the parent or child, or the parent did not contain the child)
        /// </summary>
        /// <param name="parent">The parent</param>
        /// <param name="child">The child to remove</param>
        /// <returns>True if removed, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Parent or child were null</exception>
        public static bool Unregister(IDestroy parent, IDestroy child)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            lock (locker)
            {
                if (!map.TryGetValue(parent, out Node parentNode))
                    return false;
                if (!map.TryGetValue(child, out Node childNode))
                    return false;
                if (childNode.parent != parentNode)
                    return false;
                parentNode.Remove(childNode);
                return true;
            }
        }

        /// <summary>
        /// Destroys the object's hierarchy
        /// </summary>
        /// <param name="destroyable">The object to destroy along with its hierarchy</param>
        /// <param name="canProcessUnregistered">True to destroy the object if it is not registered, false to only destroy if registered</param>
        /// <exception cref="ArgumentNullException">Destroyable object is null</exception>
        public static void Destroy(IDestroy destroyable, bool canProcessUnregistered = true)
        {
            if (destroyable == null)
                throw new ArgumentNullException(nameof(destroyable));

            lock (locker)
            {
                if (!map.TryGetValue(destroyable, out Node node) && !canProcessUnregistered)
                {
                    return;
                }

                List<IDestroy> objs = new List<IDestroy>();
                if (node == null)
                {
                    objs.Add(destroyable);
                }
                else
                {
                    node.PrepareDisposal(objs);
                }

                ErrorList errors = null;
                foreach (IDestroy item in objs)
                {
                    try
                    {
                        item.Destroy();
                    }
                    catch (Exception e)
                    {
                        (errors ?? (errors = new ErrorList("Exception while disposing one or more objects"))).Add(e);
                    }
                }

                errors?.Dispose();
            }
        }

        private static Node GetNodeOrCreate(IDestroy destroyable, Node newParent)
        {
            Node node = GetNode(destroyable);
            if (node == null)
            {
                map[destroyable] = node = new Node(destroyable);
                if (newParent == null)
                {
                    rootNodes.Add(destroyable);
                }
            }
            else if (node.parent != null)
            {
                node.parent.Remove(node);
            }
            else
            {
                rootNodes.Remove(destroyable);
            }

            return node;
        }

        private static Node GetNode(IDestroy destroyable)
        {
            return map.TryGetValue(destroyable, out Node node) ? node : null;
        }

        private class Node
        {
            public readonly IDestroy destroyable;
            public Node parent;
            public List<Node> children;

            public Node(IDestroy destroyable)
            {
                this.destroyable = destroyable;
            }

            public void Add(Node node)
            {
                List<Node> list = this.children;
                if (list == null)
                    this.children = list = new List<Node>();
                list.Add(node);
                node.parent = this;
            }

            public void Remove(Node node)
            {
                List<Node> list = this.children;
                if (list != null && list.Count > 0)
                {
                    int index = list.LastIndexOf(node);
                    if (index == -1)
                    {
                        throw new InvalidOperationException("Child node did not exist in this node");
                    }

                    list.RemoveAt(index);
                    node.parent = null;
                }
            }

            public void PrepareDisposal(List<IDestroy> list)
            {
                // generate tree
                List<Node> nodes = this.children;
                if (nodes != null)
                {
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        nodes[i].PrepareDisposal(list);
                    }
                }

                // remove entry from disposer tree
                IDestroy obj = this.destroyable;
                map.Remove(obj);
                if (this.parent != null)
                {
                    this.parent.Remove(this);
                }
                else
                {
                    rootNodes.Remove(obj);
                }

                this.children = null;
                list.Add(obj);
            }
        }
    }
}