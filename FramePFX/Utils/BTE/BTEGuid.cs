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

namespace FramePFX.Utils.BTE;

public class BTEGuid : BinaryTreeElement {
    public override BTEType Type => BTEType.Guid;

    public Guid Value { get; set; }

    public BTEGuid() {
    }

    public BTEGuid(Guid value) {
        this.Value = value;
    }

    // These are probably ultra slow but faster than writing/reading strings

    protected override void Read(BinaryReader reader) {
        this.Value = new Guid(reader.ReadBytes(16));
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value.ToByteArray());
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEGuid CloneCore() => new BTEGuid(this.Value);
}