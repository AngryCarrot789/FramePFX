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

using FramePFX.Editing;

namespace FramePFX.BaseFrontEnd.Editing;

/// <summary>
/// A button that handles automatically playing and pausing/stopping the playback
/// </summary>
public class TogglePlayStateButtonControl : PlayStateButtonControl {
    public TogglePlayStateButtonControl() {
        this.CommandId = "commands.editor.TogglePlayCommand";
    }

    protected override void UpdateButtonUI() {
        base.UpdateButtonUI();
        if (this.editor != null) {
            switch (this.editor.Playback.PlayState) {
                case PlayState.Play:
                    // TODO: when editor settings are added and there's an option to allow the toggle action to either pause or stop,
                    this.PlayState = PlayState.Pause;
                break;
                case PlayState.Pause:
                case PlayState.Stop:
                    this.PlayState = PlayState.Play;
                break;
            }
        }
        else {
            this.PlayState = PlayState.Play;
        }
    }
}