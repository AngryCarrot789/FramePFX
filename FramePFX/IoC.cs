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

using FramePFX.Configurations;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Services.ColourPicking;
using FramePFX.Services.FilePicking;
using FramePFX.Services.InputStrokes;
using FramePFX.Services.Messaging;
using FramePFX.Services.UserInputs;
using FramePFX.Tasks;

namespace FramePFX;

public static class IoC
{
    /// <summary>
    /// Gets the application's message dialog service, for showing messages to the user
    /// </summary>
    public static IMessageDialogService MessageService => Application.Instance.Services.GetService<IMessageDialogService>();

    /// <summary>
    /// Gets the application's user input dialog service, for querying basic inputs from the user
    /// </summary>
    public static IUserInputDialogService UserInputService => Application.Instance.Services.GetService<IUserInputDialogService>();

    /// <summary>
    /// Gets the application's file picking service, for picking files and directories to open/save
    /// </summary>
    public static IFilePickDialogService FilePickService => Application.Instance.Services.GetService<IFilePickDialogService>();

    public static IColourPickerService ColourPickerService => Application.Instance.Services.GetService<IColourPickerService>();

    public static IResourceLoaderService ResourceLoaderService => Application.Instance.Services.GetService<IResourceLoaderService>();
    
    public static IInputStrokeQueryService InputStrokeQueryService => Application.Instance.Services.GetService<IInputStrokeQueryService>();
    /// <summary>
    /// Gets the application's task manager
    /// </summary>
    public static TaskManager TaskManager => Application.Instance.Services.GetService<TaskManager>();
    
    /// <summary>
    /// Gets the application's configuration manager
    /// </summary>
    public static ApplicationConfigurationManager ApplicationConfigurationManager => Application.Instance.Services.GetService<ApplicationConfigurationManager>();
    
    public static IConfigurationService ConfigurationService => Application.Instance.Services.GetService<IConfigurationService>();

    public static IDispatcher Dispatcher => Application.Instance.Dispatcher;
}