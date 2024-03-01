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

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using FramePFX.Utils.Commands;

namespace FramePFX.Editors.Controls {
    public class PlayStateButton : Button {
        public static readonly DependencyProperty PlayStateProperty = DependencyProperty.Register("PlayState", typeof(PlayState), typeof(PlayStateButton), new PropertyMetadata(PlayState.Play));
        public static readonly DependencyProperty CommandIdProperty = DependencyProperty.Register("CommandId", typeof(string), typeof(PlayStateButton), new PropertyMetadata(null, OnCommandIdChanged));

        /// <summary>
        /// Gets or sets the play state that is shown in the UI, e.g. if this value is <see cref="Play"/> then it shows a play arrow.
        /// This is not the play state of the video editor, that would effectively be the opposite of this property
        /// </summary>
        public PlayState PlayState {
            get => (PlayState) this.GetValue(PlayStateProperty);
            set => this.SetValue(PlayStateProperty, value);
        }

        public string CommandId {
            get => (string) this.GetValue(CommandIdProperty);
            set => this.SetValue(CommandIdProperty, value);
        }

        protected VideoEditor editor;
        private readonly RapidDispatchAction delayedContextChangeUpdater;
        private readonly AsyncRelayCommand command;

        public PlayStateButton() {
            DataManager.AddInheritedContextInvalidatedHandler(this, this.OnInheritedContextChanged);
            this.Loaded += this.OnLoaded;
            this.delayedContextChangeUpdater = new RapidDispatchAction(this.UpdateForContext, DispatcherPriority.Loaded, "UpdateCanExecute");
            this.command = new AsyncRelayCommand(() => {
                if (this.CommandId is string cmdId && !string.IsNullOrWhiteSpace(cmdId))
                    return CommandManager.Instance.TryExecute(cmdId, () => DataManager.GetFullContextData(this));
                return Task.CompletedTask;
            }, () => {
                if (this.editor == null || !(this.CommandId is string cmdId) || string.IsNullOrWhiteSpace(cmdId))
                    return false;
                return CommandManager.Instance.CanExecute(cmdId, DataManager.GetFullContextData(this)) == ExecutabilityState.Executable;
            });

            this.Command = this.command;
        }

        static PlayStateButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayStateButton), new FrameworkPropertyMetadata(typeof(PlayStateButton)));
        }

        private static void OnCommandIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((PlayStateButton) d).UpdateButtonUI();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.UpdateForContext();
        }

        private void OnInheritedContextChanged(object sender, RoutedEventArgs e) {
            this.delayedContextChangeUpdater.InvokeAsync();
        }

        private void UpdateForContext() {
            if (this.editor != null) {
                this.editor.Playback.PlaybackStateChanged -= this.OnEditorPlayStateChanged;
                this.editor = null;
            }

            IContextData context = DataManager.GetFullContextData(this);
            if (DataKeys.VideoEditorKey.TryGetContext(context, out this.editor)) {
                this.editor.Playback.PlaybackStateChanged += this.OnEditorPlayStateChanged;
            }

            this.UpdateButtonUI();
        }

        protected virtual void OnEditorPlayStateChanged(PlaybackManager sender, PlayState state, long frame) {
            this.UpdateButtonUI();
        }

        protected virtual void UpdateButtonUI() => this.command.RaiseCanExecuteChanged();
    }
}