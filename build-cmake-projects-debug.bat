cd ..\portaudio
mkdir buildsln64
cd buildsln64
cmake ..\ -G "Visual Studio 18 2026" -A x64
cmake --build . --config Debug