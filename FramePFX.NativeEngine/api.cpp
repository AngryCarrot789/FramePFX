#include "std.h"
#include "effects/pixellate.h"

#define API_EXPORT _declspec(dllexport) WINAPI

#include <stdio.h>
#include <math.h>
#include "portaudio.h"

#define NUM_SECONDS   (2)
#define SAMPLE_RATE   (44100)
#define TABLE_SIZE    (200)
#define TEST_UNSIGNED (0)

#ifndef M_PI
#define M_PI (3.14159265)
#endif

typedef int ManagedAudioEngineCallback(void* output, unsigned long framesPerBuffer, const PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags);

typedef struct
{
    ManagedAudioEngineCallback* lpManagedAudioEngineCallBack;
    PaStream* stream;
} AudioEngineClientData;

/* This routine will be called by the PortAudio engine when audio is needed.
** It may called at interrupt level on some machines so don't do anything
** that could mess up the system like calling malloc() or free().
*/
static int AudioEngineCallback(const void* inputBuffer, void* outputBuffer, unsigned long framesPerBuffer, const PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags, void* userData)
{
    AudioEngineClientData* data = (AudioEngineClientData*)userData;
    if (data->lpManagedAudioEngineCallBack) {
        return data->lpManagedAudioEngineCallBack(outputBuffer, framesPerBuffer, timeInfo, statusFlags);
    }

    return 1;
}

extern "C" {
	HRESULT API_EXPORT PFXCE_InitEngine() {
        if (Pa_Initialize() != paNoError) {
            return -1;
        }

		return 1;
	}

    HRESULT API_EXPORT PFXCE_ShutdownEngine() {
        if (Pa_Terminate() != paNoError) {
            return -1;
        }

        return 1;
    }

	HRESULT API_EXPORT PFXCE_PixelateVfx(uint32_t* pImg, int srcWidth, int srcHeight, int left, int top, int right, int bottom, int blockSize) {
		pixelate_core(pImg, srcWidth, srcHeight, left, top, right, bottom, blockSize);
		return 0;
	}

	HRESULT API_EXPORT PFXAE_BeginAudioPlayback(AudioEngineClientData* lpEngineData) {
        PaStreamParameters outputParameters;
        PaError            err;
        PaTime             streamOpened;

        outputParameters.device = Pa_GetDefaultOutputDevice(); /* Default output device. */
        if (outputParameters.device == paNoDevice) {
            fprintf(stderr, "Error: No default output device.\n");
            goto error;
        }

        outputParameters.channelCount = 2;                     /* Stereo output. */
        outputParameters.sampleFormat = paFloat32;
        outputParameters.suggestedLatency = Pa_GetDeviceInfo(outputParameters.device)->defaultLowOutputLatency;
        outputParameters.hostApiSpecificStreamInfo = NULL;
        err = Pa_OpenStream(&lpEngineData->stream,
            NULL,      /* No input. */
            &outputParameters,
            SAMPLE_RATE,
            0,       /* Frames per buffer. */
            paClipOff, /* We won't output out of range samples so don't bother clipping them. */
            AudioEngineCallback,
            lpEngineData);

        if (err != paNoError)
            goto error;

        err = Pa_StartStream(lpEngineData->stream);
        if (err != paNoError)
            goto error;

        return 0;
    error:
        fprintf(stderr, "An error occurred while using the portaudio stream\n");
        fprintf(stderr, "Error number: %d\n", err);
        fprintf(stderr, "Error message: %s\n", Pa_GetErrorText(err));
        return (HRESULT) err;
	}

    HRESULT API_EXPORT PFXAE_EndAudioPlayback(AudioEngineClientData* lpEngineData) {
        PaError err = Pa_StopStream(lpEngineData->stream);
        if (err == paNoError)
            err = Pa_CloseStream(lpEngineData->stream);
        lpEngineData->stream = NULL;
        return err;
    }
}