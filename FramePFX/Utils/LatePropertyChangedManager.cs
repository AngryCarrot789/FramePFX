using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FramePFX.Utils {
    /// <summary>
    /// A class that helps with firing property changed events on background threads,
    /// by scheduling a callback that may fire multiple property changed events at once
    /// </summary>
    public class LatePropertyChangedManager : INotifyPropertyChanging, INotifyPropertyChanged {
        private readonly List<string> properties;
        private volatile bool criticalIsProcessingAction;
        private volatile bool isActionScheduled;
        private volatile int state;
        private readonly Action<string> onPropertyChanged;

        private const int STATE_CRITICAL_OPC = 0;   // OnPropertyChanged is processing the property list. AMT cannot do anything
        private const int STATE_APPENDED_SAFE = 1;  // OnPropertyChanged is in a safe state. AMT can begin processing
        private const int STATE_AMT_PROCESSING = 2; // AMT (App Main Thread) is processing properties

        /// <summary>
        /// An event fired, possibly on a background thread, when <see cref="RaisePropertyChanged{T}"/> is invoked.
        /// The sender parameter will not be an instance of <see cref="LatePropertyChangedManager"/>,
        /// but instead our <see cref="Owner"/> property
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// An event fired, during the main thread callback, once our owner's <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// event has been fired. The sender parameter will not be an instance of <see cref="LatePropertyChangedManager"/>,
        /// but instead our <see cref="Owner"/> property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public INotifyPropertyChanged Owner { get; }

        public LatePropertyChangedManager(INotifyPropertyChanged owner, Action<string> onPropertyChanged) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.properties = new List<string>();
            this.onPropertyChanged = onPropertyChanged;
        }

        // not exactly sure if this works when it's called by multiple threads... but it
        // should work as long as it's called by either the AMT or a single other thread

        public void RaisePropertyChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null, Action<T> inSafeState = null) {
            field = value;
            bool scheduleAction = this.BeginPropertyChanged(propertyName);
            this.PropertyChanging?.Invoke(this.Owner, new PropertyChangingEventArgs(propertyName));
            inSafeState?.Invoke(value);
            this.EndPropertyChanged(scheduleAction);
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            bool scheduleAction = this.BeginPropertyChanged(propertyName);
            this.PropertyChanging?.Invoke(this.Owner, new PropertyChangingEventArgs(propertyName));
            this.EndPropertyChanged(scheduleAction);
        }

        private bool BeginPropertyChanged(string propertyName) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            bool scheduleAction = false;
            lock (this.properties) {
                if (!this.properties.Contains(propertyName)) {
                    this.properties.Add(propertyName);
                    if (!this.isActionScheduled) {
                        this.state = STATE_CRITICAL_OPC;
                        scheduleAction = true;
                    }
                }
            }

            return scheduleAction;
        }

        private void EndPropertyChanged(bool scheduleAction) {
            this.state = STATE_APPENDED_SAFE;
            if (!this.criticalIsProcessingAction && scheduleAction) {
                this.isActionScheduled = true;
                IoC.Application.Dispatcher.Invoke(this.OnMainThreadCallback);
            }
        }

        private void OnMainThreadCallback() {
            lock (this.properties) {
                this.criticalIsProcessingAction = true;
                try {
                    // OnPropertyChanged called, is now updating field, but may be attempting to
                    // schedule an action on the main thread. Spin wait until that's done, so we know
                    while (Interlocked.CompareExchange(ref this.state, STATE_AMT_PROCESSING, STATE_APPENDED_SAFE) != STATE_APPENDED_SAFE) {
                        if (!Thread.Yield())
                            Thread.Sleep(1);
                    }

                    foreach (string property in this.properties) {
                        this.onPropertyChanged(property);
                        this.PropertyChanged?.Invoke(this.Owner, new PropertyChangedEventArgs(property));
                    }

                    this.properties.Clear();
                }
                finally {
                    this.isActionScheduled = false;
                    this.criticalIsProcessingAction = false;
                    this.state = STATE_APPENDED_SAFE;
                }
            }
        }
    }
}