#include "std.h"

#define API_EXPORT _declspec(dllexport) WINAPI

extern "C" {
	HRESULT API_EXPORT PFXCE_InitEngine() {
		return 1;
	}
}