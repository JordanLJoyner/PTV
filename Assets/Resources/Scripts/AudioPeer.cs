using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    AudioSource mAudioSource;
    public static float[] _samples = new float[512];
    public static float[] mFreqBand = new float[8];
    public static float[] mBandBuffer = new float[8];
    float[] mBufferDecrease = new float[8];

    // Start is called before the first frame update
    void Start()
    {
        mAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
    }

    void GetSpectrumAudioSource() {
        mAudioSource.GetSpectrumData(_samples,0,FFTWindow.Blackman);
    }

    void BandBuffer() {
        for(int g=0; g < 8; g++) {
            if(mFreqBand[g] > mBandBuffer[g]) {
                mBandBuffer[g] = mFreqBand[g];
                mBufferDecrease[g] = 0.005f;
            }

            if (mFreqBand[g] < mBandBuffer[g]) {
                mBandBuffer[g] -= mBufferDecrease[g];
                mBufferDecrease[g] *= 1.2f;
            }
        }
    }

    void MakeFrequencyBands() {
        /*
         * 22050 hz / 512 = 43 hertz per sample 
         * 
         * 20 - 60 hertz
         * 60 - 250 hertz
         * 250-500 hertz
         * 500-2000 hertz
         * 2000-4000 hertz
         * 4000-6000 hertz
         * 6000-20000 hertz
         * 
         * 0-2 = 86 hertz
         * 1-4 = 172 hertz - 87-258
         * 2 - 8 = 344 hert - 259 - 602 
         * 3-16 = 688 hertz - 603-1290
         * 4-32 = 1376 hertz - 1291-2666
         * 5-64 = 2752 hertz - 2667-5418
         */

        int count = 0;
        for(int i=0; i <8;i++) {
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if(i == 7) {
                sampleCount += 2;
            }
            //calculate the average amplitude of all the samples combined
            float average = 0;
            for (int j=0; j < sampleCount;j++) {
                average += _samples[count] * (count+1);
                count++;
            }
            average /= count;

            mFreqBand[i] = average * 10;
        }
    }
}
