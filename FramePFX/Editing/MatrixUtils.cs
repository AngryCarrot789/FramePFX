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

using System.Numerics;
using SkiaSharp;

namespace FramePFX.Editing;

public static class MatrixUtils {
    public static SKMatrix CreateTransformationMatrix(Vector2 pos, Vector2 scale, double rotation, Vector2 scaleOrigin, Vector2 rotationOrigin) {
        SKMatrix matrix = SKMatrix.Identity;
        matrix = matrix.PreConcat(SKMatrix.CreateTranslation(pos.X, pos.Y));
        matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees((float) rotation, rotationOrigin.X, rotationOrigin.Y));
        matrix = matrix.PreConcat(SKMatrix.CreateScale(scale.X, scale.Y, scaleOrigin.X, scaleOrigin.Y));
        return matrix;
    }

    public static SKMatrix CreateInverseTransformationMatrix(Vector2 pos, Vector2 scale, double rotation, Vector2 scaleOrigin, Vector2 rotationOrigin) {
        SKMatrix matrix = SKMatrix.Identity;
        matrix = matrix.PreConcat(SKMatrix.CreateScale(scale.X == 0.0F ? float.MaxValue : (1 / scale.X), scale.Y == 0.0F ? float.MaxValue : (1 / scale.Y), scaleOrigin.X, scaleOrigin.Y));
        matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees((float) -rotation, rotationOrigin.X, rotationOrigin.Y));
        matrix = matrix.PreConcat(SKMatrix.CreateTranslation(-pos.X, -pos.Y));
        return matrix;
    }
}