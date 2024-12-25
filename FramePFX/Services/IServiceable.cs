namespace FramePFX.Services;

/// <summary>
/// An interface for an object that contains services via a service manager
/// </summary>
public interface IServiceable
{
    /// <summary>
    /// Gets our service manager
    /// </summary>
    ServiceManager ServiceManager { get; }
}