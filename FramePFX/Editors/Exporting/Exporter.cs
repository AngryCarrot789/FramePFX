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

using System.Threading;

namespace FramePFX.Editors.Exporting {
    public abstract class Exporter {
        public string DisplayName { get; }

        protected Exporter(string displayName) {
            this.DisplayName = displayName ?? this.GetType().Name;
        }

        /// <summary>
        /// Runs a complete project export, using the given export properties and also notifying the export progress instance.
        /// <para>
        /// This should typically be run through a call to the task Run static function, as to not freeze
        /// </para>
        /// <para>
        /// The given project should not be modified externally during render
        /// </para>
        /// </summary>
        /// <param name="setup">The export setup</param>
        /// <param name="progress">A helper class for updating the UI of export progress (optionally used, but should be non-null)</param>
        /// <param name="properties">Specific export properties that aren't necessarily related to the project itself</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public abstract void Export(ExportSetup setup, IExportProgress progress, ExportProperties properties, CancellationToken cancellation);

        public abstract void LoadProjectDefaults(Project project);
    }
}