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
using Fractions;
using FramePFX.Utils.BTE;

namespace FramePFX;

public static class FractionUtils {
    public static void Serialise(this Fraction fraction, string key, BTEDictionary dict) {
        fraction.Numerator.Serialise(key + "_Num", dict);
        fraction.Denominator.Serialise(key + "_Den", dict);
    }

    public static Fraction DeserialiseFraction(BTEDictionary dict, string key) {
        BigInteger num = DeserialiseBigInteger(dict, key + "_Num");
        BigInteger den = DeserialiseBigInteger(dict, key + "_Den");
        return new Fraction(num, den);
    }

    public static void Serialise(this BigInteger integer, string key, BTEDictionary dict) {
        dict.SetByteArray(key, integer.ToByteArray());
    }

    public static BigInteger DeserialiseBigInteger(BTEDictionary dict, string key) {
        byte[] array = dict.GetByteArray(key) ?? throw new Exception("No bytes for big integer keyed by " + key);
        return new BigInteger(array);
    }
}