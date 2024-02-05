#include "..\std.h"

#include "pixellate.h"

void pixelate_core(uint32_t* pImg, int srcWidth, int srcHeight, int left, int top, int right, int bottom, int blockSize) {
    uint32_t pixel;
    for (int blockY = top; blockY < bottom; blockY += blockSize) {
        for (int blockX = left; blockX < right; blockX += blockSize) {
            uint32_t totalR = 0, totalG = 0, totalB = 0, totalA = 0;
            int pxTotal = 0;
            for (int pY = blockY; pY < blockY + blockSize && pY < bottom; ++pY) {
                for (int pX = blockX; pX < blockX + blockSize && pX < right; ++pX) {
                    pixel = pImg[pY * srcWidth + pX];
                    totalB += (pixel >> 0) & 255;
                    totalG += (pixel >> 8) & 255;
                    totalR += (pixel >> 16) & 255;
                    totalA += (pixel >> 24) & 255;
                    pxTotal++;
                }
            }

            pixel = (totalB / pxTotal) | ((totalG / pxTotal) << 8) | ((totalR / pxTotal) << 16) | ((totalA / pxTotal) << 24);

            for (int pY = blockY; pY < blockY + blockSize && pY < bottom; ++pY) {
                for (int pX = blockX; pX < blockX + blockSize && pX < right; ++pX) {
                    pImg[pY * srcWidth + pX] = pixel;
                }
            }
        }
    }
}