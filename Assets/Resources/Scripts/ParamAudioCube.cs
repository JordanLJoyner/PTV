using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamAudioCube : MonoBehaviour
{
    public int Band;
    public float startScale, scaleMultiplier;
    public bool _useBuffer = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_useBuffer) {
            transform.localScale = new Vector3(transform.localScale.x, (AudioPeer.mBandBuffer[Band] * scaleMultiplier) + startScale, transform.localScale.z);
        } else {
            transform.localScale = new Vector3(transform.localScale.x, (AudioPeer.mFreqBand[Band] * scaleMultiplier) + startScale, transform.localScale.z);
        }
    }
}
