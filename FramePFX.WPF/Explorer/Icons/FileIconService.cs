using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.Explorer.Icons {
    public class FileIconService {
        private volatile bool canFileThreadRun;
        public bool CanFileThreadRun {
            get => this.canFileThreadRun;
            set => this.canFileThreadRun = value;
        }

        private volatile bool canDirectoryThreadRun;
        public bool CanDirectoryThreadRun {
            get => this.canDirectoryThreadRun;
            set => this.canDirectoryThreadRun = value;
        }

        private volatile bool canUpdateTaskRun;
        public bool CanUpdateTaskRun {
            get => this.canUpdateTaskRun;
            set => this.canUpdateTaskRun = value;
        }

        public static FileIconService Instance { get; } = new FileIconService();

        private readonly Thread fileThread;
        private readonly Thread directoryThread;
        private readonly Thread updateThread;
        private readonly ConcurrentQueue<QueuedIconResolution> fileQueue;
        private readonly ConcurrentQueue<QueuedIconResolution> directoryQueue;
        private readonly ConcurrentQueue<PendingIconDelivery> updateQueue;

        private readonly IconCache cache;

        public FileIconService() {
            this.fileQueue = new ConcurrentQueue<QueuedIconResolution>();
            this.directoryQueue = new ConcurrentQueue<QueuedIconResolution>();
            this.updateQueue = new ConcurrentQueue<PendingIconDelivery>();
            this.cache = new IconCache();

            this.canFileThreadRun = true;
            this.canDirectoryThreadRun = true;
            this.canUpdateTaskRun = true;

            this.fileThread = new Thread(this.FileQueueThreadMain) {
                IsBackground = true, Name = "Icon Thread | Files"
            };
            this.directoryThread = new Thread(this.DirectoryQueueThreadMain) {
                IsBackground = true, Name = "Icon Thread | Directories"
            };
            this.updateThread = new Thread(this.UpdateMain) {
                IsBackground = true, Name = "Icon Thread | Update Thread"
            };

            this.fileThread.Start();
            this.directoryThread.Start();
            this.updateThread.Start();

            Task.Run(async () => {
                while (this.canUpdateTaskRun) {
                    this.cache.Tick();
                    await Task.Delay(1000);
                }
            });
        }

        public static void Init() {

        }

        private void UpdateMain() {
            while (this.canUpdateTaskRun) {
                int size = Math.Min(this.updateQueue.Count, 20);
                if (size < 1) {
                    Thread.Sleep(5);
                    continue;
                }

                Application.Current.Dispatcher.Invoke(() => {
                    for (int i = 0; i < size; i++) {
                        if (this.updateQueue.TryDequeue(out PendingIconDelivery pair)) {
                            pair.SetImage();
                            this.cache.PutImage(pair.path, pair.image);
                        }
                        else {
                            break;
                        }
                    }
                });
            }
        }

        private void FileQueueThreadMain() {
            while (this.canFileThreadRun) {
                int count = Math.Min(this.fileQueue.Count, 5);
                if (count <= 0) {
                    Thread.Sleep(5);
                    continue;
                }

                Application.Current.Dispatcher.Invoke(() => {
                    for (int i = 0; i < count; i++) {
                        if (this.fileQueue.TryDequeue(out QueuedIconResolution control)) {
                            string path = control.path;
                            if (File.Exists(path)) {
                                this.updateQueue.Enqueue(new PendingIconDelivery(path, control.imageable, ShellUtils.GetFileIconAsBitmapSource(path, control.iconType, false)));
                            }
                        }
                    }
                });
            }
        }

        private void DirectoryQueueThreadMain() {
            while (this.canDirectoryThreadRun) {
                int count = Math.Min(this.directoryQueue.Count, 5);
                if (count <= 0) {
                    Thread.Sleep(5);
                    continue;
                }

                Application.Current.Dispatcher.Invoke(() => {
                    for (int i = 0; i < count; i++) {
                        if (this.directoryQueue.TryDequeue(out QueuedIconResolution control)) {
                            string path = control.path;
                            if (Directory.Exists(path)) {
                                BitmapSource source;
                                if (control.iconType == ShellIconSize.Large) {
                                    source = ShellEx.GetBitmapSourceForPath(path, false, true);
                                }
                                else {
                                    source = ShellUtils.GetFileIconAsBitmapSource(path, control.iconType, true);
                                }

                                this.updateQueue.Enqueue(new PendingIconDelivery(path, control.imageable, source));
                            }
                        }
                    }
                });
            }
        }

        public void EnqueueForIconResolution(string path, IImageable control, bool forceFile = false, bool forceDirectory = false, ShellIconSize iconType = ShellIconSize.Normal) {
            if (forceFile || forceDirectory) {
                if (forceFile) {
                    this.fileQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
                else {
                    this.directoryQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
            }
            else if (!string.IsNullOrEmpty(path)) {
                if (this.cache.TryGetImage(path, out ImageSource source)) {
                    this.updateQueue.Enqueue(new PendingIconDelivery(path, control, source));
                }
                else if (File.Exists(path)) {
                    this.fileQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
                else if (Directory.Exists(path)) {
                    this.directoryQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
            }
        }

        private readonly struct QueuedIconResolution {
            public readonly string path;
            public readonly IImageable imageable;
            public readonly ShellIconSize iconType;

            public QueuedIconResolution(string path, IImageable imageable, ShellIconSize iconType) {
                this.path = path;
                this.imageable = imageable;
                this.iconType = iconType;
            }
        }

        private readonly struct PendingIconDelivery {
            public readonly IImageable imageable;
            public readonly string path;
            public readonly ImageSource image;

            public PendingIconDelivery(string path, IImageable imageable, ImageSource image) {
                this.path = path;
                this.imageable = imageable;
                this.image = image;
            }

            public void SetImage() {
                this.imageable.Source = this.image;
            }
        }
    }
}