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

using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace FramePFX.Utils;

public static class Filters
{
    public static readonly FileFilter All = FileFilter.Builder("All").Patterns("*.*").AppleUniformTypeIds("public.item").MimeTypes("*/*").Build();
    public static readonly FileFilter TextType = FileFilter.Builder("Text Files").Patterns("*.txt", "*.text").AppleUniformTypeIds("public.plain-text").MimeTypes("text/plain").Build();

    public static readonly FileFilter Png = FileFilter.Builder("PNG Image").Patterns("*.png").AppleUniformTypeIds("public.image").MimeTypes("image/*").Build();
    public static readonly FileFilter Jpg = FileFilter.Builder("JPG Image").Patterns("*.jpg").AppleUniformTypeIds("public.image").MimeTypes("image/*").Build();
    public static readonly FileFilter Jpeg = FileFilter.Builder("JPEG Image").Patterns("*.jpeg").AppleUniformTypeIds("public.image").MimeTypes("image/*").Build();
    public static readonly FileFilter Gif = FileFilter.Builder("GIF Image").Patterns("*.gif").AppleUniformTypeIds("public.image").MimeTypes("image/*").Build();
    public static readonly FileFilter Bmp = FileFilter.Builder("Bitmap Image").Patterns("*.bmp").AppleUniformTypeIds("public.image").MimeTypes("image/*").Build();
    public static readonly FileFilter Webp = FileFilter.Builder("WebP Image").Patterns("*.webp").AppleUniformTypeIds("public.image").MimeTypes("image/*").Build();

    public static readonly FileFilter MediaMp4 = FileFilter.Builder("MP4 Video").Patterns("*.mp4").AppleUniformTypeIds("public.mpeg-4").MimeTypes("video/mp4").Build();
    public static readonly FileFilter MediaM4v = FileFilter.Builder("M4V Video").Patterns("*.m4v").AppleUniformTypeIds("com.apple.m4v-video").MimeTypes("video/x-m4v").Build();
    public static readonly FileFilter MediaMpg = FileFilter.Builder("MPG Video").Patterns("*.mpg").AppleUniformTypeIds("public.mpeg").MimeTypes("video/mpeg").Build();
    public static readonly FileFilter MediaMpeg = FileFilter.Builder("MPEG Video").Patterns("*.mpeg").AppleUniformTypeIds("public.mpeg").MimeTypes("video/mpeg").Build();
    public static readonly FileFilter MediaMp2 = FileFilter.Builder("MP2 Video").Patterns("*.mp2").AppleUniformTypeIds("public.mpeg").MimeTypes("video/mpeg").Build();
    public static readonly FileFilter MediaMov = FileFilter.Builder("MOV Video").Patterns("*.mov").AppleUniformTypeIds("com.apple.quicktime-movie").MimeTypes("video/quicktime").Build();
    public static readonly FileFilter MediaAvi = FileFilter.Builder("AVI Video").Patterns("*.avi").AppleUniformTypeIds("public.avi").MimeTypes("video/x-msvideo").Build();
    public static readonly FileFilter MediaMkv = FileFilter.Builder("MKV Video").Patterns("*.mkv").AppleUniformTypeIds("public.matroska").MimeTypes("video/x-matroska").Build();
    public static readonly FileFilter MediaFlv = FileFilter.Builder("FLV Video").Patterns("*.flv").AppleUniformTypeIds("com.adobe.flash-video").MimeTypes("video/x-flv").Build();
    public static readonly FileFilter MediaGifv = FileFilter.Builder("GIFV Video").Patterns("*.gifv").AppleUniformTypeIds("public.video").MimeTypes("video/mp4").Build();
    public static readonly FileFilter MediaQt = FileFilter.Builder("QT Video").Patterns("*.qt").AppleUniformTypeIds("com.apple.quicktime-movie").MimeTypes("video/quicktime").Build();

    public static readonly FileFilter ProjectType = FileFilter.Builder("Frame PFX Project").Patterns("*.fpx").AppleUniformTypeIds("public.frame-pfx").Build();
    public static readonly IReadOnlyList<FileFilter> ListProjectTypeAndAll = [ProjectType, All];

    public static readonly FileFilter CombinedImageTypes = FileFilter.Combine("All Images", [Png, Jpg, Jpeg, Gif, Bmp, Webp]);
    public static readonly IReadOnlyList<FileFilter> CombinedImageTypesAndAll = [CombinedImageTypes, Png, Jpg, Jpeg, Gif, Bmp, Webp, All];

