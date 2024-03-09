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

using FramePFX.Editors.ResourceManaging.Autoloading;

namespace FramePFX.Editors.ResourceManaging.Resources
{
    /// <summary>
    /// An entry that represents an invalid image path that does not exist or couldn't represent an image
    /// </summary>
    public class InvalidImagePathEntry : InvalidResourceEntry
    {
        public new ResourceImage Resource => (ResourceImage) base.Resource;

        private string filePath;

        public string FilePath {
            get => this.filePath;
            set
            {
                if (this.filePath == value)
                    return;
                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public event InvalidResourceEntryEventHandler FilePathChanged;

        public InvalidImagePathEntry(ResourceImage resource) : base(resource)
        {
            this.DisplayName = "Invalid image file path";
        }
    }
}