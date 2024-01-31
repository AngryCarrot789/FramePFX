using System;

namespace FramePFX {
    public interface IServiceManager {
        bool HasService<T>();
        bool HasService(Type serviceType);

        T GetService<T>();
        object GetService(Type type);

        bool TryGetService(Type serviceType, out object service);
        bool TryGetService<T>(out T service);
    }
}