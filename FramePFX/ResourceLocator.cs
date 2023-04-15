using System.IO;

namespace FramePFX {
    public static class ResourceLocator {
        public static readonly string ProjectDirectory;
        public static readonly string ShaderFolderPath;

        static ResourceLocator() {
            ProjectDirectory = FindFile("FramePFX.sln", false, true, 3) ?? Directory.GetCurrentDirectory();
            ShaderFolderPath = Path.Combine(ProjectDirectory, "FramePFX\\Resources\\Shaders");
        }

        private static string FindFile(string name, bool dir, bool file, int maxDepth = 5) {
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory());
            int depth = 0;
            while (folder != null && depth < maxDepth && folder.Exists) {
                if (dir && folder.Name == name) {
                    return folder.FullName;
                }

                foreach (FileSystemInfo info in folder.EnumerateFileSystemInfos()) {
                    if (info is FileInfo) {
                        if (file && info.Name == name) {
                            return info.FullName;
                        }
                    }
                    else if (info is DirectoryInfo) {
                        if (dir && info.Name == name) {
                            return info.FullName;
                        }
                    }
                }

                depth++;
                folder = folder.Parent;
            }

            return null;
        }
    }
}