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

using System;
using System.Runtime.InteropServices;

namespace FramePFX.Views {
    /// <summary>
    /// Support for windows dark theme
    /// </summary>
    public class WinDarkTheme {
        [DllImport("ntdll.dll")]
        private static extern uint RtlGetVersion(out OSVERSIONINFOA versionInformation);

        [DllImport("dwmapi.dll")]
        private static extern bool DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute, IntPtr pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        private static extern bool SetWindowCompositionAttribute(IntPtr hwnd, IntPtr pAttrData);

        public static uint OsBuildNumber { get; }

        static WinDarkTheme() {
            RtlGetVersion(out OSVERSIONINFOA info);
            OsBuildNumber = info.BuildNumber;
        }

        public static unsafe void UpdateDarkTheme(IntPtr hWnd, bool dark) {
            uint isDark = dark ? 1u : 0u; // DWORD
            if (OsBuildNumber >= 20161) {
                // this works for build 19045 which is weird...
                DwmSetWindowAttribute(hWnd, 20, (IntPtr) (&isDark), sizeof(uint));
            }
            else if (OsBuildNumber >= 18363) {
                WindowCompositionAttributeData data = new WindowCompositionAttributeData() {
                    Attribute = 26, SizeOfData = sizeof(uint), Data = (IntPtr) (&isDark)
                };

                SetWindowCompositionAttribute(hWnd, (IntPtr) (&data));
            }
            else {
                DwmSetWindowAttribute(hWnd, 19, (IntPtr) (&isDark), sizeof(uint));
            }
        }

        private struct OSVERSIONINFOA {
            public uint OsVersionInfoSize;
            public uint MajorVersion;
            public uint MinorVersion;
            public uint BuildNumber;
            public uint PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string CSDVersion;
        }

        private struct WindowCompositionAttributeData {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }
    }
}