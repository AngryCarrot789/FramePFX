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

using Avalonia.Controls.Primitives;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Configurations.Pages;

/// <summary>
/// The base class for a page control
/// </summary>
public class BaseConfigurationPageControl : TemplatedControl {
    public ConfigurationPage? Page { get; private set; }

    public void Connect(ConfigurationPage page) {
        Validate.NotNull(page);
        if (this.Page != null)
            throw new InvalidOperationException("Already connected");

        this.Page = page;
        this.OnConnected();
    }

    public void Disconnect() {
        if (this.Page == null)
            throw new InvalidOperationException("Not connected");

        this.OnDisconnected();
        this.Page = null;
    }

    public virtual void OnConnected() {
    }

    public virtual void OnDisconnected() {
    }
}