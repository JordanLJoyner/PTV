using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExistingSeriesButtonScript : MonoBehaviour
{
    VideoSeries mSeriesData;
    OpenFileDialogScript.OnVideoSeriesSelected mClickCallbackDelegate;
    public Text ButtonText;

    public void setup(VideoSeries seriesData, OpenFileDialogScript.OnVideoSeriesSelected clickCallback) {
        mSeriesData = seriesData;
        mClickCallbackDelegate = clickCallback;
        ButtonText.text = seriesData.Name;
    }

    public void _OnClick() {
        if(mClickCallbackDelegate != null) {
            mClickCallbackDelegate(mSeriesData);
        }
    }
}
