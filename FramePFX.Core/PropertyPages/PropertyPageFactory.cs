using System;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Core.PropertyPages {
    /// <summary>
    /// Base class for a property page registry. This class is meant to map a view model (e.g. video
    /// clip view model) into a new collection of <see cref="PropertyPageViewModel{T}"/> instances which
    /// hold a reference to the actual view model
    /// <para>
    /// View models can also hide specific pages so that the user cannot modify them
    /// </para>
    /// </summary>
    /// <typeparam name="TBase">The lowest base type that this registry contains (e.g. <see cref="BaseViewModel"/>)</typeparam>
    public abstract class PropertyPageFactory<TBase, TPageBase> where TBase : BaseViewModel where TPageBase : PropertyPageViewModel<TBase> {
        // typeof(TextClipViewModel) -> List { typeof(MediaPositionPageViewModel), typeof(TextPropertyPageViewModel) }
        private readonly Dictionary<Type, TypeEntry> Map;

        protected PropertyPageFactory() {
            this.Map = new Dictionary<Type, TypeEntry>();
        }

        private TypeEntry GetEntry(Type type) {
            if (!this.Map.TryGetValue(type, out TypeEntry entry)) {
                this.Map[type] = entry = new TypeEntry(type);
            }

            return entry;
        }

        protected void RegisterPage<T, TPage>() where T : TBase where TPage : TPageBase {
            List<Type> list = this.GetEntry(typeof(T)).types;
            if (!list.Contains(typeof(TPage)))
                list.Add(typeof(TPage));
        }

        protected void HidePage<T, TPage>() where T : TBase where TPage : TPageBase {
            this.GetEntry(typeof(T)).hidden.Add(typeof(TPage));
        }

        public static IEnumerable<Type> EnumerateHierarchy(Type topLevelType) {
            Type baseType = typeof(TBase);
            for (Type type = topLevelType; type != null && baseType.IsAssignableFrom(type); type = type.BaseType) {
                yield return type;
            }
        }

        private IEnumerable<TypeEntry> EnumerateRegistryHierarchy(Type topLevelType) {
            foreach (Type type in EnumerateHierarchy(topLevelType)) {
                if (this.Map.TryGetValue(type, out TypeEntry entry)) {
                    yield return entry;
                }
            }
        }

        private List<Type> GetTypes(Type topLevelType) {
            List<Type> types = new List<Type>();
            HashSet<Type> hidden = new HashSet<Type>();
            foreach (TypeEntry entry in this.EnumerateRegistryHierarchy(topLevelType)) {
                types.AddRange(entry.types);
                foreach (Type type in entry.hidden) {
                    hidden.Add(type);
                }
            }

            types.RemoveAll(hidden.Contains);
            return types;
        }

        public List<TPageBase> CreatePages(TBase instance) {
            List<TPageBase> pages = new List<TPageBase>();
            foreach (Type type in this.GetTypes(instance.GetType())) {
                TPageBase page = this.CreateInstance(instance, type);
                pages.Add(page);
            }

            pages.Reverse();
            return pages;
        }

        public List<TPageBase> CreatePages(List<TBase> instances) {
            if (instances == null || instances.Count < 1) {
                return new List<TPageBase>();
            }

            if (instances.Count == 1) {
                return this.CreatePages(instances[0]);
            }

            return new List<TPageBase>();

            // List<TPageBase> pages = new List<TPageBase>();
            // foreach (Type type in this.GetTypes(instances.GetType())) {
            //     TPageBase page = this.CreateInstance(enumerable, type);
            //     pages.Add(page);
            // }
            // return pages;
        }

        protected virtual TPageBase CreateInstance(TBase instance, Type pageType) {
            return (TPageBase) Activator.CreateInstance(pageType, instance);
        }

        /// <summary>
        /// Stores a list of property page view model types for a specific type, along with hidden page info
        /// </summary>
        private class TypeEntry {
            public readonly Type type;
            public readonly List<Type> types;
            public readonly HashSet<Type> hidden;

            public TypeEntry(Type type) {
                this.type = type;
                this.types = new List<Type>();
                this.hidden = new HashSet<Type>();
            }
        }
    }
}