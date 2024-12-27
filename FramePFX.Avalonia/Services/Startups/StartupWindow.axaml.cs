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

using Avalonia.Interactivity;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Services.VideoEditors;

namespace FramePFX.Avalonia.Services.Startups;

public partial class StartupWindow : WindowEx {
    private readonly StartupManager startupManager;
    
    public StartupWindow(StartupManager startupManager) {
        this.InitializeComponent();
        this.startupManager = startupManager;
        this.PART_CreateDummyProjectButton.Command = this.startupManager.DoOpenDummyProjectCommand;
        this.PART_OpenEditorWithoutProjectButton.Command = this.startupManager.DoOpenEmptyEditorCommand;
        this.PART_OpenProjectButton.Command = this.startupManager.DoOpenProjectCommand;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.PART_CreateDummyProjectButton.Focus();
    }
}