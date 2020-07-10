using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiate512Cubes : MonoBehaviour
{
    public GameObject SampleCubePrefab;
    public float maxScale;
    GameObject[] mSampleCubes = new GameObject[512];
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i < 512; i++) {
            GameObject newCube = Instantiate(SampleCubePrefab);
            newCube.transform.position = this.transform.position;
            newCube.transform.parent = this.transform;
            newCube.name = "SampleCube" + i;
            this.transform.eulerAngles = new Vector3(0, -0.703125f * i);
            newCube.transform.position = Vector3.forward * 100;
            mSampleCubes[i] = newCube;
        }   
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0; i < mSampleCubes.Length; i++) {
            if(mSampleCubes[i] != null) {
                mSampleCubes[i].transform.localScale = new Vector3(10,(AudioPeer._samples[i] * maxScale)+2, 10);
            }
        }
    }
}
