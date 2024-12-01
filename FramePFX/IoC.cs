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

using FramePFX.Editing.ResourceManaging;
using FramePFX.Services.ColourPicking;
using FramePFX.Services.FilePicking;
using FramePFX.Services.Messaging;
using FramePFX.Services.UserInputs;
using FramePFX.Tasks;

namespace FramePFX;

public static class IoC {
    /// <summary>
    /// Gets the application's message dialog service, for showing messages to the user
    /// </summary>
    public static IMessageDialogService MessageService => RZApplication.Instance.Services.GetService<IMessageDialogService>();

    /// <summary>
    /// Gets the application's user input dialog service, for querying basic inputs from the user
    /// </summary>
    public static IUserInputDialogService UserInputService => RZApplication.Instance.Services.GetService<IUserInputDialogService>();

    /// <summary>
    /// Gets the application's file picking service, for picking files and directories to open/save
    /// </summary>
    public static IFilePickDialogService FilePickService => RZApplication.Instance.Services.GetService<IFilePickDialogService>();

    public static IColourPickerService ColourPickerService => RZApplication.Instance.Services.GetService<IColourPickerService>();
    
    public static IResourceLoaderService ResourceLoaderService => RZApplication.Instance.Services.GetService<IResourceLoaderService>();

    /// <summary>
    /// Gets the application's task manager
    /// </summary>
    public static TaskManager TaskManager => RZApplication.Instance.Services.GetService<TaskManager>();

    public static IDispatcher Dispatcher => RZApplication.Instance.Dispatcher;
}