using System.Collections.Generic;

namespace FramePFX {
    public class PerformanceMonitor {
        private static readonly Dictionary<string, MonitorSection> sections;

        public static void BeginSection(string section) {
        }

        private struct MonitorSection {
            public readonly string name;
            public readonly List<long> times;
            public long median;

            public MonitorSection(string name) : this() {
                this.name = name;
            }
        }
    }
}