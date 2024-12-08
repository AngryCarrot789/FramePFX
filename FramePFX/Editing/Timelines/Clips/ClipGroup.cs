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

namespace FramePFX.Editing.Timelines.Clips;

/// <summary>
/// A class used to manage a collection of clips where location changes are synchronised
/// </summary>
public class ClipGroup
{
    private readonly List<Clip> info;

    public ClipGroup()
    {
        this.info = new List<Clip>();
    }

    // Null if no clips in list
    public static ClipGroup? CreateOrMergeGroups(List<Clip> newClips)
    {
        ClipGroup? finalGroup = null;
        foreach (Clip clip in newClips)
        {
            ClipGroup? group = Clip.InternalGetGroup(clip);
            if (group != null)
            {
                if (finalGroup == null)
                {
                    finalGroup = group;
                }
                else if (finalGroup != group)
                {
                    group.RemoveClip(clip);
                    finalGroup.AddClip(clip);
                }
            }
            else
            {
                finalGroup ??= new ClipGroup();
                finalGroup.AddClip(clip);
            }
        }

        return finalGroup;
    }

    private void AddClip(Clip clip)
    {
        if (this.info.Contains(clip))
            throw new Exception("Clip already contained in this group");
        if (Clip.InternalGetGroup(clip) != null)
            throw new Exception("Clip already has a group");
        Clip.InternalSetGroup(clip, this);
        this.info.Add(clip);
    }

    private void RemoveClip(Clip clip)
    {
        ClipGroup? oldgroup = Clip.InternalGetGroup(clip);
        if (oldgroup == null)
            throw new Exception("Clip did not have a group");
        if (oldgroup != this)
            throw new Exception("Clip's group did not match the current instance");
        if (!this.info.Remove(clip))
            throw new Exception("Fatal error: list did not contain clip");
        Clip.InternalSetGroup(clip, null);
    }

    private void MoveClipToGroup(Clip clip, ClipGroup newGroup)
    {
        ClipGroup? oldgroup = Clip.InternalGetGroup(clip);
        oldgroup?.RemoveClip(clip);
        newGroup?.AddClip(clip);
    }

    public static void AddClipToGroup(Clip clip) {
    }

    private class ClipData {
    }
}