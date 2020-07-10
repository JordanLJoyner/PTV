using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 newRotation;
    // Update is called once per frame
    void Update()
    {
        newRotation = this.transform.eulerAngles;
        newRotation.z -= 180.0f * Time.deltaTime;
        this.transform.eulerAngles = newRotation;
    }
}
