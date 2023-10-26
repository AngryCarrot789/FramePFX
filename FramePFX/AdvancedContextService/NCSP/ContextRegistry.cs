using System;
using System.Collections.Generic;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Utils.Collections;

namespace FramePFX.AdvancedContextService.NCSP {
    /// <summary>
    /// Provides a registration
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

        public List<IContextEntry> GetActions(object target, IDataContext ctx) {
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
                    if (entry is ActionContextEntry ace) {
                        if (string.IsNullOrWhiteSpace(ace.ActionId) || !ActionManager.Instance.CanExecute(ace.ActionId, ctx, true)) {
                            continue;
                        }
                    }

                    entries.Add(entry);
                }

                // slightly reduce memory usage
                if (reg.actions.Count > 0 && i != endIndex && (i == 0 || !(entries[i - 1] is SeparatorEntry))) {
                    entries.Add(SeparatorEntry.Instance);
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