using System.Collections.Generic;
using System.Linq;
using FramePFX.Utils;

namespace FramePFX.Profiling {
    public sealed class Profiler {
        private readonly Dictionary<string, ProfilerSection> profiledSections;
        private readonly Stack<ProfilerSection> currStack;

        public IEnumerable<KeyValuePair<string, ProfilerSection>> Sections => this.profiledSections;

        public Profiler() {
            this.profiledSections = new Dictionary<string, ProfilerSection>();
            this.currStack = new Stack<ProfilerSection>();
        }

        private string GetCurrentPath() {
            return string.Join(".", this.currStack.Select(x => x.name));
        }

        public void BeginSection(string section) {
            string path = this.GetCurrentPath();
            if (!this.profiledSections.TryGetValue(path, out ProfilerSection ps)) {
                this.profiledSections[path] = ps = new ProfilerSection(section);
            }

            this.currStack.Push(ps);
            ps.Begin();
        }

        public void EndSection() {
            ProfilerSection topItem = this.currStack.Pop();
            topItem.End();
        }

        public void EndAndBeginSection(string section) {
            this.EndSection();
            this.BeginSection(section);
        }

        public sealed class ProfilerSection {
            public readonly string name;
            private long totalTimeSpent;
            private long activeBegin;

            public long TotalTimeSpent => this.totalTimeSpent;

            public ProfilerSection(string name) {
                this.name = name;
            }

            public void Begin() {
                this.activeBegin = Time.GetSystemTicks();
            }

            public void End() {
                long duration = Time.GetSystemTicks() - this.activeBegin;
                this.totalTimeSpent += duration;
            }
        }
    }
}