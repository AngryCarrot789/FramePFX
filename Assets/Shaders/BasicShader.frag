// #version 330
// precision highp float;
//
// #define LIGHT vec3(0.36, 0.80, 0.48)
//
// // Inputs
//
// uniform vec3 in_colour;
// in vec2 ex_uv;
// in vec3 ex_normal;
//
// void main(void) {
//     float s = dot(ex_normal, LIGHT) * 0.25 + 0.75;
//     gl_FragColor = vec4(in_colour * s, 1.0);
// }


#version 330

uniform vec3 in_color;
out vec3 color;

void main(void) {
    color = in_color;
}