using Avalonia;

namespace FramePFX.Avalonia.Interactivity.Contexts;

/// <summary>
/// Context data for a control that automatically invalidates the control's inherited context data when modifying this instance
/// </summary>
public sealed class ControlContextData : BaseControlContextData {
    public ControlContextData(AvaloniaObject owner) : base(owner) {
    }
    
    public ControlContextData(AvaloniaObject owner, InheritingControlContextData? copyFromNonInherited) : this(owner) {
        this.CopyFrom(copyFromNonInherited?.NonInheritedEntries);
    }

    public override MultiChangeToken BeginChange() => new MultiChangeTokenImpl(this);

    private class MultiChangeTokenImpl : MultiChangeToken {
        public MultiChangeTokenImpl(ControlContextData context) : base(context) {
            context.batchCounter++;
        }

        protected override void OnDisposed() {
            ((ControlContextData) this.Context).OnMultiChangeTokenDisposed();
        }
    }
}