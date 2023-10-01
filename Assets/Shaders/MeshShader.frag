#version 330
#define LIGHT vec3(0.36, 0.80, 0.48)

uniform sampler2D tex;
uniform float use_colour;
uniform vec3 in_color;

// Inputs
in vec2 ex_uv;
in vec3 ex_normal;

out vec4 FragColor;

void main(void) {
    vec3 colour;
    if (use_colour > 0.5) {
        colour = in_color;
    }
    else {
        colour = texture(tex, ex_uv).rgb;
    }

    float s = dot(ex_normal, LIGHT) * 0.25 + 0.75;
    FragColor = vec4(colour * s, 1.0);
}