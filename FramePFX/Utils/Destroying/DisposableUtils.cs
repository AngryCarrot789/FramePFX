namespace FramePFX.Utils.Destroying;

/// <summary>
/// A helper class for disposable objects
/// </summary>
public static class DisposableUtils {
    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2) {
        if (d1 != null) try { d1.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d2 != null) try { d2.Dispose(); } catch (Exception e) { errorList?.Add(e); }
    }
    
    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3) {
        if (d1 != null) try { d1.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d2 != null) try { d2.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d3 != null) try { d3.Dispose(); } catch (Exception e) { errorList?.Add(e); }
    }
    
    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3, IDisposable? d4) {
        if (d1 != null) try { d1.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d2 != null) try { d2.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d3 != null) try { d3.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d4 != null) try { d4.Dispose(); } catch (Exception e) { errorList?.Add(e); }
    }
    
    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3, IDisposable? d4, IDisposable? d5) {
        if (d1 != null) try { d1.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d2 != null) try { d2.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d3 != null) try { d3.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d4 != null) try { d4.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d5 != null) try { d5.Dispose(); } catch (Exception e) { errorList?.Add(e); }
    }
    
    public static void DisposeMany(ErrorList? errorList, IDisposable? d1, IDisposable? d2, IDisposable? d3, IDisposable? d4, IDisposable? d5, IDisposable? d6) {
        if (d1 != null) try { d1.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d2 != null) try { d2.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d3 != null) try { d3.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d4 != null) try { d4.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d5 != null) try { d5.Dispose(); } catch (Exception e) { errorList?.Add(e); }
        if (d6 != null) try { d6.Dispose(); } catch (Exception e) { errorList?.Add(e); }
    }
}