    public static readonly FileFilter CombinedVideoTypes = FileFilter.Combine("All Videos", [MediaMp4, MediaM4v, MediaMpg, MediaMpeg, MediaMp2, MediaMov, MediaAvi, MediaMkv, MediaFlv, MediaGifv, MediaQt]);
    public static readonly IReadOnlyList<FileFilter> CombinedVideoTypesAndAll = [CombinedVideoTypes, MediaMp4, MediaM4v, MediaMpg, MediaMpeg, MediaMp2, MediaMov, MediaAvi, MediaMkv, MediaFlv, MediaGifv, MediaQt, All];
}

public class FileFilter
{
    /// <summary>
    /// File type name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// List of extensions in GLOB format. I.e. "*.png" or "*.*"
    /// </summary>
    /// <remarks>
    /// Used on Windows, Linux and Browser platforms.
    /// </remarks>
    public IReadOnlyList<string>? Patterns { get; init; }

    /// <summary>
    /// List of extensions in MIME format
    /// </summary>
    /// <remarks>
    /// Used on Android, Linux and Browser platforms
    /// </remarks>
    public IReadOnlyList<string>? MimeTypes { get; init; }

    /// <summary>
    /// List of extensions in Apple uniform format
    /// </summary>
    /// <remarks>
    /// Used only on Apple devices
    /// See https://developer.apple.com/documentation/uniformtypeidentifiers/system_declared_uniform_type_identifiers
    /// </remarks>
    public IReadOnlyList<string>? AppleUniformTypeIdentifiers { get; init; }

    public FileFilter(string? name)
    {
        Validate.NotNullOrWhiteSpaces(name);
        this.Name = name;
    }

    public static FileFilterBuilder Builder(string name) => new FileFilterBuilder(name);

    internal IReadOnlyList<string>? TryGetExtensions()
    {
        // Converts random glob pattern to a simple extension name.
        // Path.GetExtension should be sufficient here,
        // Only exception is "*.*proj" patterns that should be filtered as well.
        return this.Patterns?.Select(Path.GetExtension).Where(e => !string.IsNullOrEmpty(e) && !e.Contains('*') && e.StartsWith(".")).Select(e => e!.TrimStart('.')).ToArray()!;
    }

    /// <summary>
    /// Tries to match the path against our filters
    /// </summary>
    /// <param name="path">The file path</param>
    /// <returns>Null if we have no patterns, true if a match was made, false if no matches were made</returns>
    public bool? MatchFilePath(string path)
    {
        if (this.Patterns == null || this.Patterns.Count < 1)
        {
            return null;
        }

        string fileName = Path.GetFileName(path);
        foreach (string filter in this.Patterns)
        {
            if (IsMatch(fileName, filter))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsMatch(string fileName, string filter)
    {
        string regexPattern = "^" + Regex.Escape(filter).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase);
    }

    public static FileFilter Combine(string name, IReadOnlyList<FileFilter> filters)
    {
        return new FileFilter(name)
        {
            Patterns = filters.SelectMany(x => x.Patterns ?? ImmutableList<string>.Empty).ToList(),
            MimeTypes = filters.SelectMany(x => x.MimeTypes ?? ImmutableList<string>.Empty).Distinct().ToList(),
            AppleUniformTypeIdentifiers = filters.SelectMany(x => x.AppleUniformTypeIdentifiers ?? ImmutableList<string>.Empty).Distinct().ToList(),
        };
    }
}

public class FileFilterBuilder
{
    private List<string>? myPatterns, myAppleUniformTypeIds, myMimeTypes;
    private readonly string name;

    public FileFilterBuilder(string name)
    {
        Validate.NotNullOrWhiteSpaces(name);
        this.name = name;
    }

    public FileFilterBuilder Patterns(string pattern)
    {
        (this.myPatterns ??= new List<string>()).Add(pattern);
        return this;
    }

    public FileFilterBuilder Patterns(params string[] patterns)
    {
        (this.myPatterns ??= new List<string>()).AddRange(patterns);
        return this;
    }

    public FileFilterBuilder AppleUniformTypeIds(params string[] typeIds)
    {
        (this.myAppleUniformTypeIds ??= new List<string>()).AddRange(typeIds);
        return this;
    }

    public FileFilterBuilder MimeTypes(params string[] mimes)
    {
        (this.myMimeTypes ??= new List<string>()).AddRange(mimes);
        return this;
    }

    public FileFilter Build()
    {
        return new FileFilter(this.name)
        {
            Patterns = this.myPatterns ?? new List<string>(),
            MimeTypes = this.myMimeTypes ?? new List<string>(),
            AppleUniformTypeIdentifiers = this.myAppleUniformTypeIds ?? new List<string>()
        };
    }
}