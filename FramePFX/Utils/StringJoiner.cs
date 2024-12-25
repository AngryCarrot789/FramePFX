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

using System.Text;

namespace FramePFX.Utils;

public class StringJoiner {
    private readonly StringBuilder sb;
    private readonly string delimiter;
    private bool hasFirst;

    public StringJoiner(string delimiter) {
        this.sb = new StringBuilder();
        this.delimiter = delimiter;
    }

    public void Append(string value) {
        if (this.hasFirst) {
            this.sb.Append(this.delimiter);
        }
        else {
            this.hasFirst = true;
        }

        this.sb.Append(value);
    }

    public override string ToString() {
        return this.sb.ToString();
    }
}