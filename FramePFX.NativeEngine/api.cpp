#include "std.h"
#include "effects/pixellate.h"

#define API_EXPORT _declspec(dllexport) WINAPI

extern "C" {
	HRESULT API_EXPORT PFXCE_InitEngine() {
		return 1;
	}

	HRESULT API_EXPORT PFXCE_PixelateVfx(uint32_t* pImg, int srcWidth, int srcHeight, int left, int top, int right, int bottom, int blockSize) {
		pixelate_core(pImg, srcWidth, srcHeight, left, top, right, bottom, blockSize);
		return 0;
	}
}