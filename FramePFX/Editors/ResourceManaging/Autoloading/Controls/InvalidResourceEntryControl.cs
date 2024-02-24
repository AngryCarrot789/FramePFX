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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.ResourceManaging.Autoloading.Controls {
    /// <summary>
    /// A base control for the content panel that allows the user to fixed an invalid resource
    /// </summary>
    public abstract class InvalidResourceEntryControl : Control {
        private static readonly Dictionary<Type, Func<InvalidResourceEntryControl>> Constructors;

        public InvalidResourceEntry Entry { get; private set; }

        protected InvalidResourceEntryControl() {
            this.Loaded += (s, e) => ((InvalidResourceEntryControl) s).OnLoaded();
            this.Unloaded += (s, e) => ((InvalidResourceEntryControl) s).OnUnloaded();
        }

        static InvalidResourceEntryControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InvalidResourceEntryControl), new FrameworkPropertyMetadata(typeof(InvalidResourceEntryControl)));
            Constructors = new Dictionary<Type, Func<InvalidResourceEntryControl>>();
            RegisterType(typeof(InvalidImagePathEntry), () => new InvalidImagePathEntryControl());
            RegisterType(typeof(InvalidMediaPathEntry), () => new InvalidMediaPathEntryControl());
        }

        public virtual void AttachToEntry(InvalidResourceEntry entry) {
            this.Entry = entry;
        }

        public virtual void DetatchFromEntry() {
            this.Entry = null;
        }

        protected abstract void OnLoaded();

        protected abstract void OnUnloaded();

        protected void GetTemplateChild<T>(string name, out T value) where T : DependencyObject {
            if ((value = this.GetTemplateChild(name) as T) == null)
                throw new Exception("Missing part: " + name + " of type " + typeof(T));
        }

        protected T GetTemplateChild<T>(string name) where T : DependencyObject {
            this.GetTemplateChild(name, out T value);
            return value;
        }

        protected bool TryGetTemplateChild<T>(string name, out T value) where T : DependencyObject {
            return (value = this.GetTemplateChild(name) as T) != null;
        }

        public static void RegisterType<T>(Type resourceType, Func<T> func) where T : InvalidResourceEntryControl {
            Constructors[resourceType] = func;
        }

        public static InvalidResourceEntryControl NewInstance(Type resourceType) {
            if (resourceType == null) {
                throw new ArgumentNullException(nameof(resourceType));
            }

            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            bool hasLogged = false;
            for (Type type = resourceType; type != null; type = type.BaseType) {
                if (Constructors.TryGetValue(type, out var func)) {
                    return func();
                }

                if (!hasLogged) {
                    hasLogged = true;
                    Debugger.Break();
                    Debug.WriteLine("Could not find control for resource type on first try. Scanning base types");
                }
            }

            throw new Exception("No such content control for resource type: " + resourceType.Name);
        }
    }
}