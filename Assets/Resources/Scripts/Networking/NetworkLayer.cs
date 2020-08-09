using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkLayer : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        MyNetworkClass b = new MyNetworkClass();
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnDestroy() {
    }
}
