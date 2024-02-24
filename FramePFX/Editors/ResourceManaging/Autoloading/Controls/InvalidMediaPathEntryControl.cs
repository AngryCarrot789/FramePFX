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

using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.ResourceManaging.Autoloading.Controls {
    public class InvalidMediaPathEntryControl : InvalidResourceEntryControl {
        private TextBox filePathBox;
        private TextBlock errorMessageBlock;
        private Button confirmButton;

        private readonly GetSetAutoEventPropertyBinder<InvalidMediaPathEntry> filePathBinder = new GetSetAutoEventPropertyBinder<InvalidMediaPathEntry>(TextBox.TextProperty, nameof(InvalidMediaPathEntry.FilePathChanged), b => b.Model.FilePath, (b, v) => b.Model.FilePath = (string) v);

        public InvalidMediaPathEntryControl() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.filePathBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            this.confirmButton = this.GetTemplateChild<Button>("PART_Button");
            this.errorMessageBlock = this.GetTemplateChild<TextBlock>("PART_TextBlockErrMsg");
            this.confirmButton.Click += this.ConfirmClick;
        }

        private void ConfirmClick(object sender, RoutedEventArgs e) {
            if (!this.Entry.TryLoad()) {
                IoC.MessageService.ShowMessage("No such file", "Media file path is still invalid");
            }
        }

        protected override void OnLoaded() {
            this.filePathBinder.Attach(this.filePathBox, (InvalidMediaPathEntry) this.Entry);
            this.errorMessageBlock.Text = ((InvalidMediaPathEntry) this.Entry).ExceptionMessage;
        }

        protected override void OnUnloaded() {
            this.filePathBinder.Detatch();
        }
    }
}