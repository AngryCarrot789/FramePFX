using System;
using FramePFX.RBC;

namespace FramePFX.Editor
{
    /// <summary>
    /// A struct for storing a file path along with flags which specify how to handle the file path.
    /// <para>
    /// By default, all file paths are absolute
    /// </para>
    /// </summary>
    public readonly struct ProjectPath
    {
        public readonly string FilePath;
        public readonly EnumPathFlags Flags;

        public bool IsAbsolute => (this.Flags & EnumPathFlags.AbsoluteFilePath) != 0;

        public ProjectPath(string filePath, EnumPathFlags flags)
        {
            ValidateEnum(flags);
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            this.FilePath = filePath;
            this.Flags = flags;
        }

        public string Resolve(Project project) => project.GetFilePath(this);

        public static ProjectPath ReadFromRBE(RBEDictionary dictionary)
        {
            EnumPathFlags flags = (EnumPathFlags) dictionary.GetInt(nameof(Flags));
            ValidateEnum(flags);
            string path = dictionary.GetString(nameof(FilePath));
            if (string.IsNullOrEmpty(path))
                throw new Exception("Data contained an empty string for the file path");
            return new ProjectPath(path, flags);
        }

        public void WriteToRBE(RBEDictionary dictionary)
        {
            dictionary.SetString(nameof(this.FilePath), this.FilePath);
            dictionary.SetInt(nameof(this.Flags), (int) this.Flags);
        }

        private static void ValidateEnum(EnumPathFlags value)
        {
            int i32 = (int) value;
            if (i32 < 0 || i32 > 1)
            {
                throw new Exception("Invalid project path flags: " + value);
            }
        }
    }

    [Flags]
    public enum EnumPathFlags
    {
        None = 0,
        AbsoluteFilePath = 1
    }
}