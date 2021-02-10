using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SettingsParentScript : MonoBehaviour {
    [SerializeField] private TMP_InputField m_ServerUrl;
    [SerializeField] private TMP_InputField m_ServerPort;
    [SerializeField] private TextMeshProUGUI m_MusicDirectory;
    [SerializeField] private TextMeshProUGUI m_BumpDirectory;

    private void Start() {
        TheaterSettings settings = FileUtils.LoadSettings();
        m_MusicDirectory.text = settings.musicDirectory;
        m_BumpDirectory.text = settings.bumpDirectory;
        m_ServerPort.text = settings.restServerPort;
        m_ServerUrl.text = settings.restServerUrl;
    }

    public void _OnSaveClicked() {
        TheaterSettings settings = new TheaterSettings();
        settings.musicDirectory = m_MusicDirectory.text;
        settings.bumpDirectory = m_BumpDirectory.text;
        settings.restServerPort = m_ServerPort.text;
        settings.restServerUrl = m_ServerUrl.text;

        FileUtils.SaveSettings(settings);
    }

    public void _OnStartingSceneClicked() {
        SceneManager.LoadScene("StartingScene");
    }
}
