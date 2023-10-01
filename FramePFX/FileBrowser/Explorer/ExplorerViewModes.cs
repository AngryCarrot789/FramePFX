using System;
using System.Collections.Generic;
using FramePFX.FileBrowser.Explorer.ViewModes;

namespace FramePFX.FileBrowser.Explorer
{
    public static class ExplorerViewModes
    {
        public static ListBasedViewMode ListBased { get; } = new ListBasedViewMode();

        public static IconViewMode SmallIcons => new IconViewMode(40);
        public static IconViewMode MediumIcons => new IconViewMode(80);
        public static IconViewMode LargeIcons => new IconViewMode(150);

        private static readonly Dictionary<string, Func<IExplorerViewMode>> idToCtor = new Dictionary<string, Func<IExplorerViewMode>>();

        public static void Register(string id, Func<IExplorerViewMode> constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));
            idToCtor[id] = constructor;
        }

        static ExplorerViewModes()
        {
            Register("List", () => new ListBasedViewMode());
            Register("Icons", () => new IconViewMode());
        }

        public static IExplorerViewMode CreateView(string id)
        {
            if (!idToCtor.TryGetValue(id, out Func<IExplorerViewMode> func))
            {
                throw new Exception("No such constructor for id: " + id);
            }

            return func() ?? throw new Exception("Constructor returned a null value");
        }
    }
}