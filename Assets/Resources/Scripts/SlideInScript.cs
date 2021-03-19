using System.Collections;
using UnityEngine;

public class SlideInScript : MonoBehaviour
{
    [SerializeField] private float mDistance = 250.0f;
    [SerializeField] private int mSteps = 10;
    [SerializeField] private float mTotalTime = 1.0f;

    public void SlideIn() {
        var currentPos = this.transform.position;
        currentPos.x -= mDistance;
        this.transform.position = currentPos;
        StartCoroutine(Slide(mDistance));    
    }

    public void SlideOut() {
        StartCoroutine(Slide(mDistance * -1));
    }

    private IEnumerator Slide(float distance) {
        int counter = 0;
        float steps = mSteps;
        float totalTime = mTotalTime;
        while (counter < (int)steps) {
            counter++;
            var currentPos = this.transform.position;
            currentPos.x += distance / steps;
            this.transform.position = currentPos;
            yield return new WaitForSeconds(totalTime / steps);
        }
    }
}
