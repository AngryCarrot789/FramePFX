// #version 330
//
// uniform mat4 mvp;   // projection matrix
// uniform mat4 mv;    // model view; used for normals
//
// // Inputs
// in vec3 in_pos;     // input vertex position, from mesh
// in vec3 in_normal;  // input vertex position, from mesh
//
// // Outputs
// out vec3 ex_normal;
//
// void main(void) {
//     gl_Position = mvp * vec4(in_pos, 1.0);
//     ex_normal = normalize((mv * vec4(in_normal, 0.0)).xyz);
// }

#version 330

// Uniforms
uniform mat4 mvp;

// Inputs
layout(location = 0) in vec3 in_pos;

void main(void) {
    gl_Position = mvp * vec4(in_pos, 1.0);
}
