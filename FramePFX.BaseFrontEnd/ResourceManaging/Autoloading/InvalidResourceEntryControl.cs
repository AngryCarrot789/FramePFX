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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using Avalonia.Controls.Primitives;
using FramePFX.Editing.ResourceManaging.Autoloading;
using FramePFX.Editing.ResourceManaging.Resources;
using PFXToolKitUI.Avalonia.Utils;

namespace FramePFX.BaseFrontEnd.ResourceManaging.Autoloading;

/// <summary>
/// A control that represents the information present in the resource loader dialog's details panel
/// </summary>
public class InvalidResourceEntryControl : TemplatedControl {
    public static readonly ModelControlRegistry<InvalidResourceEntry, InvalidResourceEntryControl> Registry;

    public InvalidResourceEntry? Entry { get; private set; }

    public InvalidResourceEntryControl() {
    }

    static InvalidResourceEntryControl() {
        Registry = new ModelControlRegistry<InvalidResourceEntry, InvalidResourceEntryControl>();
        Registry.RegisterType<InvalidImagePathEntry>(() => new InvalidImagePathEntryControl());
    }

    public void Attach(InvalidResourceEntry item) {
        this.Entry = item ?? throw new ArgumentNullException(nameof(item));
        this.OnAttached();
    }

    public void Detach() {
        this.OnDetached();
        this.Entry = null;
    }

    protected virtual void OnAttached() {
    }

    protected virtual void OnDetached() {
    }
}