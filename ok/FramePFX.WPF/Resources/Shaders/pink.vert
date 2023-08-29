#version 150

//Globals
uniform mat4 mvp;

//Inputs
in vec3 in_pos;

void main(void) {
	gl_Position = mvp * vec4(in_pos, 1.0);
}
