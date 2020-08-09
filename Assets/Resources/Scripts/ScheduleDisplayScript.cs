using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleDisplayScript : MonoBehaviour
{
    public GameObject gridParent;
    public GridLayoutGroup gridView;
    public GameObject scheduleObjectPrefab;
    int index = 0;
    List<string> scheduleObjects;
    List<GameObject> instantiatedScheduleObjects = new List<GameObject>();
    private const int mNumScheduleItemsToDisplay = 3;

    public void LoadSchedule(List<string> displaySchedule) {
        index = 0;
        scheduleObjects = displaySchedule;
        for(int i=0; i < instantiatedScheduleObjects.Count; i++) {
            Destroy(instantiatedScheduleObjects[i]);
        }
        instantiatedScheduleObjects.Clear();

        int maxLoop = mNumScheduleItemsToDisplay;
        if(displaySchedule.Count < maxLoop) {
            maxLoop = displaySchedule.Count;
        }

        for (int i=0; i < maxLoop; i++) {
            instantiatedScheduleObjects.Add(Instantiate(scheduleObjectPrefab, gridView.transform));   
        }
        UpdateDisplay();
    }

    public void AdvanceSchedule() {
        index++;
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        for(int i=0; i < instantiatedScheduleObjects.Count; i++) {
            instantiatedScheduleObjects[i].SetActive(false);
        }
        int maxLoop = 3;
        if(scheduleObjects.Count - index < 3) {
            maxLoop = scheduleObjects.Count - index;
        }
        for(int i=0; i < maxLoop; i++) {
            string text = "";
            if (i == 0) {
                text += "Up Next - ";
            }
            text += scheduleObjects[index + i];
            instantiatedScheduleObjects[i].SetActive(true);
            instantiatedScheduleObjects[i].GetComponent<TMPro.TextMeshProUGUI>().SetText(text);
        }
    }

    public void ShowSchedule() {
        this.gameObject.SetActive(true);
    }

    public void HideSchedule() {
        this.gameObject.SetActive(false);
    }
}
