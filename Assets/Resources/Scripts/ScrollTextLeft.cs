using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScrollTextLeft : MonoBehaviour
{
    Vector3 temp = new Vector3();
    TextMeshProUGUI text;
    private bool scroll = true;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>(); 
    }

    IEnumerator resetScroll() {
        scroll = false;
        yield return new WaitForSeconds(2.0f);
        scroll = true;
    }

    // Update is called once per frame
    void Update()
    {
        temp = this.transform.position;
        if (text.text.Length > 30) {
            if (scroll) {
                if (this.transform.position.x < -1200) {
                    temp.x = 0;
                    StartCoroutine(resetScroll());
                } else {
                    temp.x -= 100 * Time.deltaTime;
                }
            }
        } else {
            temp.x = 0;
        }
        this.transform.position = temp;
    }
}
