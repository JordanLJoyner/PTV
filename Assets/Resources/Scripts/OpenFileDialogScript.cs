using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpenFileDialogScript : MonoBehaviour
{
    public DisplaySeriesScript mDisplaySeriesScript;
    public TMPro.TMP_InputField NameEditText;
    public TextMeshProUGUI ChosenPathText;
    public TextMeshProUGUI currentSeason;
    public TextMeshProUGUI currentEpisode;
    public FindFilePathScript mFindFilePathScript;

    public GridLayoutGroup AllTagsGrid;
    public TextMeshProUGUI NewTagInput;
    
    public GameObject PlusMinusUIPrefab;
    public GridLayoutGroup CurrentTagsGrid;
    private List<GameObject> mTagsForCurrentSeries = new List<GameObject>(); 

    private const string mDefaultNewSeriesName = "Unset";
    private int mCurrentlySelectedSeries = -1;
    List<VideoSeries> mSeries = new List<VideoSeries>();
    List<string> mTags = new List<string>();
    List<GameObject> mExistingSeriesButtons = new List<GameObject>();


    public void Start() {
        //Load Series Data
        mSeries.AddRange(FileUtils.LoadSeriesData());
        mDisplaySeriesScript.SetButtonClickCallback(OnSeriesButtonClicked);

        //Populate the initial series
        if (mSeries.Count > 0) {
            OnSeriesButtonClicked(mSeries[0]);
        }

        //Setup our find finle path button callback
        if (mFindFilePathScript != null) {
            mFindFilePathScript.OnFilePathFoundEvent += OnFilePathChosen;
        }

        refreshTags();

        //Load all the tags and display them as PlusMisBlocks
        for(int i=0; i < mTags.Count; i++) {
            MakeBlockForNewTag(mTags[i]);
        }
    }

    void refreshTags() {
        for (int i = 0; i < mSeries.Count; i++) {
            VideoSeries s = mSeries[i];
            for (int j = 0; j < s.Tags.Count; j++) {
                if (!mTags.Contains(s.Tags[j])) {
                    mTags.Add(s.Tags[j]);
                }
            }
        }
    }

    void OnSeriesButtonClicked(VideoSeries s) {
        //Update all the UI so it reflects the current series' values
        int index = -1;
        for (int i = 0; i < mSeries.Count; i++) {
            if (mSeries[i].Name.Equals(s.Name)) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            mCurrentlySelectedSeries = index;
        }

        if (NameEditText != null) {
            NameEditText.text = s.Name;
        }

        if (ChosenPathText != null) {
            ChosenPathText.SetText(s.FilePath);
        }


        RefreshTagsForCurrentSeries();
    }

    //TAGS
    public void _AddNewTagButtonClicked() {
        mTags.Add(NewTagInput.text);
        MakeBlockForNewTag(NewTagInput.text);
    }

    void MakeBlockForNewTag(string tag) {
        var gameObject = Instantiate(PlusMinusUIPrefab, AllTagsGrid.transform);
        gameObject.GetComponent<PlusMinusBlock>().Setup(onAddTagToSeriesClicked, null, tag);
    }

    private void RefreshTagsForCurrentSeries() {
        for (int i = 0; i < mTagsForCurrentSeries.Count; i++) {
            Destroy(mTagsForCurrentSeries[i]);
        }
        mTagsForCurrentSeries.Clear();

        VideoSeries s = mSeries[mCurrentlySelectedSeries];
        for (int i = 0; i < s.Tags.Count; i++) {
            GameObject obj = Instantiate(PlusMinusUIPrefab, CurrentTagsGrid.transform);
            var plusMinus = obj.GetComponent<PlusMinusBlock>();
            plusMinus.Setup(null, RemoveTagFromSeries, s.Tags[i]);
            mTagsForCurrentSeries.Add(obj);
        }
    }

    private void onAddTagToSeriesClicked(string tagToAdd) {
        var currentSeries = mSeries[mCurrentlySelectedSeries];
        if (!currentSeries.Tags.Contains(tagToAdd)) {
            currentSeries.Tags.Add(tagToAdd);
            RefreshTagsForCurrentSeries();
        }
    }

    private void RemoveTagFromSeries(string tagToRemove) {
        mSeries[mCurrentlySelectedSeries].Tags.Remove(tagToRemove);
        RefreshTagsForCurrentSeries();
    }

    //END TAGS

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
        mSeries.Add(s);
        mDisplaySeriesScript.MakeSeriesButton(s);
        OnSeriesButtonClicked(s);
    }

    public void _UpdateSeries() {
        VideoSeries s = mSeries[mCurrentlySelectedSeries];
        string oldName = s.Name;
        s.Name = NameEditText.text;
        s.FilePath = ChosenPathText.text;
        mDisplaySeriesScript.UpdateName(oldName, s.Name);
    }

    public void _SaveItemInfo() {
        string path = null;
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

    public void _ClearItemInfo() {
        mSeries.Clear();
        mDisplaySeriesScript.ClearSeries();
    }

    public void _OnBackToStartingSceneClicked() {
        SceneManager.LoadScene("StartingScene");
    }

}
