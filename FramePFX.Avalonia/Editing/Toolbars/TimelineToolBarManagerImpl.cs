// 
// Copyright (c) 2024-2024 REghZy
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

using Avalonia.Controls;
using Avalonia.Media;
using FramePFX.Editing.Toolbars;

namespace FramePFX.Avalonia.Editing.Toolbars;

public class TimelineToolBarManagerImpl : TimelineToolBarManager {
    public TimelineToolBarManagerImpl() {
        this.AddStandard();
    }

    public void AddStandard() {
        // Add purely custom buttons
        {
            // Toggle play/pause button
            {
                TogglePlayStateButtonControl btn = new TogglePlayStateButtonControl() {
                    Width = 24, Focusable = false, Background = Brushes.Transparent
                };
                
                ToolTip.SetTip(btn, "Play or pause playback");
                this.WestButtons.Add(new PlayStateToolbarButton(btn));
            }
            
            // Toggle play/pause button
            {
                PlayStateButtonControl btn = new PlayStateButtonControl() {
                    Width = 24, Focusable = false 
                };
                
                ToolTip.SetTip(btn, "Play or pause playback");
                this.WestButtons.Add(new PlayStateToolbarButton(btn));
            }
            
            // Toggle play/pause button
            {
                PlayStateButtonControl btn = new PlayStateButtonControl() {
                    Width = 24, Focusable = false 
                };
                
                ToolTip.SetTip(btn, "Play or pause playback");
                this.WestButtons.Add(new PlayStateToolbarButton(btn));
            }
            
            // Toggle play/pause button
            {
                PlayStateButtonControl btn = new PlayStateButtonControl() {
                    Width = 24, Focusable = false 
                };
                
                ToolTip.SetTip(btn, "Play or pause playback");
                this.WestButtons.Add(new PlayStateToolbarButton(btn));
            }
        }

        // this.WestButtons.Add();
    }
}