using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FramePFX.Core.Editor.ViewModels.Timelines
{
    /// <summary>
    /// A helper for grouping together clips
    /// </summary>
    public class ClipGroupViewModel : BaseViewModel
    {
        public ObservableCollection<ClipViewModel> Clips { get; }

        public ClipGroupViewModel(IEnumerable<ClipViewModel> set)
        {
            this.Clips = new ObservableCollection<ClipViewModel>(set);
        }

        /// <summary>
        /// Resolves all connected clips
        /// </summary>
        /// <returns></returns>
        public ClipGroupViewModel ResolveAll()
        {
            HashSet<ClipViewModel> set = new HashSet<ClipViewModel>();
            foreach (ClipViewModel clip in this.Clips)
            {
                foreach (ClipGroupViewModel group in clip.ConnectedGroups)
                {
                    if (group != this)
                    {
                        group.Resolve(set);
                    }
                }
            }

            return new ClipGroupViewModel(set);
        }

        private void Resolve(HashSet<ClipViewModel> clips)
        {
            foreach (ClipViewModel clip in this.Clips)
            {
                if (!clips.Contains(clip))
                {
                    clips.Add(clip);
                    foreach (ClipGroupViewModel group in clip.ConnectedGroups)
                    {
                        if (group != this)
                        {
                            group.Resolve(clips);
                        }
                    }
                }
            }
        }
    }
}