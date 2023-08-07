using System.Threading.Tasks;

namespace FramePFX.Core.Editor
{
    public interface IProjectSettingsEditor
    {
        Task<ProjectSettings> EditSettingsAsync(ProjectSettings settings);
    }
}