using System.Threading.Tasks;

namespace FramePFX.Core.Editor {
    public interface IProjectSettingsEditor {
        Task<ProjectSettingsModel> EditSettingsAsync(ProjectSettingsModel settings);
    }
}