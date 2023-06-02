using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FrameControlEx.Core;
using FrameControlEx.Core.Services;

namespace FrameControlEx.Services {
    [Service(typeof(IExplorerService))]
    public class WinExplorerService : IExplorerService {
        public void OpenFileInExplorer(string filePath) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(filePath)) {
                Process.Start("explorer.exe", $"/select, \"{filePath.Replace('/', '\\')}\"");
            }
        }
    }
}