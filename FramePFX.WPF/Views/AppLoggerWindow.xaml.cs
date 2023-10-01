using System;
using System.Windows;
using FramePFX.Logger;

namespace FramePFX.WPF.Views
{
    public partial class AppLoggerWindow : WindowEx
    {
        private bool isEventRegistered;

        public AppLoggerWindow()
        {
            this.DataContext = AppLogger.ViewModel;
            this.InitializeComponent();
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.isEventRegistered)
                return; // ...?
            AppLogger.OnLogEntryBlockPosted += this.AppLoggerOnOnLogEntryBlockPosted;
            this.isEventRegistered = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.isEventRegistered)
            {
                AppLogger.OnLogEntryBlockPosted -= this.AppLoggerOnOnLogEntryBlockPosted;
                this.isEventRegistered = false;
            }
        }

        private void AppLoggerOnOnLogEntryBlockPosted(object sender, EventArgs e)
        {
            this.MainScrollViewer.ScrollToEnd();
        }
    }
}