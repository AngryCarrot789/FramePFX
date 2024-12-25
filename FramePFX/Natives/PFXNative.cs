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

using System.Runtime.InteropServices;

namespace FramePFX.Natives;

/// <summary>
/// A class which contains all the native methods available through the PFX native composition engine
/// </summary>
public static class PFXNative {
    public const string DllName = "FramePFX.NativeEngine.dll";

    public struct NativeAudioEngineData {
        public IntPtr ManagedAudioEngineCallback;
        public IntPtr AudioEngineStream;
    }

    [DllImport(DllName, EntryPoint = "PFXCE_InitEngine")]
    private static extern int InitEngine();

    [DllImport(DllName, EntryPoint = "PFXCE_ShutdownEngine")]
    private static extern int ShutdownEngine();

    [DllImport(DllName, EntryPoint = "PFXCE_TestEngineSubNumbers")]
    public static extern ulong TestEngineSubNumbers(ulong a, ushort b);

    [DllImport(DllName, EntryPoint = "PFXCE_PixelateVfx")]
    public static extern unsafe int PFXCE_PixelateVfx(uint* pImg, int srcWidth, int srcHeight, int left, int top, int right, int bottom, int blockSize);

    [DllImport(DllName, EntryPoint = "PFXAE_BeginAudioPlayback")]
    public static extern unsafe int PFXAE_BeginAudioPlayback(NativeAudioEngineData* pEngineData);

    [DllImport(DllName, EntryPoint = "PFXAE_EndAudioPlayback")]
    public static extern unsafe int PFXAE_EndAudioPlayback(NativeAudioEngineData* pEngineData);

    public static bool IsInitialised { get; set; }

    public static void InitialiseLibrary() {
        if (InitEngine() != 1)
            throw new Exception("Engine initialisation failed");

        IsInitialised = true;
    }

    public static void ShutdownLibrary() {
        if (IsInitialised) {
            try {
                ShutdownEngine();
            }
            finally {
                IsInitialised = false;
            }
        }
    }
}