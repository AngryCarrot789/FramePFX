// #version 150
//
// #define NUM_OF_ROWS 32
//
// //Globals
// uniform vec2 uv_offset;
// uniform mat4 mvp;
// uniform mat4 mv;
//
// //Inputs
// in vec3 in_pos;
// in vec3 in_normal;
// in vec2 in_uv;
//
// //Outputs
// out vec2 ex_uv;
// out vec3 ex_normal;
//
// void main(void) {
//     gl_Position = mvp * vec4(in_pos, 1.0);
//     ex_uv = ((in_uv + uv_offset) / NUM_OF_ROWS);
//     // ex_uv = in_uv;
//     ex_normal = normalize((mv * vec4(in_normal, 0.0)).xyz);
// }

#version 150

#define NUM_OF_ROWS 32

//Globals
uniform vec2 uv_offset;
uniform mat4 mvp;       // projection matrix
uniform mat4 mv;        // model view; used for normals

//Inputs
in vec3 in_pos;     // input vertex position, from mesh
in vec3 in_normal;  // input vertex position, from mesh
in vec2 in_uv;      // input vertex position, from mesh

//Outputs
out vec2 ex_uv;
out vec3 ex_normal;

// vec2 textureCoords[4] = vec2[4](vec2(0.0f, 0.0f), vec2(1.0f, 0.0f), vec2(1.0f, 1.0f), vec2(0.0f, 1.0f));

void main(void) {
	gl_Position = mvp * vec4(in_pos, 1.0);
    ex_uv = ((in_uv + uv_offset) / NUM_OF_ROWS);
    // ex_uv = in_uv;
    ex_normal = normalize((mv * vec4(in_normal, 0.0)).xyz);
}
