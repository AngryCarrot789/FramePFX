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

using FramePFX.DataTransfer;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Editing.Exporting;

public delegate void ExporterInfoCurrentSetupChangedEventHandler(BaseExporterInfo sender, ExportSetup? oldCurrentSetup, ExportSetup? newCurrentSetup);

/// <summary>
/// Contains information about an exporter, and provides a mechanism for creating an exportation context for actually exporting content.
/// <para>
/// Exporters are singletons, but an <see cref="BaseExportContext"/> is created each time the user actually attempts to export content.
/// </para>
/// </summary>
public abstract class BaseExporterInfo : ITransferableData
{
    private ExporterKey myKey;

    public ExporterKey Key => !this.myKey.IsEmpty ? this.myKey : throw new InvalidOperationException("This exporter has not been registered yet");

    /// <summary>
    /// Gets our property editor, which contains all specific information editable by the user to change
    /// how the export takes place. This shouldn't contain the basic information that all exporters have
    /// in common which is destination file path, timeline, editor or the span
    /// </summary>
    public PropertyEditor PropertyEditor { get; }

    public TransferableData TransferableData { get; }

    public ExportSetup? CurrentSetup { get; private set; }

    public event ExporterInfoCurrentSetupChangedEventHandler? CurrentSetupChanged;

    protected BaseExporterInfo()
    {
        this.TransferableData = new TransferableData(this);
        this.PropertyEditor = new PropertyEditor();
    }

    public void OnSelected(ExportSetup setup)
    {
        if (this.CurrentSetup != null)
            throw new InvalidOperationException("Setup already connected");

        ExportSetup? oldCurrentSetup = this.CurrentSetup;
        if (oldCurrentSetup == setup)
            throw new InvalidOperationException("Setup changed to the same value");

        this.PropertyEditor.Root.SetupHierarchyState(new SingletonReadOnlyList<object>(this));
        this.CurrentSetup = setup;
        this.CurrentSetupChanged?.Invoke(this, oldCurrentSetup, setup);
    }

    public void Deselect()
    {
        ExportSetup? old = this.CurrentSetup;
        if (old == null)
            throw new InvalidOperationException("Setup not connected");

        this.CurrentSetup = null;
        this.CurrentSetupChanged?.Invoke(this, old, null);
        this.PropertyEditor.Root.ClearHierarchy();
    }

    /// <summary>
    /// Resets information changed by the user, if necessary. This is
    /// called when the export dialog closes (export cancelled, completed, etc.)
    /// </summary>
    public virtual void Reset() {
    }

    /// <summary>
    /// Creates exporting context for the given timeline with the given span
    /// </summary>
    /// <param name="setup">The setup information</param>
    /// <returns>The exporting context</returns>
    public abstract BaseExportContext CreateContext(ExportSetup setup);

    internal static void InternalSetKey(BaseExporterInfo info, ExporterKey key) => info.myKey = key;
}