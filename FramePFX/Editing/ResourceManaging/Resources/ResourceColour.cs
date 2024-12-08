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

using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editing.ResourceManaging.Resources;

public class ResourceColour : ResourceItem {
    private SKColor myColour;

    public SKColor Colour {
        get => this.myColour;
        set {
            if (this.myColour != value) {
                this.myColour = value;
                this.ColourChanged?.Invoke(this);
            }
        }
    }

    public float ScR {
        get => Maths.Clamp(this.myColour.Red / 255F, 0F, 1F);
        set => this.myColour = this.myColour.WithRed((byte) Maths.Clamp((int) (value * 255F), 0, 255));
    }

    public float ScG {
        get => Maths.Clamp(this.myColour.Green / 255F, 0F, 1F);
        set => this.myColour = this.myColour.WithGreen((byte) Maths.Clamp((int) (value * 255F), 0, 255));
    }

    public float ScB {
        get => Maths.Clamp(this.myColour.Blue / 255F, 0F, 1F);
        set => this.myColour = this.myColour.WithBlue((byte) Maths.Clamp((int) (value * 255F), 0, 255));
    }

    public float ScA {
        get => Maths.Clamp(this.myColour.Alpha / 255F, 0F, 1F);
        set => this.myColour = this.myColour.WithAlpha((byte) Maths.Clamp((int) (value * 255F), 0, 255));
    }

    public byte ByteR {
        get => this.myColour.Red;
        set => this.myColour = this.myColour.WithRed(value);
    }

    public byte ByteG {
        get => this.myColour.Green;
        set => this.myColour = this.myColour.WithGreen(value);
    }

    public byte ByteB {
        get => this.myColour.Blue;
        set => this.myColour = this.myColour.WithBlue(value);
    }

    public byte ByteA {
        get => this.myColour.Alpha;
        set => this.myColour = this.myColour.WithAlpha(value);
    }

    public event ResourceEventHandler ColourChanged;

    public ResourceColour() : this(0, 0, 0) {
    }

    public ResourceColour(byte r, byte g, byte b, byte a = 255) {
        this.myColour = new SKColor(r, g, b, a);
        this.TryAutoEnable(null);
    }

    static ResourceColour() {
        SerialisationRegistry.Register<ResourceColour>(0, (resource, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            resource.myColour = new SKColor(data.GetUInt(nameof(resource.myColour)));
        }, (resource, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetUInt(nameof(resource.myColour), (uint) resource.myColour);
        });
    }

    protected override void LoadDataIntoClone(BaseResource clone) {
        base.LoadDataIntoClone(clone);
        ((ResourceColour) clone).myColour = this.myColour;
    }
}