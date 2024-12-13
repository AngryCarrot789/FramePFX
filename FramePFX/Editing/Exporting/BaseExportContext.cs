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

using FramePFX.Editing.Timelines;
using FramePFX.Utils;

namespace FramePFX.Editing.Exporting;

/// <summary>
/// A specific exportation context for an exporter
/// </summary>
public abstract class BaseExportContext
{
    /// <summary>
    /// The export registration that created this context
    /// </summary>
    public BaseExporterInfo Exporter { get; }

    /// <summary>
    /// Gets the export setup
    /// </summary>
    public ExportSetup Setup { get; }

    /// <summary>
    /// The editor that our timeline belongs to. This is fetched from our <see cref="Setup"/>
    /// </summary>
    public VideoEditor Editor => this.Setup.Editor;

    /// <summary>
    /// The timeline instance being exported. This is fetched from our <see cref="Setup"/>
    /// </summary>
    public Timeline Timeline => this.Setup.Timeline;

    /// <summary>
    /// The duration of the timeline. A full export spans from 0 to usually the last frame
    /// of the last clip in the entire timeline. This is fetched from our <see cref="Setup"/>
    /// </summary>
    public FrameSpan Span => this.Setup.Span;

    protected BaseExportContext(BaseExporterInfo exporter, ExportSetup setup)
    {
        Validate.NotNull(exporter);
        Validate.NotNull(setup);

        this.Exporter = exporter;
        this.Setup = setup;
    }

    /// <summary>
    /// Runs a timeline export, using the given export properties and also notifying the export progress instance.
    /// <para>
    /// This should typically be run through a call to the task Run static function, as to not freeze
    /// </para>
    /// <para>
    /// The given project should not be modified externally during render
    /// </para>
    /// </summary>
    /// <param name="progress">A helper class for updating the UI of export progress (optionally used, but should be non-null)</param>
    /// <param name="cancellation">Export cancellation token. Check regularly</param>
    public abstract void Export(IExportProgress progress, CancellationToken cancellation);
}