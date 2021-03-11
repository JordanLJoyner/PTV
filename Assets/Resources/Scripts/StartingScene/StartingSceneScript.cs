using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StartingSceneScript : MonoBehaviour
{
    public void _OnOpenSeriesBuilderClicked() {
        SceneManager.LoadScene("SeriesBuilder");
    }

    public void _OnOpenTheaterClicked() {
        SceneManager.LoadScene("PTV");
    }

    public void _OnChangeSettingsClicked() {
        SceneManager.LoadScene("SettingsScene");
    }

    public void _OnScheduleBuilderClicked() {
        SceneManager.LoadScene("ScheduleBuilder");
    }
}
