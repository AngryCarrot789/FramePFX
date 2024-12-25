#include "pixellate.h"

void pixelate_core(uint32_t* p_img, const int src_width, int src_height, const int left, const int top, const int right, const int bottom, const int block_size)
{
    uint32_t pixel;
    for (int block_y = top; block_y < bottom; block_y += block_size)
    {
        for (int block_x = left; block_x < right; block_x += block_size)
        {
            uint32_t total_r = 0, total_g = 0, total_b = 0, total_a = 0;
            int px_total = 0;
            for (int pY = block_y; pY < block_y + block_size && pY < bottom; ++pY)
            {
                for (int pX = block_x; pX < block_x + block_size && pX < right; ++pX)
                {
                    pixel = p_img[pY * src_width + pX];
                    total_b += (pixel >> 0) & 255;
                    total_g += (pixel >> 8) & 255;
                    total_r += (pixel >> 16) & 255;
                    total_a += (pixel >> 24) & 255;
                    px_total++;
                }
            }

            pixel = (total_b / px_total) | ((total_g / px_total) << 8) | ((total_r / px_total) << 16) | ((total_a / px_total) << 24);

            for (int pY = block_y; pY < block_y + block_size && pY < bottom; ++pY)
            {
                for (int pX = block_x; pX < block_x + block_size && pX < right; ++pX)
                {
                    p_img[pY * src_width + pX] = pixel;
                }
            }
        }
    }
}
