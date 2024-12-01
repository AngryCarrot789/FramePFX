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

using FramePFX.CommandSystem;
using FramePFX.Editing.Factories;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public abstract class AddClipCommand<T> : AsyncCommand where T : Clip {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        return DataKeys.TrackKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track? track)) {
            return;
        }

        FrameSpan span = new FrameSpan(0, 300);
        if (DataKeys.TrackContextMouseFrameKey.TryGetContext(e.ContextData, out long frame)) {
            span = span.WithBegin(frame);
        }

        T clip = this.NewInstance();
        clip.FrameSpan = span;
        await this.OnPreAddToTrack(track, clip);
        track.AddClip(clip);
        await this.OnPostAddToTrack(track, clip);
    }

    protected virtual bool IsAllowedInTrack(Track track, T clip) {
        return track.IsClipTypeAccepted(clip.GetType());
    }

    protected virtual T NewInstance() {
        return (T) ClipFactory.Instance.NewClip(ClipFactory.Instance.GetId(typeof(T)));
    }
    
    protected virtual Task OnPreAddToTrack(Track track, T clip) {
        return Task.CompletedTask;
    }
    
    protected virtual Task OnPostAddToTrack(Track track, T clip) {
        return Task.CompletedTask;
    }
}

public class AddTextClipCommand : AddClipCommand<TextVideoClip>;
public class AddTimecodeClipCommand : AddClipCommand<TimecodeClip>;
public class AddVideoClipShapeCommand : AddClipCommand<VideoClipShape>;
public class AddImageVideoClipCommand : AddClipCommand<ImageVideoClip>;
public class AddCompositionVideoClipCommand : AddClipCommand<CompositionVideoClip>;