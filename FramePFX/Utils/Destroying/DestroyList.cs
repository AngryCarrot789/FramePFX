namespace FramePFX.Utils.Destroying;

public sealed class DestroyList : IDestroy {
    private object? list;

    public void Add(IDestroy destroy) {
        if (this.list == null) {
            this.list = destroy;
        }
        else if (this.list is IDestroy otherDestroy) {
            this.list = new List<IDestroy>() { otherDestroy, destroy };
        }
        else {
            ((List<IDestroy>) this.list).Add(destroy);
        }
    }

    public void Destroy() {
        switch (this.list) {
            case null:             return;
            case IDestroy destroy: destroy.Destroy(); break;
            case List<IDestroy> destroyList: {
                using ErrorList errorList = new ErrorList("Exception destroying one or more objects", true, false);
                foreach (IDestroy destroy in destroyList) {
                    try {
                        destroy.Destroy();
                    }
                    catch (Exception e) {
                        errorList.Add(e);
                    }
                }

                break;
            }
        }
    }
}