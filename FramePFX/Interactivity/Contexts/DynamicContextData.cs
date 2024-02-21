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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Windows;
using FramePFX.Utils;

namespace FramePFX.Interactivity.Contexts {
    public class DynamicContextData : IContextData {
        private readonly WeakReference objRef;
        private Dictionary<string, object> cachedValues;

        public DynamicContextData(DependencyObject obj) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            this.objRef = new WeakReference(obj);
        }

        public void InvalidateCache() {
            this.cachedValues = null;
        }

        public bool TryGetContext(string key, out object value) {
            if (this.cachedValues != null && this.cachedValues.TryGetValue(key, out value)) {
                return true;
            }

            if (this.objRef.Target is DependencyObject obj && TryGetDataCore(obj, key, out value)) {
                (this.cachedValues ?? (this.cachedValues = new Dictionary<string, object>()))[key] = value;
                return true;
            }

            value = null;
            return false;
        }

        public bool ContainsKey(DataKey key) {
            return this.TryGetContext(key.Id, out _);
        }

        public bool ContainsKey(string key) => this.TryGetContext(key, out _);

        public static bool TryGetDataCore(DependencyObject startingObject, string key, out object value) {
            for (DependencyObject dp = startingObject; dp != null; dp = VisualTreeUtils.GetParent(dp)) {
                object localEntry = dp.ReadLocalValue(DataManager.ContextDataProperty);
                if (localEntry != DependencyProperty.UnsetValue && localEntry is IContextData dpCtx) {
                    if (dpCtx.TryGetContext(key, out value)) {
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }
    }
}