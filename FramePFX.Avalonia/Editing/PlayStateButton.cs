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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Interactivity;
using FramePFX.CommandSystem;
using FramePFX.Editing;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils.Commands;
using FramePFX.Utils.RDA;

namespace FramePFX.Avalonia.Editing;

public class PlayStateButton : Button
{
    public static readonly StyledProperty<PlayState> PlayStateProperty = AvaloniaProperty.Register<PlayStateButton, PlayState>(nameof(PlayState));
    public static readonly StyledProperty<string?> CommandIdProperty = AvaloniaProperty.Register<PlayStateButton, string?>(nameof(CommandId));

    /// <summary>
    /// Gets or sets the play state that is shown in the UI, e.g. if this value is <see cref="Play"/> then it shows a play arrow.
    /// This is not the play state of the video editor, that would effectively be the opposite of this property
    /// </summary>
    public PlayState PlayState
    {
        get => this.GetValue(PlayStateProperty);
        set => this.SetValue(PlayStateProperty, value);
    }

    public string? CommandId
    {
        get => (string?) this.GetValue(CommandIdProperty);
        set => this.SetValue(CommandIdProperty, value);
    }

    protected VideoEditor? editor;
    private readonly RapidDispatchAction delayedContextChangeUpdater;
    private readonly RelayCommand command;

    public PlayStateButton()
    {
        DataManager.AddInheritedContextChangedHandler(this, this.OnInheritedContextChanged);
        this.delayedContextChangeUpdater = new RapidDispatchAction(this.UpdateForContext, DispatchPriority.Loaded, "UpdateCanExecute");
        this.command = new RelayCommand(() =>
        {
            if (this.CommandId is string cmdId && !string.IsNullOrWhiteSpace(cmdId))
                CommandManager.Instance.TryExecute(cmdId, () => DataManager.GetFullContextData(this));
        }, () =>
        {
            if (this.editor == null || !(this.CommandId is string cmdId) || string.IsNullOrWhiteSpace(cmdId))
                return false;
            return CommandManager.Instance.CanExecute(cmdId, DataManager.GetFullContextData(this)) == Executability.Valid;
        });

        this.Command = this.command;
    }

    static PlayStateButton()
    {
        CommandIdProperty.Changed.AddClassHandler<PlayStateButton>((d, e) => d.UpdateButtonUI());
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.UpdateForContext();
    }

    private void OnInheritedContextChanged(object sender, RoutedEventArgs e)
    {
        this.delayedContextChangeUpdater.InvokeAsync();
    }

    private void UpdateForContext()
    {
        if (this.editor != null)
        {
            this.editor.Playback.PlaybackStateChanged -= this.OnEditorPlayStateChanged;
            this.editor = null;
        }

        IContextData context = DataManager.GetFullContextData(this);
        if (DataKeys.VideoEditorKey.TryGetContext(context, out this.editor))
        {
            this.editor.Playback.PlaybackStateChanged += this.OnEditorPlayStateChanged;
        }

        this.UpdateButtonUI();
    }

    protected virtual void OnEditorPlayStateChanged(PlaybackManager sender, PlayState state, long frame)
    {
        this.UpdateButtonUI();
    }

    protected virtual void UpdateButtonUI() => this.command.RaiseCanExecuteChanged();
}