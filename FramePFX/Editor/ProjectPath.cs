using System;
using System.IO;
using FramePFX.RBC;

namespace FramePFX.Editor {
    /// <summary>
    /// A struct for storing a file path along with flags which specify how to handle the file path.
    /// <para>
    /// By default, all file paths are absolute
    /// </para>
    /// </summary>
    public readonly struct ProjectPath {
        public const string UriPrefix = "pfx";
        public const string UriPrefixAndSeparator = UriPrefix + "://";
        public static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        public readonly string FilePath;
        public readonly EnumPathFlags Flags;

        public bool IsAbsolute => (this.Flags & EnumPathFlags.Absolute) != 0;

        public ProjectPath(string filePath, EnumPathFlags flags) {
            ValidateEnum(flags);
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            this.FilePath = filePath;
            this.Flags = flags;
        }

        public static bool HasInvalidChars(string input, out int index) => (index = input.IndexOfAny(InvalidPathChars)) >= 0;
        public static bool HasInvalidChars(string input) => input.IndexOfAny(InvalidPathChars) >= 0;

        public static void CheckPathIsValid(string path) {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Input path cannot be null, empty or whitespaces");
            if (HasInvalidChars(path, out int index)) {
                throw new Exception($"Invalid character at index {index}: '{path[index]}'");
            }
        }

        /// <summary>
        /// Tries to parse the input string as a project-relative path or an absolute file path and returns
        /// true. Returns false if the input contains invalid characters or is otherwise cannot denote a file path
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="path">Output project path</param>
        /// <returns>See above</returns>
        public static bool TryParse(string input, out ProjectPath path) {
            try {
                path = Parse(input);
                return true;
            }
            catch {
                path = default;
                return false;
            }
        }

        public static ProjectPath Parse(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                throw new ArgumentException("Input cannot be null, empty or only whitespaces");
            }

            if (input.StartsWith(UriPrefixAndSeparator)) {
                string filePath = input.Substring(UriPrefixAndSeparator.Length);
                CheckPathIsValid(filePath);
                return new ProjectPath(filePath, EnumPathFlags.Relative);
            }
            else {
                string filePath;
                try {
                    filePath = Path.GetFullPath(input);
                }
                catch (Exception e) {
                    throw new Exception("Invalid input value", e);
                }

                return new ProjectPath(filePath, EnumPathFlags.Absolute);
            }
        }

        public string Resolve(Project project) => project.GetFilePath(this);

        public static ProjectPath ReadFromRBE(RBEDictionary dictionary) {
            EnumPathFlags flags = (EnumPathFlags) dictionary.GetInt(nameof(Flags));
            ValidateEnum(flags);
            string path = dictionary.GetString(nameof(FilePath));
            if (string.IsNullOrEmpty(path))
                throw new Exception("Data contained an empty string for the file path");
            return new ProjectPath(path, flags);
        }

        public void WriteToRBE(RBEDictionary dictionary) {
            dictionary.SetString(nameof(this.FilePath), this.FilePath);
            dictionary.SetInt(nameof(this.Flags), (int) this.Flags);
        }

        private static void ValidateEnum(EnumPathFlags value) {
            int i32 = (int) value;
            if (i32 < 0 || i32 > 2) {
                throw new Exception("Invalid project path flags: " + value);
            }
        }
    }

    [Flags]
    public enum EnumPathFlags {
        /// <summary>
        /// The project path is invalid/empty
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// The path is relative to the project data folder
        /// </summary>
        Relative = 1,
        /// <summary>
        /// The path is absolute relative to the system file system
        /// </summary>
        Absolute = 2
    }
}