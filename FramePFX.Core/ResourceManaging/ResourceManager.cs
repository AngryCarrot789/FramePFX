using System;
using System.Collections.Generic;

namespace FramePFX.Core.ResourceManaging {
    public class ResourceManager {
        private readonly Dictionary<int, ResourceBase> resources;

        public ResourceManager() {
            this.resources = new Dictionary<int, ResourceBase>();
        }

        public ResourceBase GetResource(int id) {
            return this.resources.TryGetValue(id, out ResourceBase value) ? value : null;
        }

        public T GetResource<T>(int id) where T : ResourceBase {
            return this.resources.TryGetValue(id, out ResourceBase value) && value is T t ? t : null;
        }

        public bool TryGetResource(int id, out ResourceBase value) {
            return this.resources.TryGetValue(id, out value);
        }

        public bool TryGetResource<T>(int id, out T value) where T : ResourceBase {
            if (this.resources.TryGetValue(id, out ResourceBase resourceBase) && resourceBase is T) {
                value = (T) resourceBase;
                return true;
            }
            else {
                value = null;
                return false;
            }
        }

        public bool RemoveResource(int key) {
            if (this.resources.TryGetValue(key, out ResourceBase value)) {
                value.resourceId = -1;
                this.resources.Remove(key);
                return true;
            }
            else {
                return false;
            }
        }

        public void AddResource(int id, ResourceBase resource) {
            if (this.resources.TryGetValue(id, out ResourceBase existing)) {
                throw new Exception($"Resource already registered with id {id}: {existing}");
            }

            this.resources[id] = resource;
            resource.resourceId = id;
        }
    }
}