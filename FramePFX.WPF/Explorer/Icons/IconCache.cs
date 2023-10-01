using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media;

namespace FramePFX.WPF.Explorer.Icons
{
    public class IconCache
    {
        private struct TimedUsageImageSource
        {
            private long lastAccess;
            public long LastAccess => this.lastAccess;

            private readonly WeakReference<ImageSource> imageReference;

            public TimedUsageImageSource(ImageSource imageReference)
            {
                this.imageReference = new WeakReference<ImageSource>(imageReference);
                this.lastAccess = SystemTime();
            }

            public bool TryAccess(out ImageSource source)
            {
                if (this.imageReference.TryGetTarget(out source))
                {
                    this.lastAccess = SystemTime();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void SetImage(ImageSource source)
            {
                this.imageReference.SetTarget(source);
                this.lastAccess = SystemTime();
            }

            public bool IsInvalid(long minimumTime)
            {
                return this.lastAccess < minimumTime || !this.imageReference.TryGetTarget(out _);
            }
        }

        private readonly ConcurrentDictionary<string, TimedUsageImageSource> pathMap;

        public IconCache()
        {
            this.pathMap = new ConcurrentDictionary<string, TimedUsageImageSource>();
        }

        public void PutImage(string path, ImageSource source)
        {
            if (this.pathMap.ContainsKey(path))
            {
                this.pathMap[path].SetImage(source);
            }
            else
            {
                this.pathMap[path] = new TimedUsageImageSource(source);
            }
        }

        public bool TryGetImage(string path, out ImageSource source)
        {
            if (this.pathMap.TryGetValue(path, out TimedUsageImageSource reference) && reference.TryAccess(out source))
            {
                return true;
            }
            else
            {
                source = null;
                return false;
            }
        }

        public void Tick()
        {
            if (this.pathMap.Count == 0)
            {
                return;
            }

            long time = SystemTime();
            long minimum = time - 10000;
            HashSet<string> remove = new HashSet<string>();
            foreach (KeyValuePair<string, TimedUsageImageSource> entry in this.pathMap)
            {
                if (entry.Value.IsInvalid(minimum))
                {
                    remove.Add(entry.Key);
                }
            }

            if (remove.Count > 0)
            {
                foreach (string key in remove)
                {
                    this.pathMap.TryRemove(key, out _);
                }
            }
        }

        public static long SystemTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Clear()
        {
            this.pathMap.Clear();
        }
    }
}