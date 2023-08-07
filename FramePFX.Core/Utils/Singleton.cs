namespace FramePFX.Core.Utils
{
    public class Singleton<T> where T : class, new()
    {
        private static T instance;

        public static T Instance { get => instance ?? (instance = new T()); }
    }
}