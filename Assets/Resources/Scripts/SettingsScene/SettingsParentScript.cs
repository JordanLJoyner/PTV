using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SettingsParentScript : MonoBehaviour {
    [SerializeField] private TMP_InputField m_ServerUrl;
    [SerializeField] private TMP_InputField m_ServerPort;
    [SerializeField] private TMP_InputField m_StreamUrl;
    [SerializeField] private TMP_InputField m_TheaterName;
    [SerializeField] private TextMeshProUGUI m_MusicDirectory;
    [SerializeField] private TextMeshProUGUI m_BumpDirectory;
    
    private void Start() {
        LoadSettings(FileUtils.LoadSettings());
    }

    public void _OnSaveClicked() {
        TheaterSettings settings = new TheaterSettings();
        settings.musicDirectory = m_MusicDirectory.text;
        settings.bumpDirectory = m_BumpDirectory.text;
        settings.restServerPort = m_ServerPort.text;
        settings.restServerUrl = m_ServerUrl.text;
        settings.streamUrl = m_StreamUrl.text;
        settings.theaterName = m_TheaterName.text;
        FileUtils.SaveSettings(settings);
    }

    private void LoadSettings(TheaterSettings settings) {
        m_MusicDirectory.text = settings.musicDirectory;
        m_BumpDirectory.text = settings.bumpDirectory;
        m_ServerPort.text = settings.restServerPort;
        m_ServerUrl.text = settings.restServerUrl;
        m_StreamUrl.text = settings.streamUrl;
        m_TheaterName.text = settings.theaterName;
    }

    public void _OnLoadLocalHostSettingsClicked() {
        LoadSettings(FileUtils.LoadSettings(FileUtils.eSettingsType.LOCAL_HOST));
    }

    public void _OnLoadPTVSettingsClicked() {
        LoadSettings(FileUtils.LoadSettings(FileUtils.eSettingsType.PTV));
    }

    public void _OnStartingSceneClicked() {
        SceneManager.LoadScene("StartingScene");
    }
}
