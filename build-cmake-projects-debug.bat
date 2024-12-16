cd ..\portaudio
mkdir buildsln64
cd buildsln64
cmake ..\ -G "Visual Studio 17 2022" -A x64
cmake --build . --config Debug