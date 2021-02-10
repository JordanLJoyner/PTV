using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySeriesScript : MonoBehaviour
{
    public GridLayoutGroup existingSeriesGrid;
    public GameObject existingSeriesButtonPrefab;
    public delegate void OnVideoSeriesSelected(VideoSeries v);
    Dictionary<string, ExistingSeriesButtonScript> mSeriesButtonDictionary = new Dictionary<string, ExistingSeriesButtonScript>();
    private List<GameObject> mButtons = new List<GameObject>();
    OnVideoSeriesSelected mClickCallback;

    private void Start() {
        //Make a button for all the series data
        foreach (var series in FileUtils.LoadSeriesData()) {
            MakeSeriesButton(series);
        }
    }

    public void MakeSeriesButton(VideoSeries series) {
        GameObject newSeriesButton = Instantiate(existingSeriesButtonPrefab, existingSeriesGrid.transform);
        ExistingSeriesButtonScript script = newSeriesButton.GetComponent<ExistingSeriesButtonScript>();
        script.setup(series, OnVideoSeriesButtonClicked);
        mSeriesButtonDictionary.Add(series.Name, script);
        mButtons.Add(newSeriesButton);
    }


    public void SetButtonClickCallback(OnVideoSeriesSelected clickCallback) {
        mClickCallback += clickCallback;
    }

    public void OnVideoSeriesButtonClicked(VideoSeries s) {
        if (mClickCallback != null) {
            mClickCallback(s);
        }
    }

    public void UpdateName(VideoSeries s, string newName) {
        mSeriesButtonDictionary[s.Name].ButtonText.text = newName;
    }

    public void ClearSeries() {
        mSeriesButtonDictionary.Clear();
        foreach(GameObject go in mButtons) {
            Destroy(go);
        }
        mButtons.Clear();
    }
}
