using System.Threading.Tasks;

namespace FramePFX.Interactivity
{
    public interface IFileDropNotifier
    {
        EnumDropType GetFileDropType(string[] paths);

        Task OnFilesDropped(string[] paths, EnumDropType dropType);
    }
}