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

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace FramePFX.Avalonia;

public partial class AppSplashScreen : Window, IApplicationStartupProgress
{
    public string? CurrentActivity
    {
        get => this.CurrentActivityTextBlock.Text;
        set => this.CurrentActivityTextBlock.Text = value;
    }

    public AppSplashScreen()
    {
        this.InitializeComponent();
    }

    public async Task SetAction(string? header, string? description)
    {
        // AppLogger.Instance.WriteLine(header);
        this.CurrentActivity = header;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);
    }
}