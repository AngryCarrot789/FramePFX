using System;
using System.Collections.Generic;

namespace FramePFX.Editors.Timelines.Clips {
    /// <summary>
    /// A class used to manage a collection of clips where location changes are synchronised
    /// </summary>
    public class ClipGroup {
        private readonly List<Clip> info;

        public ClipGroup() {
            this.info = new List<Clip>();
        }

        public static ClipGroup CreateOrMergeGroups(List<Clip> newClips) {
            ClipGroup finalGroup = null;
            foreach (Clip clip in newClips) {
                ClipGroup group = Clip.InternalGetGroup(clip);
                if (group != null) {
                    if (finalGroup == null) {
                        finalGroup = group;
                    }
                    else if (finalGroup != group) {
                        group.RemoveClip(clip);
                        finalGroup.AddClip(clip);
                    }
                }
                else {
                    if (finalGroup == null)
                        finalGroup = new ClipGroup();

                    finalGroup.AddClip(clip);
                }
            }

            return finalGroup;
        }

        private void AddClip(Clip clip) {
            if (this.info.Contains(clip))
                throw new Exception("Clip already contained in this group");
            if (Clip.InternalGetGroup(clip) != null)
                throw new Exception("Clip already has a group");
            Clip.InternalSetGroup(clip, this);
            this.info.Add(clip);
        }

        private void RemoveClip(Clip clip) {
            ClipGroup oldgroup = Clip.InternalGetGroup(clip);
            if (oldgroup == null)
                throw new Exception("Clip did not have a group");
            if (oldgroup != this)
                throw new Exception("Clip's group did not match the current instance");
            if (!this.info.Remove(clip))
                throw new Exception("Fatal error: list did not contain clip");
            Clip.InternalSetGroup(clip, null);
        }

        private void MoveClipToGroup(Clip clip, ClipGroup newGroup) {
            ClipGroup oldgroup = Clip.InternalGetGroup(clip);
            oldgroup?.RemoveClip(clip);
            newGroup?.AddClip(clip);
        }

        public static void AddClipToGroup(Clip clip) {

        }

        private class ClipData {

        }
    }
}