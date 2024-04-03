#include "std.h"
#include "effects/pixellate.h"

#define API_EXPORT _declspec(dllexport) WINAPI

#include <stdio.h>
#include "portaudio.h"

#define SAMPLE_RATE   (44100)

#ifndef M_PI
#define M_PI (3.14159265)
#endif

typedef int ManagedAudioEngineCallback(void* output, unsigned long frames_per_buffer, const PaStreamCallbackTimeInfo* time_info, PaStreamCallbackFlags status_flags);

typedef struct
{
    ManagedAudioEngineCallback* ManagedAudioEngineCallBack;
    PaStream* stream;
} AudioEngineClientData;

/* This routine will be called by the PortAudio engine when audio is needed.
** It may call at interrupt level on some machines so don't do anything
** that could mess up the system like calling malloc() or free().
*/
static int AudioEngineCallback(const void* inputBuffer, 
                               void* outputBuffer,
                               const unsigned long framesPerBuffer,
                               const PaStreamCallbackTimeInfo* timeInfo,
                               const PaStreamCallbackFlags statusFlags,
                               void* userData) {
    AudioEngineClientData* data = static_cast<AudioEngineClientData*>(userData);
    if (data->ManagedAudioEngineCallBack) {
        return data->ManagedAudioEngineCallBack(outputBuffer, framesPerBuffer, timeInfo, statusFlags);
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

	HRESULT API_EXPORT PFXCE_PixelateVfx(uint32_t* pImg, const int srcWidth, const int srcHeight, const int left, const int top, const int right, const int bottom, const int blockSize) {
		pixelate_core(pImg, srcWidth, srcHeight, left, top, right, bottom, blockSize);
		return 0;
	}

	HRESULT API_EXPORT PFXAE_BeginAudioPlayback(AudioEngineClientData* pEngineData) {
        PaStreamParameters outputParameters;
        PaError            err = {};
        PaTime             streamOpened;

        outputParameters.device = Pa_GetDefaultOutputDevice(); /* Default output device. */
        if (outputParameters.device == paNoDevice) {
            fprintf(stderr, "Error: No default output device.\n");
            goto error;
        }

        outputParameters.channelCount = 2; /* Stereo output. */
        outputParameters.sampleFormat = paFloat32;
        outputParameters.suggestedLatency = Pa_GetDeviceInfo(outputParameters.device)->defaultLowOutputLatency;
        outputParameters.hostApiSpecificStreamInfo = NULL;
        err = Pa_OpenStream(&pEngineData->stream,
            NULL,       /* No input. */
            &outputParameters,
            SAMPLE_RATE,
            0,          /* Frames per buffer. */
            paClipOff,  /* We won't output out of range samples so don't bother clipping them. */
            AudioEngineCallback,
            pEngineData);

        if (err != paNoError)
            goto error;

        err = Pa_StartStream(pEngineData->stream);
        if (err != paNoError)
            goto error;

        return 0;
    error:
        fprintf(stderr, "An error occurred while using the portaudio stream\n");
        fprintf(stderr, "Error number: %d\n", err);
        fprintf(stderr, "Error message: %s\n", Pa_GetErrorText(err));
        return err;
	}

    HRESULT API_EXPORT PFXAE_EndAudioPlayback(AudioEngineClientData* pEngineData) {
        PaError err = Pa_StopStream(pEngineData->stream);
        if (err == paNoError)
            err = Pa_CloseStream(pEngineData->stream);
        pEngineData->stream = NULL;
        return err;
    }
}