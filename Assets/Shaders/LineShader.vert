#version 330 core

uniform mat4 mvp;
layout (location = 0) in vec3 in_pos;

void main() {
   gl_Position = mvp * vec4(in_pos, 1.0);
}