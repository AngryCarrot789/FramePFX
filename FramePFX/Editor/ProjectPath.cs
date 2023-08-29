using System;
using FramePFX.RBC;

namespace FramePFX.Editor {
    /// <summary>
    /// A struct for storing a file path along with flags which specify how to handle the file path.
    /// <para>
    /// By default, all file paths are absolute
    /// </para>
    /// </summary>
    public readonly struct ProjectPath {
        public readonly string FilePath;
        public readonly EnumPathFlags Flags;

        public bool IsAbsolute => (this.Flags & EnumPathFlags.AbsoluteFilePath) != 0;

        public ProjectPath(string filePath, EnumPathFlags flags) {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            this.FilePath = filePath;
            this.Flags = flags;
        }

        public string Resolve(Project project) => project.GetAbsolutePath(this.FilePath);

        public static ProjectPath Read(RBEDictionary dictionary) {
            EnumPathFlags flags = (EnumPathFlags) dictionary.GetInt(nameof(Flags));
            string path = dictionary.GetString(nameof(FilePath));
            return new ProjectPath(path, flags);
        }

        public static void Write(ProjectPath path, RBEDictionary dictionary) {
            dictionary.SetString(nameof(FilePath), path.FilePath);
            dictionary.SetInt(nameof(Flags), (int) path.Flags);
        }
    }

    [Flags]
    public enum EnumPathFlags : int {
        None = 0,
        AbsoluteFilePath = 1
    }
}