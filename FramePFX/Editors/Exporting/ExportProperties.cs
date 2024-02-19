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

using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Exporting {
    public delegate void ExportPropertiesEventHandler(ExportProperties sender);

    public class ExportProperties {
        private FrameSpan span;
        private string filePath;

        /// <summary>
        /// Gets the timeline-relative span that will be exported. Usually spans from 0 to <see cref="Timeline.LargestFrameInUse"/>
        /// </summary>
        public FrameSpan Span {
            get => this.span;
            set {
                if (this.span == value)
                    return;
                this.span = value;
                this.SpanChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets the file path that the user wants to export the file to
        /// </summary>
        public string FilePath {
            get => this.filePath;
            set {
                if (this.filePath == value)
                    return;
                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public event ExportPropertiesEventHandler SpanChanged;
        public event ExportPropertiesEventHandler FilePathChanged;

        public ExportProperties() {
        }
    }
}