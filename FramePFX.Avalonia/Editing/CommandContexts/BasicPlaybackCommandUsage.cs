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
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.CommandContexts;

public class BasicPlaybackCommandUsage : CommandSourceCommandUsage {
    private VideoEditor editor;

    public BasicPlaybackCommandUsage(string cmdId) : base(cmdId) {
    }

    private void OnEditorPlayStateChanged(PlaybackManager sender, PlayState state, long frame) {
        this.UpdateCanExecute();
    }

    protected override void OnContextChanged() {
        base.OnContextChanged();
        if (this.editor != null) {
            this.editor.Playback.PlaybackStateChanged -= this.OnEditorPlayStateChanged;
            this.editor = null;
        }

        if (DataKeys.VideoEditorKey.TryGetContext(this.GetContextData(), out this.editor)) {
            this.editor.Playback.PlaybackStateChanged += this.OnEditorPlayStateChanged;
        }
    }
}

public class PlayCommandUsage : BasicPlaybackCommandUsage {
    public PlayCommandUsage() : base("commands.editor.PlaybackPlayCommand") {
    }
}

public class PauseCommandUsage : BasicPlaybackCommandUsage {
    public PauseCommandUsage() : base("commands.editor.PlaybackPauseCommand") {
    }
}

public class StopCommandUsage : BasicPlaybackCommandUsage {
    public StopCommandUsage() : base("commands.editor.PlaybackStopCommand") {
    }
}