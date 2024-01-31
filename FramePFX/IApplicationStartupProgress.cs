using System.Threading.Tasks;

namespace FramePFX {
    public interface IApplicationStartupProgress {
        Task SetAction(string header, string description);
    }
}