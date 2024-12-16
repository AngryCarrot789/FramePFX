#pragma once
#include <cstdint>

void pixelate_core(uint32_t* p_img, int src_width, int src_height, int left, int top, int right, int bottom, int block_size);
