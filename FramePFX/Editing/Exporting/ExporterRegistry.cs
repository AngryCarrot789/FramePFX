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

using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.Exporting.FFmpeg;
using FramePFX.Utils;

namespace FramePFX.Editing.Exporting;

/// <summary>
/// A registry of exporters
/// </summary>
public class ExporterRegistry {
    public static ExporterRegistry Instance { get; } = new ExporterRegistry();

    private readonly Dictionary<ExporterKey, BaseExporterInfo> exporters;
    private readonly List<(ExporterKey, BaseExporterInfo)> exporterList;

    /// <summary>
    /// Gets the un-ordered dictionary of registered exporters by key
    /// </summary>
    public IReadOnlyDictionary<ExporterKey, BaseExporterInfo> Exporters => this.exporters;

    /// <summary>
    /// Gets an ordered enumerable of keys, ordered by the registration order
    /// </summary>
    public IEnumerable<ExporterKey> Keys => this.exporterList.Select(x => x.Item1);

    private ExporterRegistry() {
        this.exporters = new Dictionary<ExporterKey, BaseExporterInfo>(ExporterKey.DefaultComparer);
        this.exporterList = new List<(ExporterKey, BaseExporterInfo)>();
    }

    static ExporterRegistry() {
        // Standard exporters
        Instance.RegisterExporter(new ExporterKey("exporter_ffmpeg", "FFmpeg"), new FFmpegExporterInfo());
    }

    public void RegisterExporter(ExporterKey key, BaseExporterInfo exporter) {
        Validate.NotNull(exporter);
        if (!this.exporters.TryAdd(key, exporter))
            throw new InvalidOperationException("Key already registered: " + key.ToString());
        this.exporterList.Add((key, exporter));
        BaseExporterInfo.InternalSetKey(exporter, key);
    }

    public bool TryGetExporter(ExporterKey key, [NotNullWhen(true)] out BaseExporterInfo? exporter) => this.exporters.TryGetValue(key, out exporter);
}