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
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils.Collections;

namespace FramePFX.AdvancedContextService.NCSP {
    /// <summary>
    /// Provides a registration for object context menus
    /// </summary>
    public class ContextRegistry {
        public static ContextRegistry Instance { get; } = new ContextRegistry();

        private readonly InheritanceDictionary<TypeRegistration> map;

        public ContextRegistry() {
            this.map = new InheritanceDictionary<TypeRegistration>();
        }

        public IContextRegistration RegisterType(Type type, bool canInheritFromParent = true) {
            if (this.map.TryGetLocalValue(type, out TypeRegistration registration))
                throw new InvalidOperationException("Type already registered: " + type);
            this.map.SetValue(type, registration = new TypeRegistration(this, type, canInheritFromParent));
            return registration;
        }

        public bool GetTypeRegistration(Type type, out IContextRegistration registration) {
            if (!this.map.TryGetLocalValue(type, out TypeRegistration x)) {
                registration = null;
                return false;
            }

            registration = x;
            return true;
        }

        public List<IContextEntry> GetActions(object target, IContextData ctx, bool checkCanExecute = true) {
            List<IContextEntry> entries = new List<IContextEntry>();
            InheritanceDictionary<TypeRegistration>.LocalValueEntryEnumerator enumerator = this.map.GetLocalValueEnumerator(target.GetType());
            if (!enumerator.MoveNext()) {
                return entries;
            }

            List<TypeRegistration> list = new List<TypeRegistration>();
            foreach (ITypeEntry<TypeRegistration> entry in this.map.GetLocalValueEnumerable(target.GetType())) {
                TypeRegistration reg = entry.LocalValue;
                list.Add(reg);
                if (!reg.canInheritFromParent)
                    break;
            }

            for (int i = 0, endIndex = list.Count - 1; i <= endIndex; i++) {
                TypeRegistration reg = list[i];
                foreach (IContextEntry entry in reg.actions) {
                    if (entry is CommandContextEntry ace) {
                        if (string.IsNullOrWhiteSpace(ace.CommandId) || (checkCanExecute && CommandManager.Instance.CanExecute(ace.CommandId, ctx, true) != ExecutabilityState.Executable)) {
                            continue;
                        }
                    }

                    entries.Add(entry);
                }

                // slightly reduce memory usage
                if (reg.actions.Count > 0 && i != endIndex && (i == 0 || !(entries[i - 1] is SeparatorEntry))) {
                    entries.Add(SeparatorEntry.NewInstance);
                }
            }

            return entries;
        }

        public class TypeRegistration : IContextRegistration {
            public readonly ContextRegistry registry;
            public readonly Type type;
            public readonly List<IContextEntry> actions;
            public readonly bool canInheritFromParent;

            bool IContextRegistration.CanInheritFromParent => this.canInheritFromParent;

            public TypeRegistration(ContextRegistry owner, Type type, bool canInheritFromParent) {
                this.type = type;
                this.canInheritFromParent = canInheritFromParent;
                this.actions = new List<IContextEntry>();
            }

            public void AddEntry(IContextEntry entry) {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));
                this.actions.Add(entry);
            }
        }
    }

    public interface IContextRegistration {
        bool CanInheritFromParent { get; }

        void AddEntry(IContextEntry entry);
    }
}