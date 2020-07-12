using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.IO;
using System;

public class CountdownScript : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI songTitleText;
    public TextMeshProUGUI streamOverText;
    public TextMeshProUGUI nextUpVideoTitleText;
    public TextMeshProUGUI playbackLogo;
    public GameObject NextUpParent;
    public AudioSource musicPlayer;
    public VideoPlayer videoPlayer;
    public Image scrimPanel;
    public LogoScript mLogoScript;
    public ScheduleDisplayScript scheduleScript;
    public GameObject VisualizerParent;
    public Slider volumeSlider;
    public SubtitleDisplayer subtitleDisplayer;

    public List<VideoClip> videoFiles;
    public List<TextAsset> subFiles;

    private List<AudioClip> musicFiles = new List<AudioClip>();
    private List<string> musicFilePaths = new List<string>();
    private List<string> ptvBumpFilePaths = new List<string>();    
    private int videoIndex = 0;
    private int countdownTimer = 0;
    private int musicIndex = 0;
    private bool firedAlmostComplete = false;
    private Action mQueuedPostLogoAction = null;

    private enum eTVState {
        UNSET,
        COUNTDOWN,
        TRANSITIONING,
        PLAYBACK,
        LOGO,
        BUMP
    }
    private eTVState state;
    private bool playbackStarted = false;
    private bool bumpPlaybackStarted = false;
    private bool mLoadedAllMusicTracks = false;
    private int preshowWait = 1500;
    private int mMidShowWait = 180;
    private float mMaxVolume = 1.0f;
    private string mSongName;

    private string mFilesFound = "";
    private List<string> mVideoFilePathsFound = new List<string>();

    // Start is called before the first frame update
    void Start() {
        streamOverText.enabled = false;
        LoadMusic();
        LoadBumps();
        List<string> itemNames = new List<string>();
        var seriesData = FileUtils.LoadSeriesData();
        foreach(VideoSeries series in seriesData) {
            FileUtils.FindAllFilesForPath(ref mVideoFilePathsFound, series.FilePath);
        }

        for (int i=0; i < videoFiles.Count; i++) {
            if(videoFiles[i] != null) {
                itemNames.Add(videoFiles[i].name);
            }
        }
        scheduleScript.LoadSchedule(itemNames);
        ResetToCountdownState(preshowWait);
    }

    private void LoadMusic() {
        musicFiles.Clear();
        //TODO pull this out of a JSON file instead of hardcoding it
        string directoryName = "D:/Music/PTV";
        var info = new DirectoryInfo(directoryName);
        var files = info.GetFiles();
        foreach(FileInfo file in files) {
            if (!file.Name.Contains(".meta") && file.Name.Contains(".ogg")) {
                musicFilePaths.Add(directoryName + "/" + file.Name);
            }
        }
        //AudioClip[] audio = Resources.LoadAll<AudioClip>("Music");
        //musicFiles.AddRange(audio);
        //musicIndex = Random.Range(0,musicFiles.Count);
    }

    private void LoadBumps() {
        musicFiles.Clear();
        string bumpDirectory = "C:/Users/Jorda/Videos/PTV/Bumps";
        List<string> bumpFileNames = new List<string>();
        FileUtils.FindAllFilesForPath(ref ptvBumpFilePaths, bumpDirectory);
        //var info = new DirectoryInfo(Application.dataPath + "/Resources/Bumps/Random Bumps");
        //var files = info.GetFiles();
        //foreach (FileInfo file in files) {
        //    if (!file.Name.Contains(".meta")) {
        //        randomBumpFilePaths.Add(file.Name);
        //    }
       //}
    }

    private void PullUpNextTrack() {
        if(musicFiles.Count >= musicFilePaths.Count) {
            mLoadedAllMusicTracks = true;
            return;
        }
        musicIndex = UnityEngine.Random.Range(0, musicFilePaths.Count);
        string fileName = musicFilePaths[musicIndex];
        int slashIndex = fileName.LastIndexOf("/");
        int dotIndex = fileName.LastIndexOf(".");
        int fileTypelength = fileName.Length - dotIndex;
        mSongName = fileName.Substring(slashIndex+1, fileName.Length - (slashIndex + fileTypelength+1));
        //Stream the file in from our loca storage
        AudioClip clp = new WWW("file:///" + fileName).GetAudioClip(false,true);//Resources.Load<AudioClip>("Music/" + fileName);
        musicFiles.Add(clp);
        musicFilePaths.RemoveAt(musicIndex);
    }

    void PlayNextMusicClip() {
        PullUpNextTrack();
        if (mLoadedAllMusicTracks) {
            musicPlayer.clip = musicFiles[UnityEngine.Random.Range(0, musicFiles.Count)];
        } else {
            musicPlayer.clip = musicFiles[musicFiles.Count - 1];
        }
        musicPlayer.time = 20;
        musicPlayer.Play();
        StartCoroutine(SongSkipTimer());
        songTitleText.text = mSongName;
    }

    private IEnumerator SongSkipTimer() {
        string name = musicPlayer.clip.name;
        yield return new WaitForSeconds(120);
        void TransitionSongs() {
            StartCoroutine(FadeInMusic());
        }
        if(state == eTVState.COUNTDOWN && name == musicPlayer.clip.name) {
            StartCoroutine(FadeOutMusic(TransitionSongs));
        }
    }

    

    public void StartCountdown(int numSeconds) {
        state = eTVState.COUNTDOWN;
        VisualizerParent.SetActive(true);
        countdownTimer = numSeconds;
        StartCoroutine(DoCountdown());
        NextUpParent.SetActive(false);
        nextUpVideoTitleText.text = videoFiles[videoIndex].name;
    }

    public void _OnVolumeSliderChanged(float value) {
        if (volumeSlider.gameObject.activeSelf) {
            mMaxVolume = volumeSlider.value;
            musicPlayer.volume = volumeSlider.value;
        }
    }

    private void Update() {
        if(state == eTVState.COUNTDOWN) {
            if (!musicPlayer.isPlaying) {
                PlayNextMusicClip();
            }
            //HACKS
            if (Input.GetKeyDown(KeyCode.P)) {
                PlayNextMusicClip();
            }

            if (Input.GetKeyDown(KeyCode.S)) {
                countdownTimer = 13;
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                musicPlayer.time = musicPlayer.clip.length - 10;
            }

            if (Input.GetKeyDown(KeyCode.V)) {
                volumeSlider.gameObject.SetActive(!volumeSlider.gameObject.activeSelf);
            }
        } else {
            volumeSlider.gameObject.SetActive(false);
        }

        if(state == eTVState.BUMP) {

            if (videoPlayer.isPlaying) {
                bumpPlaybackStarted = true;
            }
            //Video just ended
            if (bumpPlaybackStarted) {
                if (!videoPlayer.isPlaying) {
                    OnBumpCompleted();
                }
            }
        }
        
        playbackLogo.gameObject.SetActive(state == eTVState.PLAYBACK);
        if (state == eTVState.PLAYBACK) {
            if (Input.GetKeyDown(KeyCode.X)) {
                videoPlayer.time = videoPlayer.length - 10;
            }

            //Video just ended
            if (playbackStarted) {
                if (!videoPlayer.isPlaying) {
                    //Stop the subtitles
                    StopSubtitles();
                    if (videoIndex >= videoFiles.Count) {
                        videoIndex = 0;
                        ResetToEndOfStreamState();
                    } else {
                        PlayBumpAndLogo(OnLogoPostRollComplete);
                    }
                }
            }

            if (videoPlayer.isPlaying) {
                playbackStarted = true;
            }
        }
    }

    void ResetToCountdownState(int countdownLength) {
        scheduleScript.ShowSchedule();
        playbackStarted = false;
        firedAlmostComplete = false;
        videoPlayer.enabled = false;
        //Fade in the scrim
        StartCoroutine(TriggerScrimFadeIn());
        //Reset the countdown
        StartCountdown(countdownLength);
        //Fade the music in
        StartCoroutine(FadeInMusic());
        ShowTextFields();
    }

    void ResetToEndOfStreamState() {
        playbackStarted = false;
        firedAlmostComplete = false;
        //Fade the music in
        StartCoroutine(FadeInMusic());
        songTitleText.enabled = true;
        streamOverText.enabled = true;
    }

    private IEnumerator DoCountdown() {
        while (countdownTimer > 0) {
            string timerText = (countdownTimer / 60).ToString() + ":";
            if(countdownTimer % 60 < 10) {
                timerText += "0";
            }
            timerText += (countdownTimer % 60).ToString();
            countdownText.text = timerText;
            yield return new WaitForSeconds(1);
            countdownTimer--;
            if(countdownTimer < 10 && firedAlmostComplete == false) {
                firedAlmostComplete = true;
                OnCountdownAlmostComplete();
            }
        }
        OnCountdownComplete();
    }

    private void OnCountdownAlmostComplete() {
        state = eTVState.TRANSITIONING;
        StartCoroutine(FadeOutMusic(musicPlayer.Stop));
        StartCoroutine(TriggerScrimFadeOut());
    }

    private void OnCountdownComplete() {
        PlayPrerollBump();
        HideTextFields();
    }

    private void ShowTextFields() {
        songTitleText.enabled = true;
        countdownText.enabled = true;
    }

    private void HideTextFields() {
        songTitleText.enabled = false;
        countdownText.enabled = false;
    }

    private IEnumerator TriggerScrimFadeIn() {
        scrimPanel.enabled = true;
        Color temp = scrimPanel.color;
        temp.a = 1;
        scrimPanel.color = temp;
        while (scrimPanel.color.a > 0) {
            var newColor = scrimPanel.color;
            newColor.a -= 0.025f;
            scrimPanel.color = newColor;
            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator TriggerScrimFadeOut() {
        while(scrimPanel.color.a < 1) {
            var newColor = scrimPanel.color;
            newColor.a += 0.025f;
            scrimPanel.color = newColor;
            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator FadeInMusic() {
        PlayNextMusicClip();
        musicPlayer.volume = 0;
        while (musicPlayer.volume < mMaxVolume) {
            musicPlayer.volume += 0.1f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator FadeOutMusic(Action MusicFadeCompleteCallback) {
        while(musicPlayer.volume > 0) {
            musicPlayer.volume -= 0.05f;
            yield return new WaitForSeconds(0.5f);
        }
        if(MusicFadeCompleteCallback != null) {
            MusicFadeCompleteCallback();
        }
    }

    void PlayPrerollBump() {
        NextUpParent.SetActive(false);
        videoPlayer.enabled = true;
        scrimPanel.enabled = false;
        musicPlayer.Stop();
        PlayBumpAndLogo(OnLogoPrerollComplete);
    }

    void PlayBumpAndLogo(Action postLogoAction) {
        VisualizerParent.SetActive(false);
        scheduleScript.HideSchedule();
        state = eTVState.BUMP;
        bumpPlaybackStarted = false;
        videoPlayer.Stop();
        if (ptvBumpFilePaths.Count > 0) {
            int bumpIndex = UnityEngine.Random.Range(0, ptvBumpFilePaths.Count);
            string fileName = ptvBumpFilePaths[bumpIndex];
            fileName = fileName.Substring(0, fileName.LastIndexOf("."));
            //VideoClip clp = Resources.Load<VideoClip>("Bumps/Random Bumps/" + fileName);
            videoPlayer.url = ptvBumpFilePaths[bumpIndex];
            videoPlayer.enabled = true;
            //videoPlayer.clip = clp;
            videoPlayer.Play();
            mQueuedPostLogoAction = postLogoAction;
            ptvBumpFilePaths.RemoveAt(bumpIndex);
        } else {
            //play the bump sound effects
            mLogoScript.PlayLogo(OnLogoPrerollComplete);
        }
    }

    void OnBumpCompleted() {
        state = eTVState.LOGO;
        videoPlayer.enabled = false;
        //play the bump sound effects
        mLogoScript.PlayLogo(mQueuedPostLogoAction);
    }

    void OnLogoPrerollComplete() {
        mQueuedPostLogoAction = null;
        videoPlayer.Stop();
        //Start the subtitles
        StartSubtitles(subFiles[videoIndex]);
        
        videoPlayer.clip = null;
        videoPlayer.url = mVideoFilePathsFound[0];
        // videoPlayer.clip = videoFiles[videoIndex++];
        videoPlayer.enabled = true;
        videoPlayer.Play();
        //This has to be called after the call to viddyP.play
        state = eTVState.PLAYBACK;
    }

    void OnLogoPostRollComplete() {
        mQueuedPostLogoAction = null;
        scheduleScript.AdvanceSchedule();
        ResetToCountdownState(mMidShowWait);
    }

    private void StartSubtitles(TextAsset assetToSub) {
        subtitleDisplayer.gameObject.SetActive(true);
        subtitleDisplayer.Subtitle = assetToSub;
        subtitleDisplayer.StopSubs();
        if (assetToSub != null) {
            StartCoroutine(subtitleDisplayer.Begin());
        }
    }

    private void StopSubtitles() {
        subtitleDisplayer.StopSubs();
        subtitleDisplayer.gameObject.SetActive(false);
    }
}
