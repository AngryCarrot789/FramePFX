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

using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Audio;
using FramePFX.Editing.Timelines.Clips.Core;

namespace FramePFX.Editing.Factories;

public class ClipFactory : ReflectiveObjectFactory<Clip> {
    public static ClipFactory Instance { get; } = new ClipFactory();

    private ClipFactory() {
        // no need to register the base class, since you can't
        // create an instance of an abstract class
        // this.RegisterType("clip_vid", typeof(VideoClip));
        base.RegisterType("vc_shape", typeof(VideoClipShape));
        base.RegisterType("vc_image", typeof(ImageVideoClip));
        base.RegisterType("vc_timecode", typeof(TimecodeClip));
        base.RegisterType("vc_text", typeof(TextVideoClip));
        base.RegisterType("vc_comp", typeof(CompositionVideoClip));

        base.RegisterType("ac_dummytest", typeof(AudioClip));
    }

    public Clip NewClip(string id) {
        return base.NewInstance(id);
    }

    public new void RegisterType(string id, Type type) => base.RegisterType(id, type);
}