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