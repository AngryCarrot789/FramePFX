using System;
using System.Collections.ObjectModel;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels
{
    public class ProjectSettingsViewModel : BaseViewModel, IModifyProject
    {
        public ProjectSettings Model { get; }

        public Rational FrameRate
        {
            get => this.Model.TimeBase;
            set
            {
                this.Model.TimeBase = value;
                this.RaisePropertyChanged();
                this.ProjectModified?.Invoke(this, nameof(this.FrameRate));
            }
        }

        public Resolution Resolution
        {
            get => this.Model.Resolution;
            set
            {
                this.Model.Resolution = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Width));
                this.RaisePropertyChanged(nameof(this.Height));
                this.ProjectModified?.Invoke(this, nameof(this.Resolution));
            }
        }

        public ObservableCollection<string> ChannelFormats { get; }

        public string ChannelFormat
        {
            get => this.Model.ChannelFormat;
            set
            {
                this.Model.ChannelFormat = value;
                this.RaisePropertyChanged();
            }
        }

        public int SampleRate
        {
            get => this.Model.SampleRate;
            set
            {
                this.Model.SampleRate = value;
                this.RaisePropertyChanged();
            }
        }

        public int Width => this.Model.Resolution.Width;
        public int Height => this.Model.Resolution.Height;

        public event ProjectModifiedEvent ProjectModified;

        public ProjectSettingsViewModel(ProjectSettings model)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.ChannelFormats = new ObservableCollection<string>()
            {
                "Stereo"
            };
        }
    }
}