// 
// Copyright (c) 2024-2026 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FFmpeg.AutoGen;
using FramePFX.Avalonia.Editor;
using FramePFX.BaseFrontEnd.Themes;
using FramePFX.Editing;
using FramePFX.Editing.Audio;
using FramePFX.Editing.Video;
using FramePFX.FFmpeg;
using FramePFX.NAudio;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Avalonia.Themes;
using PFXToolKitUI;
using PFXToolKitUI.Activities;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop.Impl;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace FramePFX.Avalonia;

public class FramePFXApplication : AvaloniaApplicationPFX {
    // TODO: fixshit

    public FramePFXApplication(Application app) : base(app) {
    }

    protected override void RegisterComponents(ComponentStorage manager) {
        if (this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime) {
            manager.AddComponent<IDesktopService>(new DesktopServiceImpl(this.Application));

            DesktopWindowManager dwm = new DesktopWindowManager(new Uri("avares://FramePFX.Avalonia/FramePFX-256.ico", UriKind.RelativeOrAbsolute));
            manager.AddComponent<IWindowManager>(dwm);
            manager.AddComponent<ITopLevelManager>(dwm);
            manager.AddComponent<IForegroundActivityService>(new DesktopForegroundActivityServiceImpl());
        }

        base.RegisterComponents(manager);
        manager.AddComponent<IIconPreferences>(new IconPreferencesImpl());
    }

    private class IconPreferencesImpl : IIconPreferences {
        public bool UseAntiAliasing { get; set; }
    }

    protected override async Task OnSetupApplication(IApplicationStartupProgress progress) {
        await base.OnSetupApplication(progress);

        FramePFXBrushLoader.Init();

        if (OperatingSystem.IsWindows()) {
            this.PluginLoader.AddCorePlugin(typeof(NAudioPlugin));
        }

        await progress.ProgressAndWaitForRender("Loading FFmpeg...", 0.75);

        bool success = false;
        try {
            ffmpeg.avdevice_register_all();
            success = true;
        }
        catch (Exception e) {
            await IMessageDialogService.Instance.ShowMessage("FFmpeg registration failed", "Failed to register all FFmpeg devices. Maybe check FFmpeg is installed? Media clips are now unavailable", e.GetToString());
        }

        if (success) {
            this.PluginLoader.AddCorePlugin(typeof(FFmpegPlugin));
        }

        this.ComponentStorage.AddComponent<IStartupManager>(new FramePFXStartupManager());
    }

    protected override void RegisterConfigurations() {
        base.RegisterConfigurations();

        PersistentStorageManager psm = this.PersistentStorageManager;

        psm.Register<ThemeConfigurationOptions>(new ThemeConfigurationOptionsImpl(), "themes", "themes");
    }

    protected override Task OnApplicationFullyLoaded() {
        // TODO: fixshit

        // Register controls
        // ResourceExplorerListItemContent.Registry.RegisterType<ResourceFolder>(() => new RELIC_Folder());
        // ResourceExplorerListItemContent.Registry.RegisterType<ResourceColour>(() => new RELIC_Colour());
        // ResourceExplorerListItemContent.Registry.RegisterType<ResourceImage>(() => new RELIC_Image());
        // ResourceExplorerListItemContent.Registry.RegisterType<ResourceComposition>(() => new RELIC_Composition());
        //
        // ConfigurationPageRegistry.Registry.RegisterType<EditorWindowConfigurationPage>(() => new BasicEditorWindowConfigurationPageControl());
        //
        // BasePropertyEditorSlotControl.Registry.RegisterType<DisplayNamePropertyEditorSlot>(() => new DisplayNamePropertyEditorSlotControl());
        // BasePropertyEditorSlotControl.Registry.RegisterType<VideoClipMediaFrameOffsetPropertyEditorSlot>(() => new VideoClipMediaFrameOffsetPropertyEditorSlotControl());
        // BasePropertyEditorSlotControl.Registry.RegisterType<TimecodeFontFamilyPropertyEditorSlot>(() => new TimecodeFontFamilyPropertyEditorSlotControl());
        //
        // // automation parameter editors
        // BasePropertyEditorSlotControl.Registry.RegisterType<ParameterFloatPropertyEditorSlot>(() => new ParameterFloatPropertyEditorSlotControl());
        // BasePropertyEditorSlotControl.Registry.RegisterType<ParameterDoublePropertyEditorSlot>(() => new ParameterDoublePropertyEditorSlotControl());
        // BasePropertyEditorSlotControl.Registry.RegisterType<ParameterLongPropertyEditorSlot>(() => new ParameterLongPropertyEditorSlotControl());
        // BasePropertyEditorSlotControl.Registry.RegisterType<ParameterVector2PropertyEditorSlot>(() => new ParameterVector2PropertyEditorSlotControl());
        // BasePropertyEditorSlotControl.Registry.RegisterType<ParameterBoolPropertyEditorSlot>(() => new ParameterBoolPropertyEditorSlotControl());

        return Task.CompletedTask;
    }

