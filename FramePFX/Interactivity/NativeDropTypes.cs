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

namespace FramePFX.Interactivity;

public static class NativeDropTypes {
    public static readonly string Text = nameof(Text);
    public static readonly string UnicodeText = nameof(UnicodeText);
    public static readonly string Dib = "DeviceIndependentBitmap";
    public static readonly string Bitmap = nameof(Bitmap);
    public static readonly string EnhancedMetafile = nameof(EnhancedMetafile);
    public static readonly string MetafilePicture = "MetaFilePict";
    public static readonly string SymbolicLink = nameof(SymbolicLink);
    public static readonly string Dif = "DataInterchangeFormat";
    public static readonly string Tiff = "TaggedImageFileFormat";
    public static readonly string OemText = "OEMText";
    public static readonly string Palette = nameof(Palette);
    public static readonly string PenData = nameof(PenData);
    public static readonly string Riff = "RiffAudio";
    public static readonly string WaveAudio = nameof(WaveAudio);
    public static readonly string Files = nameof(Files);
    public static readonly string Locale = nameof(Locale);
    public static readonly string Html = "HTML Format";
    public static readonly string Rtf = "Rich Text Format";
    public static readonly string CommaSeparatedValue = "CSV";
    public static readonly string StringFormat = typeof(string).FullName;
    public static readonly string Serializable = "PersistentObject";
    public static readonly string Xaml = nameof(Xaml);
    public static readonly string XamlPackage = nameof(XamlPackage);
}