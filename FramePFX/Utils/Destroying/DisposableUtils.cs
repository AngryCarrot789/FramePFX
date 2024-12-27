// 
// Copyright (c) 2024-2024 REghZy
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