    protected override async Task OnApplicationRunning(IApplicationStartupProgress progress, string[] envArgs) {
        if (this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            (progress as AppSplashScreen)?.Close();
            await progress.ProgressAndWaitForRender("Startup completed", 1.0);
            await base.OnApplicationRunning(progress, envArgs);
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        else {
            await base.OnApplicationRunning(progress, envArgs);
        }
    }

    protected override void OnExiting(int exitCode) {
        base.OnExiting(exitCode);
    }

    protected override string? GetSolutionFileName() {
        return "FramePFX.sln";
    }

    public override string GetApplicationName() {
        return "FramePFX";
    }
}

public class FramePFXStartupManager : IStartupManager {
    public Task OnApplicationStartupWithArgs(IApplicationStartupProgress progress, string[] args) {
        if (!IWindowManager.TryGetInstance(out IWindowManager? instance)) {
            return Task.CompletedTask;
        }

        VideoTrack vTrack1 = new VideoTrack() { DisplayName = "Vid track 1" };
        VideoTrack vTrack2 = new VideoTrack() { DisplayName = "Vid track 2" };
        AudioTrack aTrack1 = new AudioTrack() { DisplayName = "Audio track 1" };
        
        vTrack1.AddClip(new ShapeVideoClip() {
            Span = ClipSpan.FromDuration(0, TimeSpan.FromSeconds(1)),
            DisplayName = "My clip 1"
        });
        
        vTrack2.AddClip(new ShapeVideoClip() {
            Span = ClipSpan.FromDuration(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)),
            DisplayName = "My clip 2"
        });
        
        aTrack1.AddClip(new BlankAudioClip() {
            Span = ClipSpan.FromDuration(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(4)),
            DisplayName = "My clip 3"
        });

        VideoEditor editor = new VideoEditor();

        Project project = new Project();
        project.MainTimeline.AddTrack(vTrack1);
        project.MainTimeline.AddTrack(vTrack2);
        project.MainTimeline.AddTrack(aTrack1);

        editor.SetProject(project);
        
        IDesktopWindow window = instance.CreateWindow(new WindowBuilder() {
            Title = "FramePFX v2.0.0-beta",
            Content = new EditorView(TopLevelIdentifier.Single("toplevel.videoeditor")) {
                VideoEditor = editor
            },
            BorderBrush = BrushManager.Instance.GetDynamicThemeBrush("PanelBorderBrush")
        });

        _ = window.ShowAsync();

        return Task.CompletedTask;
    }

    // public async Task OnApplicationStartupWithArgs(IApplicationStartupProgress progress, string[] args) {
    //     const int sampleRate = 48000;
    //     const int channels = 2;
    //     const int framesPerBlock = sampleRate / 100; // 10ms
    //     const int samplesPerBlock = framesPerBlock * channels;
    //     float[] interleaved = new float[samplesPerBlock];
    //
    //     // Setup audio system
    //     NAudioSystem system = new NAudioSystem(sampleRate, channels);
    //     AudioSystem.OpenAudioSystem(system);
    //     system.BeginPlayback();
    //
    //     // Timeline setup
    //     Timeline timeline = new Timeline {
    //         Tracks = {
    //             new AudioTrack {
    //                 Clips = {
    //                     new AudioClip {
    //                         AudioProducer = new SineAudioProducer(),
    //                         Span = ClipSpan.FromDuration(0, TimeSpan.FromSeconds(1))
    //                     },
    //                     new AudioClip {
    //                         AudioProducer = new SineAudioProducer(),
    //                         Span = ClipSpan.FromDuration(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3))
    //                     }
    //                 }
    //             }
    //         }
    //     };
    //
    //     TimelineAudioProducer producer = new TimelineAudioProducer(timeline);
    //
    //     CancellationTokenSource cts = new CancellationTokenSource(5000); // 5s playback
    //
    //     await Task.Run(async () => {
    //         while (!cts.IsCancellationRequested) {
    //             int produced = 0;
    //
    //             // Fill a block of audio
    //             while (produced < samplesPerBlock) {
    //                 int n = producer.Produce(interleaved.AsSpan(produced, samplesPerBlock - produced));
    //                 if (n == 0) {
    //                     // No audio from timeline → fill with silence
    //                     interleaved.AsSpan(produced, samplesPerBlock - produced).Clear();
    //                     n = samplesPerBlock - produced;
    //                 }
    //
    //                 produced += n;
    //             }
    //
    //             // Enqueue into the AudioSystem
    //             int written = 0;
    //             while (written < samplesPerBlock) {
    //                 int count = system.Enqueue(interleaved.AsSpan(written, samplesPerBlock - written));
    //                 if (count == 0) {
    //                     // Ring buffer full, wait a bit
    //                     await Task.Delay(1);
    //                     continue;
    //                 }
    //
    //                 written += count;
    //             }
    //         }
    //     });
    //
    //     system.StopPlayback();
    //     AudioSystem.CloseAudioSystem();
    //
    //     ApplicationPFX.Instance.Shutdown();
    // }
}

