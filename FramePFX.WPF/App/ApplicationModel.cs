using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using FramePFX.App;
using FramePFX.App.Exceptions;
using FramePFX.Components;
using FramePFX.ServiceManaging;
using FramePFX.WPF.Utils;
using Time = FramePFX.Utils.Time;

namespace FramePFX.WPF.App {
    /// <summary>
    /// The <see cref="IApplication"/> implementation for FramePFX
    /// </summary>
    public class ApplicationModel : IApplication {
        private static readonly long TicksPerQuaterSecond = Time.TICK_PER_SECOND / 4;
        private readonly Dispatcher dispatcher;
        private readonly AppWPF appInstance;
        private readonly ServiceManager services;
        private readonly Thread mainThread; // main app/dispatcher thread

        public bool IsRunning => Application.Current != null;

        public bool IsOnMainThread => Thread.CurrentThread == this.mainThread;

        public IDispatcher Dispatcher { get; }

        public Version Version { get; } = new Version(1, 0, 0, 800);

        public ApplicationModel(AppWPF app) {
            this.appInstance = app ?? throw new ArgumentNullException(nameof(app));
            this.dispatcher = app.Dispatcher ?? throw new Exception("Application dispatcher detached");
            this.mainThread = this.dispatcher.Thread;
            this.Dispatcher = new DispatcherDelegate(this.dispatcher);
            this.services = new ServiceManager();
            this.RegisterService<IDispatcher>(this.Dispatcher);
        }

        public object GetService(Type type) {
            return this.services.GetService(type);
        }

        public bool HasService(Type serviceType) {
            return this.services.HasService(serviceType);
        }

        public bool TryGetService(Type serviceType, out object service) {
            return this.services.TryGetService(serviceType, out service);
        }

        public void RegisterService<T>(T service) {
            this.services.Register(service);
        }

        public void RegisterService(Type type, object service) {
            this.services.Register(type, service);
        }

        public void ValidateIsMainThread(string exceptionMessage) {
            if (!this.IsOnMainThread) {
                throw new WrongThreadException(exceptionMessage);
            }
        }

        public static DispatcherPriority ConvertPriority(DispatchPriority priority) {
            switch (priority) {
                case DispatchPriority.AppIdle: return DispatcherPriority.ApplicationIdle;
                case DispatchPriority.Background: return DispatcherPriority.Background;
                case DispatchPriority.AfterRender: return DispatcherPriority.Render;
                case DispatchPriority.Normal: return DispatcherPriority.Normal;
                case DispatchPriority.Send: return DispatcherPriority.Send;
                default: throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
            }
        }
    }
}