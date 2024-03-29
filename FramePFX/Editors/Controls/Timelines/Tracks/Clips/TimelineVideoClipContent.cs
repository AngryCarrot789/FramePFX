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

using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips
{
    /// <summary>
    /// A class that extends <see cref="TimelineClipContent"/> to handle additional video clip specific properties (e.g. visibility)
    /// </summary>
    public abstract class TimelineVideoClipContent : TimelineClipContent
    {
        public bool IsClipVisible { get; private set; }

        public new VideoClip Model => (VideoClip) base.Model;

        protected TimelineVideoClipContent()
        {
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            base.Model.TransferableData.AddValueChangedHandler(VideoClip.IsVisibleParameter, this.OnVisibilityParameterChanged);
            this.UpdateClipVisibility();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            base.Model.TransferableData.RemoveValueChangedHandler(VideoClip.IsVisibleParameter, this.OnVisibilityParameterChanged);
        }

        private void OnVisibilityParameterChanged(DataParameter parameter, ITransferableData owner)
        {
            this.UpdateClipVisibility();
        }

        private void UpdateClipVisibility()
        {
            VideoClip clip = this.Model;
            if (clip != null)
            {
                bool value = VideoClip.IsVisibleParameter.GetValue(clip);
                if (this.IsClipVisible != value)
                {
                    this.IsClipVisible = value;
                    this.OnClipVisibilityChanged();
                }
            }
        }

        /// <summary>
        /// Called when our clip model's visibility changes
        /// </summary>
        protected virtual void OnClipVisibilityChanged()
        {
        }
    }
}