// public class TimelineAudioProducer {
//     public Timeline Timeline { get; }
//
//     private long currentTickOffset; // playhead position in ticks
//     private readonly int sampleRate = 48000;
//
//     public TimelineAudioProducer(Timeline timeline) {
//         this.Timeline = timeline;
//     }
//
//     /// <summary>
//     /// Produces up to dstSamples.Length samples starting from currentTickOffset.
//     /// Returns how many samples were written.
//     /// </summary>
//     public int Produce(Span<float> dstSamples) {
//         long offsetTicks = this.currentTickOffset;
//         int sampleCount = dstSamples.Length;
//
//         // Convert sample count to ticks
//         long ticksPerSample = 10_000_000 / this.sampleRate;
//         long startOffsetTicks = offsetTicks;
//         long endOffsetTicks = offsetTicks + sampleCount / 2 * ticksPerSample; // divide by 2 if stereo interleaved
//
//         // Clear the buffer first
//         dstSamples.Clear();
//
//         int writtenSamples = 0;
//
//         foreach (Track track in this.Timeline.Tracks) {
//             if (track is AudioTrack audioTrack) {
//                 foreach (AudioClip clip in audioTrack.Clips.Cast<AudioClip>()) {
//                     if (clip.AudioProducer != null) {
//                         long clipStart = clip.Span.Start;
//                         long clipEnd = clip.Span.End;
//
//                         // Check if clip overlaps current block
//                         long blockEndTicks = startOffsetTicks + dstSamples.Length / 2 * ticksPerSample;
//                         if (clipEnd <= startOffsetTicks || clipStart >= blockEndTicks)
//                             continue; // skip clips outside this block
//
//                         // Compute offset in ticks relative to clip
//                         long clipOffsetTicks = Math.Max(startOffsetTicks - clipStart, 0);
//
//                         // Compute maximum number of samples we can produce from this clip
//                         long maxTicks = clipEnd - Math.Max(startOffsetTicks, clipStart);
//                         int maxSamples = (int) ((maxTicks * this.sampleRate) / 10_000_000) * 2; // *2 for stereo
//
//                         int count = Math.Min(maxSamples, dstSamples.Length);
//
//                         // Call the clip's producer
//                         int produced = clip.AudioProducer.Produce(clipOffsetTicks, dstSamples.Slice(0, count), this.sampleRate);
//
//                         writtenSamples = Math.Max(writtenSamples, produced);
//                     }
//                 }
//             }
//         }
//
//         // Advance playhead
//         this.currentTickOffset += writtenSamples / 2 * ticksPerSample;
//         return writtenSamples;
//     }
// }
//
// public class SineAudioProducer : IAudioProducer {
//     private readonly int channels = 2;
//
//     public int Frequency {
//         get => field;
//         set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.FrequencyChanged);
//     } = 440;
//
//     public event EventHandler? FrequencyChanged;
//
//     public int Produce(long offset, Span<float> dstSamples, int sampleRate) {
//         for (int i = 0; i < dstSamples.Length * this.channels; i += this.channels) {
//             double t = (offset + i) / (double) sampleRate;
//             float sample = (float) Math.Sin(2 * Math.PI * this.Frequency * t);
//             dstSamples[i] = sample;
//             dstSamples[i + 1] = sample;
//         }
//
//         return dstSamples.Length;
//
//         /*
//
//          // Sine-wave phase variables
//         double phase = 0.0;
//         double phaseIncrement = (2 * Math.PI * frequency) / sampleRate;
//
//         // Process 10ms per iteration
//         int framesPerBlock = sampleRate / 100; // 10ms
//         int samplesPerBlock = framesPerBlock * channels;
//         float[] interleaved = new float[samplesPerBlock];
//
//                 // === Fill interleaved buffer ===
//                 for (int i = 0; i < framesPerBlock; i++) {
//                     float sample = (float) (Math.Sin(phase) * amplitude);
//                     interleaved[i * 2] = sample; // L
//                     interleaved[i * 2 + 1] = sample; // R
//
//                     phase += phaseIncrement;
//                     if (phase >= 2 * Math.PI)
//                         phase -= 2 * Math.PI;
//                 }
//
//                 // === Enqueue until all samples are accepted ===
//                 int written = 0;
//                 while (written < samplesPerBlock) {
//                     int count = system.Enqueue(interleaved.AsSpan(written));
//                     if (count == 0) {
//                         // Ring buffer full → give NAudio time to consume data
//                         await Task.Delay(1);
//                         continue;
//                     }
//
//                     written += count;
//                 }
//          */
//     }
// }