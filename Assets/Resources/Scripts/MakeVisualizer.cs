using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeVisualizer : MonoBehaviour
{
    public GameObject AudioBlockPrefab;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i < 8; i++) {
            GameObject obj = Instantiate(AudioBlockPrefab, this.transform);
            obj.name = "Visualizer block " + i;
            obj.GetComponent<ParamAudioCube>().Band = i;
            obj.transform.position = new Vector3(7.5f * i, obj.transform.position.y, obj.transform.position.z);
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
