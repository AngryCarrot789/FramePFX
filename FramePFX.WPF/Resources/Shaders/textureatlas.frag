#version 150
precision highp float;

#define LIGHT vec3(0.36, 0.80, 0.48)

//Inputs
uniform sampler2D tex;
in vec2 ex_uv;
in vec3 ex_normal;

void main(void) {
	float s = dot(ex_normal, LIGHT) *0.25 + 0.75;
	gl_FragColor = vec4(texture(tex, ex_uv).rgb * s, 1.0);
}
