#version 150
precision highp float;

//Inputs
uniform vec3 in_colour;

void main(void) {
	gl_FragColor = vec4(in_colour, 1.0);
}
