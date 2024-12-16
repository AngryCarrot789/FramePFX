#include <windows.h>

BOOL APIENTRY DllMain(HMODULE hModuleDll, DWORD  reason, CONTEXT * ctx)
{
    BOOL result = TRUE;

    switch (reason) {

    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH: {
            HRESULT hResult = S_OK;

            // TODO: init

            result = SUCCEEDED(hResult);
            break;
    }

    case DLL_PROCESS_DETACH: 
    case DLL_THREAD_DETACH: {
            break;
    }
    }

    return(result);
}

