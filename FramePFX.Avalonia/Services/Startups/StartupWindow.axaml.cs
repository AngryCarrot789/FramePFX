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
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Themes.Controls;
using FramePFX.Services.VideoEditors;

namespace FramePFX.Avalonia.Services.Startups;

public partial class StartupWindow : WindowEx {
    private readonly StartupManager startupManager;
    
    public StartupWindow(StartupManager startupManager) {
        this.InitializeComponent();
        this.startupManager = startupManager;
        this.PART_CreateDemoProjectButton.Command = this.startupManager.DoOpenDemoProjectCommand;
        this.PART_OpenEditorWithoutProjectButton.Command = this.startupManager.DoOpenEmptyEditorCommand;
        this.PART_OpenProjectButton.Command = this.startupManager.DoOpenProjectCommand;
        this.PART_AlwaysUseThisOption.IsCheckedChanged += this.PART_AlwaysUseThisOptionOnIsCheckedChanged;
    }

    private void PART_AlwaysUseThisOptionOnIsCheckedChanged(object? sender, RoutedEventArgs e) {
        this.startupManager.UseSelectedOptionOnStartup = this.PART_AlwaysUseThisOption.IsChecked == true;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.PART_CreateDemoProjectButton.Focus();
    }
}