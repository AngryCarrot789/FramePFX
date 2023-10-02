using System;
using System.IO;

namespace FramePFX
{
    public class ResourceLocator
    {
        public static readonly string AppDir;
        public static readonly string ResourceDirectory;

        static ResourceLocator()
        {
            AppDir = Directory.GetCurrentDirectory();
#if DEBUG
            ResourceDirectory = Path.Combine(AppDir, "..\\..\\..\\Assets");
#else
            ResourceDirectory = Path.Combine(AppDir, "Assets");
#endif
        }

        public static void Setup()
        {
            if (!Directory.Exists(AppDir))
            {
                throw new Exception("App launch directory not found");
            }

            if (!Directory.Exists(ResourceDirectory))
            {
                throw new Exception("Resource directory not found");
            }
        }

        public static string GetResourceFile(string path)
        {
            return Path.Combine(ResourceDirectory, path);
        }

        public static string ReadFile(string path)
        {
            string filePath = GetResourceFile(path);
            return File.ReadAllText(filePath);
        }

        public static string[] ReadFileLines(string path)
        {
            string filePath = GetResourceFile(path);
            return File.ReadAllLines(filePath);
        }
    }
}