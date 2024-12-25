using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Interactivity.Contexts;

public class DelegatingContextData : IContextData {
    private readonly IContextData[] contextDataArray;

    public IEnumerable<KeyValuePair<string, object>> Entries => this.contextDataArray.SelectMany(x => x.Entries);

    public DelegatingContextData(IContextData data1) : this([data1]) { }

    public DelegatingContextData(IContextData data1, IContextData data2) : this([data1, data2]) { }

    public DelegatingContextData(IContextData[] contextDataArray) {
        this.contextDataArray = contextDataArray;
    }

    public bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        foreach (IContextData data in this.contextDataArray) {
            if (data.TryGetContext(key, out value))
                return true;
        }

        value = null;
        return false;
    }

    public bool ContainsKey(string key) => this.contextDataArray.Any(x => x.ContainsKey(key));
}