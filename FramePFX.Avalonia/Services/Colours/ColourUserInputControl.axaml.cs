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

using Avalonia.Controls;
using Avalonia.Media;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Services.Messages.Controls;
using FramePFX.Services.ColourPicking;
using FramePFX.Services.UserInputs;
using SkiaSharp;

namespace FramePFX.Avalonia.Services.Colours;

public partial class ColourUserInputControl : UserControl, IUserInputContent {
    private readonly DataParameterPropertyBinder<ColourUserInputInfo> colourBinder;
    private UserInputDialog myDialog;
    private ColourUserInputInfo myData;

    public ColourUserInputControl() {
        this.InitializeComponent();
        this.colourBinder = new DataParameterPropertyBinder<ColourUserInputInfo>(ColorView.ColorProperty, ColourUserInputInfo.ColourParameter, arg => {
            SKColor c = (SKColor) arg!;
            return new Color(c.Alpha, c.Red, c.Green, c.Blue);
        }, arg => {
            Color c = (Color) arg!;
            return new SKColor(c.R, c.G, c.B, c.A);
        });
        
        this.colourBinder.AttachControl(this.PART_ColorView);
    }

    public void Connect(UserInputDialog dialog, UserInputInfo info) {
        this.myDialog = dialog;
        this.myData = (ColourUserInputInfo) info;
        this.colourBinder.AttachModel(this.myData);
    }

    public void Disconnect() {
        this.colourBinder.DetachModel();
        this.myDialog = null;
        this.myData = null;
    }
    
    public bool FocusPrimaryInput() {
        return false;
    }
}