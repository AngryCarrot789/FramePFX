using System;
using System.Threading.Tasks;
using FramePFX.Components;
using FramePFX.ServiceManaging;

namespace FramePFX.App {
    /// <summary>
    /// Represents the application. This class is mainly useful for accessing services and invoking read/write operations
    /// </summary>
    public interface IApplication : IServiceProviderEx {
        /// <summary>
        /// Whether or not this application is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Returns true if we are on the write thread (aka the main application thread)
        /// </summary>
        bool IsOnMainThread { get; }

        /// <summary>
        /// Gets the application dispatcher. This is used to dispatch method invocations onto the application's main thread (AMT)
        /// </summary>
        IDispatcher Dispatcher { get; }

        /// <summary>
        /// Gets the current version of the application
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Convenience function to throw an exception if not on the main thread
        /// </summary>
        /// <param name="exceptionMessage">The exception message</param>
        void ValidateIsMainThread(string exceptionMessage = "Not on the main thread");
    }
}