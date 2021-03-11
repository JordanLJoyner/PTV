using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class BuildScheduleScript : MonoBehaviour {
    public DisplaySeriesScript mDisplaySeriesScript;
    public GameObject SeriesButtonPrefab;

    public TMPro.TextMeshProUGUI StyleText;
    private ScheduleType mScheduleType = ScheduleType.HARD_RANDOM;

    private List<ExistingSeriesButtonScript> mSeriesButtons = new List<ExistingSeriesButtonScript>();
    public GridLayoutGroup scheduleGrid;

    // Start is called before the first frame update
    void Start() {
        mDisplaySeriesScript.SetButtonClickCallback(OnSeriesClicked);
        Schedule schedule = FileUtils.LoadSchedule();
        mScheduleType = schedule.scheduleType;
        VideoSeries[] seriesData = FileUtils.LoadSeriesData();
        foreach (var item in schedule.items) {
            string seriesName = item.showName;
            foreach(VideoSeries s in seriesData) {
                if (s.Name.Equals(seriesName)) {
                    OnSeriesClicked(s);
                    break;
                }
            }
        }
        
        UpdateScheduleText();
    }

    public void OnSeriesClicked(VideoSeries s) {
        //Create a series button and add it to the schedule
        GameObject newButton = Instantiate(SeriesButtonPrefab, scheduleGrid.transform);
        var seriesButtonScript = newButton.GetComponent<ExistingSeriesButtonScript>();
        seriesButtonScript.setup(s, OnScheduledSeriesButtonClick);
        mSeriesButtons.Add(seriesButtonScript);
    }

    private void OnScheduledSeriesButtonClick(VideoSeries s) {

    }

    public void _ChangeScheduleStyle() {
        mScheduleType++;
        if(mScheduleType >= ScheduleType.MAX) {
            mScheduleType = ScheduleType.START + 1;
        }
        UpdateScheduleText();
    }

    private void UpdateScheduleText() {
        StyleText.text = "Style: " + mScheduleType.ToString();
    }

    public void _SaveSchedule() {
        Schedule schedule = new Schedule();
        schedule.scheduleType = mScheduleType;
        var scheduleItems = new List<ScheduleItem>();
        for(int i=0; i < mSeriesButtons.Count; i++) {
            ScheduleItem item = new ScheduleItem();
            item.showName = mSeriesButtons[i].GetSeriesName();
            item.scheduleType = ScheduleItemType.RANDOM;
            scheduleItems.Add(item);
        }
        schedule.items = scheduleItems;

        string path = null;
        path = Application.streamingAssetsPath + "/Schedule.json";
        using (FileStream fs = new FileStream(path, FileMode.Truncate)) {
            using (StreamWriter writer = new StreamWriter(fs)) {
                string temp = JsonUtility.ToJson(schedule);
                writer.Write(temp);
            }
        }
        #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    public void _OnBackToStartingSceneClicked() {
        SceneManager.LoadScene("StartingScene");
    }

    public void _OnClearScheduleClicked() {
        foreach(var button in mSeriesButtons) {
            Destroy(button.gameObject);
        }
        mSeriesButtons.Clear();
    }
}
