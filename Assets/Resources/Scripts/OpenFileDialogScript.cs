using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class OpenFileDialogScript : MonoBehaviour
{
    public GridLayoutGroup existingSeriesGrid;
    public GameObject existingSeriesButtonPrefab;
    public delegate void OnVideoSeriesSelected(VideoSeries v);
    public TMPro.TMP_InputField NameEditText;
    public TextMeshProUGUI ChosenPathText;
    public TextMeshProUGUI currentSeason;
    public TextMeshProUGUI currentEpisode;
    public FindFilePathScript FindFilePath;

    private const string mDefaultNewSeriesName = "Unset";

    List<VideoSeries> mSeries = new List<VideoSeries>();
    Dictionary<VideoSeries, ExistingSeriesButtonScript> mSeriesButtonDictionary = new Dictionary<VideoSeries, ExistingSeriesButtonScript>();

    private int mCurrentlySelectedSeries = -1;

    public void Start() {
        LoadSeriesData();
        foreach(var series in mSeries) {
            MakeSeriesButton(series);
        }
        if(mSeries.Count > 0) {
            OnSeriesButtonClicked(mSeries[0]);
        }
        if(FindFilePath != null) {
            FindFilePath.OnFilePathFoundEvent += OnFilePathChosen;
        }
    }

    private void MakeSeriesButton(VideoSeries series) {
        GameObject newSeriesButton = Instantiate(existingSeriesButtonPrefab, existingSeriesGrid.transform);
        ExistingSeriesButtonScript script = newSeriesButton.GetComponent<ExistingSeriesButtonScript>();
        script.setup(series, OnSeriesButtonClicked);
        mSeriesButtonDictionary.Add(series, script);
    }

    void OnSeriesButtonClicked(VideoSeries s) {
        int index = mSeries.IndexOf(s);

        if (index >= 0) {
            mCurrentlySelectedSeries = index;
        }

        if(NameEditText != null) {
            NameEditText.text = s.Name;
        }
        if(ChosenPathText != null) {
            ChosenPathText.SetText(s.FilePath);
        }
    }

    private void OnFilePathChosen(string newPath) {
        if (newPath != "") {
            UpdatePathForCurrentSeries(newPath);
        }
    }

    private void UpdatePathForCurrentSeries(string path) {
        mSeries[mCurrentlySelectedSeries].FilePath = path;
        if (ChosenPathText != null) {
            ChosenPathText.SetText(path);
        }
        Debug.Log(path);
    }

    public void _AddSeries() {
        VideoSeries s = new VideoSeries();
        s.Name = mDefaultNewSeriesName;
        s.FilePath = "";
        s.LastWatchedEpisode = 1;
        s.LastWatchedSeason = 1;
        mSeries.Add(s);
        MakeSeriesButton(s);
        OnSeriesButtonClicked(s);
    }

    public void _UpdateSeries() {
        VideoSeries s = mSeries[mCurrentlySelectedSeries];
        s.Name = NameEditText.text;
        s.FilePath = ChosenPathText.text;
        mSeriesButtonDictionary[s].ButtonText.text = s.Name;
    }

    void LoadSeriesData() {
        string path = Application.streamingAssetsPath + "/Series/SeriesInfo.json";
        string str = "";
        using (FileStream fs = new FileStream(path, FileMode.Open)) {
            using (StreamReader reader = new StreamReader(fs)) {
                str += reader.ReadToEnd(); 
            }
            VideoSeries[] vidArray = JsonHelper.FromJson<VideoSeries>(str);
            mSeries.AddRange(vidArray);
        }
    }

    public void _SaveItemInfo() {
        string path = null;
#if UNITY_EDITOR
        //path = "Assets/Resources/SeriesData/SeriesInfo.json";
#endif
#if UNITY_STANDALONE
        // You cannot add a subfolder, at least it does not work for me

#endif
        path = Application.streamingAssetsPath + "/Series/SeriesInfo.json";
        using (FileStream fs = new FileStream(path, FileMode.Truncate)) {
            using (StreamWriter writer = new StreamWriter(fs)) {
                string temp = JsonHelper.ToJson<VideoSeries>(mSeries.ToArray(), true);
                writer.Write(temp);
            }
        }
        #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
        #endif
    }

}
