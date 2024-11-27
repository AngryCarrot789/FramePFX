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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.History;
using FramePFX.RBC;

namespace FramePFX.Editors.Commands
{
    public class DeleteSelectedClipsCommand : Command
    {
        public override Executability CanExecute(CommandEventArgs e)
        {
            return ClipContextRegistry.CanGetClipSelection(e.ContextData);
        }

        protected override void Execute(CommandEventArgs e)
        {
            if (!ClipContextRegistry.GetClipSelection(e.ContextData, out Clip[] clips))
            {
                return;
            }

            foreach (Clip clip in clips)
            {
                clip.Destroy();
                clip.Track.RemoveClip(clip);
            }
        }
    }

    public class MultiClipDeletionHistoryAction : HistoryAction {
        private readonly Dictionary<Track, List<byte[]>> trackIdToClipArray;
        private readonly Timeline timeline;
        private readonly List<Clip> redeletionList;
        
        public MultiClipDeletionHistoryAction(List<Clip> clips, Timeline timeline) {
            this.timeline = timeline;
            this.trackIdToClipArray = new Dictionary<Track, List<byte[]>>();
            this.redeletionList = new List<Clip>();
            foreach (Clip clip in clips) {
                if (!this.trackIdToClipArray.TryGetValue(clip.Track, out List<byte[]> list))
                    this.trackIdToClipArray[clip.Track] = list = new List<byte[]>();

                RBEDictionary dictionary = new RBEDictionary();
                Clip.WriteSerialisedWithId(dictionary, clip);
                list.Add(RBEUtils.ToByteArray(dictionary, Encoding.Default, true, 2048));
            }
        }

        protected override bool OnUndo() {
            foreach (KeyValuePair<Track,List<byte[]>> pair in this.trackIdToClipArray) {
                List<Clip> clips = pair.Value.Select(x => {
                    RBEDictionary dictionary = (RBEDictionary) RBEUtils.FromByteArray(x, Encoding.Default, true);
                    return Clip.ReadSerialisedWithId(dictionary);
                }).ToList();

                foreach (Clip clip in clips) {
                    // Clips are ordered by their timecode position, not list index
                    pair.Key.AddClip(clip);
                }
                
                this.redeletionList.AddRange(clips);
            }

            return true;
        }

        protected override bool OnRedo() {
            foreach (Clip clip in this.redeletionList) {
                clip.Track.RemoveClip(clip);
            }
            
            this.redeletionList.Clear();
            return true;
        }
    }
}