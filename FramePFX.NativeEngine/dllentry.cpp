#include "std.h"

#include <iostream>

extern "C"
BOOL __stdcall DllMain(HINSTANCE dllHandle, ULONG reason, CONTEXT * ctx) {
    BOOL result = TRUE;

    switch (reason) {

    case DLL_PROCESS_ATTACH: {
        HRESULT hr = S_OK;

        // TODO: init

        result = SUCCEEDED(hr);
        break;
    }

    case DLL_PROCESS_DETACH: {
        break;
    }

    }

    return(result);
}
