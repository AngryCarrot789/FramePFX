// 
// Copyright (c) 2026-2026 REghZy
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

using FramePFX.Editing;
using PFXToolKitUI.Utils.Events;

namespace FramePFX;

public sealed class PlaybackManager {
    public VideoEditor VideoEditor { get; }

    public PlayState PlayState {
        get => field;
        private set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.PlayStateChanged);
    }

    public event EventHandler<ValueChangedEventArgs<PlayState>>? PlayStateChanged;

    public PlaybackManager(VideoEditor videoEditor) {
        this.VideoEditor = videoEditor;
    }

    public void Play() {
        this.PlayState = PlayState.Play;
    }
    
    public void Pause() {
        this.PlayState = PlayState.Paused;
    }
    
    public void Stop() {
        this.PlayState = PlayState.Stopped;
    }
}

public enum PlayState {
    Stopped,
    Paused,
    Play
}