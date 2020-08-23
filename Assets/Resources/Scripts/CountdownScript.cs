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
    public TextMeshProUGUI reminderText;
    public TextMeshProUGUI EventText;
    public UIParticleSystem uiParticleSystem;

    private List<AudioClip> musicFiles = new List<AudioClip>();
    private List<string> musicFilePaths = new List<string>();
    private List<string> ptvBumpFilePaths = new List<string>();    
    private int mVideoIndex = 0;
    private int mCountdownTimer = 0;
    private int mMusicIndex = 0;
    private bool mFiredAlmostComplete = false;
    private Action mQueuedPostLogoAction = null;
    private const string mMusicDirectory = "D:/Music/PTV";
    private const string mBumpDirectory = "D:/PTV/Bumps";

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
    private int preshowWait = 80;
    private int mTimeBetweenShowWait = 180;
    private float mMaxVolume = 1.0f;
    private string mSongName;

    private string mFilesFound = "";
    private List<string> mVideoFilePathsFound = new List<string>();
    private List<SaveDataItem> mSaveData = new List<SaveDataItem>();
    Dictionary<String, List<string>> mSeriesDict = new Dictionary<String, List<string>>();
    // Start is called before the first frame update
    void Start() {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        streamOverText.enabled = false;
        LoadMusic();
        LoadBumps();
        LoadVideos();
        ResetToCountdownState(preshowWait);
        StartCoroutine(StartServerTimeCountdown());
    }

    private void LoadVideos() {
        void RandomizeFilePaths() {
            //Randomize the file paths
            for (int i = 0; i < mVideoFilePathsFound.Count; i++) {
                int newIndex = UnityEngine.Random.Range(0, mVideoFilePathsFound.Count);
                string currentPath = mVideoFilePathsFound[i];
                mVideoFilePathsFound[i] = mVideoFilePathsFound[newIndex];
                mVideoFilePathsFound[newIndex] = currentPath;
            }
        }
        //Load the schedule
        Schedule schedule = FileUtils.LoadSchedule();
       
        var seriesData = FileUtils.LoadSeriesData();
        foreach (VideoSeries series in seriesData) {
            List<string> filePathsForSeries = new List<string>();
            FileUtils.FindAllFilesForPath(ref filePathsForSeries, series.FilePath);
            mSeriesDict.Add(series.Name, filePathsForSeries);
        }

        switch (schedule.scheduleType) {
            case ScheduleType.SEQUENTIAL:
                mSaveData = FileUtils.LoadSaveData();
                for (int i = 0; i < schedule.items.Count; i++) {
                    mVideoFilePathsFound.Add(mSeriesDict[schedule.items[i].showName][0]);
                }
                break;
            case ScheduleType.SCHEDULED_BUT_RANDOM_EPISODE:
                for (int i = 0; i < schedule.items.Count; i++) {
                    List<string> showsInSchedule = mSeriesDict[schedule.items[i].showName];
                    mVideoFilePathsFound.Add(showsInSchedule[UnityEngine.Random.Range(0, showsInSchedule.Count)]);
                }
                break;
            case ScheduleType.RANDOM_FROM_SHOWS:
                for (int i = 0; i < schedule.items.Count; i++) {
                    List<string> showsInSchedule = mSeriesDict[schedule.items[i].showName];
                    mVideoFilePathsFound.AddRange(showsInSchedule);
                }
                RandomizeFilePaths();
                break;
            case ScheduleType.DISTRIBUTED_RANDOM:
                //pick a show at random first, then pick a random episode from the list, repeat X times
                List<string> keyList = new List<string>(mSeriesDict.Keys);
                
                for (int i = 0; i < 100; i++) {
                    string randomKey = keyList[UnityEngine.Random.Range(0, keyList.Count)];
                    List<string> randomEpisodes = mSeriesDict[randomKey];
                    if(randomEpisodes.Count == 0) {
                        continue;
                    }
                    string randomEpisode = "";
                    try {
                        randomEpisode = randomEpisodes[UnityEngine.Random.Range(0, randomEpisodes.Count)];
                        mVideoFilePathsFound.Add(randomEpisode);
                    } catch (Exception e) {
                        Debug.Log("Series fucked up: " + randomKey);
                    }
                    
                }
                
                break;
            case ScheduleType.HARD_RANDOM:
            default:
                foreach (List<string> filePaths in mSeriesDict.Values) {
                    mVideoFilePathsFound.AddRange(filePaths);
                }
                RandomizeFilePaths();
                break;
        }
        mNextShowText = GetEpisodeNameFromPath(mVideoFilePathsFound[mVideoIndex]);
        UpdateCurrentShowText();
        UpdateScheduleScript();
    }

    private void UpdateScheduleScript() {
        List<string> itemNames = new List<string>();

        for (int i = 0; i < mVideoFilePathsFound.Count; i++) {
            itemNames.Add(GetEpisodeNameFromPath(mVideoFilePathsFound[i]));
        }
        scheduleScript.LoadSchedule(itemNames);
    }

    private string GetEpisodeNameFromPath(string fullPath) {
        int lastSlash = fullPath.LastIndexOf('\\');
        lastSlash++;
        int lastDot = fullPath.LastIndexOf('.');
        return fullPath.Substring(lastSlash, (lastDot - lastSlash));
    }

    private void LoadMusic() {
        musicFiles.Clear();
        //TODO pull this out of a JSON file instead of hardcoding it
        string directoryName = mMusicDirectory;
        var info = new DirectoryInfo(directoryName);
        if (!info.Exists) {
            Debug.LogError("No music directory found");
            return;
        }
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
        string bumpDirectory = mBumpDirectory;
        List<string> bumpFileNames = new List<string>();
        var info = new DirectoryInfo(bumpDirectory);
        if (!info.Exists) {
            Debug.LogError("No bump directory found");
            return;
        }
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
        mMusicIndex = UnityEngine.Random.Range(0, musicFilePaths.Count);
        string fileName = musicFilePaths[mMusicIndex];
        int slashIndex = fileName.LastIndexOf("/");
        int dotIndex = fileName.LastIndexOf(".");
        int fileTypelength = fileName.Length - dotIndex;
        mSongName = fileName.Substring(slashIndex+1, fileName.Length - (slashIndex + fileTypelength+1));
        StartCoroutine(RESTApiTest.UpdateSongOnServer(mSongName));

        //Stream the file in from our local storage
        AudioClip clp = new WWW("file:///" + fileName).GetAudioClip(false,true);
        musicFiles.Add(clp);
        musicFilePaths.RemoveAt(mMusicIndex);
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
        mCountdownTimer = numSeconds;
        StartCoroutine(DoCountdown());
        NextUpParent.SetActive(false);
        nextUpVideoTitleText.text = mVideoFilePathsFound[mVideoIndex];
    }

    public void _OnVolumeSliderChanged(float value) {
        if (volumeSlider.gameObject.activeSelf) {
            mMaxVolume = volumeSlider.value;
            musicPlayer.volume = volumeSlider.value;
        }
    }

    private string timeUntilNextEvent() {
        DateTime now = DateTime.Now;
        int hours = 0, minutes = 0, seconds = 0, totalSeconds = 0;
        hours = (24 - now.Hour) - 1;
        minutes = (60 - now.Minute) - 1;
        seconds = (60 - now.Second - 1);

        totalSeconds = seconds + (minutes * 60) + (hours * 3600);
        string returnVal = "Midnight in ";
        if(hours > 0) {
            returnVal += hours.ToString() + ":";
        }
        if(minutes > 0) {
            returnVal += minutes.ToString();
        }
        return returnVal;
    }

    private void Update() {
        if (state == eTVState.COUNTDOWN) {
            if (!musicPlayer.isPlaying) {
                PlayNextMusicClip();
            }
            //HACKS
            if (Input.GetKeyDown(KeyCode.P)) {
                PlayNextMusicClip();
            }

            if (Input.GetKeyDown(KeyCode.S)) {
                mCountdownTimer = 13;
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                musicPlayer.time = musicPlayer.clip.length - 10;
            }

            if (Input.GetKeyDown(KeyCode.V)) {
                //volumeSlider.gameObject.SetActive(!volumeSlider.gameObject.activeSelf);
                VetoNextShow();
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
        
        if (state == eTVState.PLAYBACK) {
            if (Input.GetKeyDown(KeyCode.X)) {
                SkipToEndOfPlayback();
            }

            //Video just ended
            if (playbackStarted) {
                if (!videoPlayer.isPlaying) {
                    CompletedShowPlayback();
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
        mFiredAlmostComplete = false;
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
        mFiredAlmostComplete = false;
        //Fade the music in
        StartCoroutine(FadeInMusic());
        songTitleText.enabled = true;
        streamOverText.enabled = true;
    }

    private IEnumerator DoCountdown() {
        while (mCountdownTimer > 0) {
            string timerText = (mCountdownTimer / 60).ToString() + ":";
            if(mCountdownTimer % 60 < 10) {
                timerText += "0";
            }
            timerText += (mCountdownTimer % 60).ToString();
            countdownText.text = timerText;
            yield return new WaitForSeconds(1);
            mCountdownTimer--;
            if(mCountdownTimer < 10 && mFiredAlmostComplete == false) {
                mFiredAlmostComplete = true;
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

    //****************
    //BUMP
    //****************
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
        StartContentPlayback();
    }
    //****************
    //END BUMP
    //****************

    //****************
    //PLAYBACK
    //****************
    private string mCurrentShowText = "";
    private string mNextShowText = "";
    private IEnumerator mServerTimeEnumerator;

    //#StartVideoPlayback
    //#Video Playback
    //#Playback
    void StartContentPlayback() {
        videoPlayer.Stop();
        //Start the subtitles
        //StartSubtitles(subFiles[videoIndex]);
        playbackLogo.gameObject.SetActive(true);
        videoPlayer.clip = null;
        UpdateCurrentShowText();
        videoPlayer.url = mVideoFilePathsFound[mVideoIndex++];
        StartCoroutine(RESTApiTest.UpdateShowOnServer(mCurrentShowText));
        if (mVideoIndex < mVideoFilePathsFound.Count) {
            mNextShowText = GetEpisodeNameFromPath(mVideoFilePathsFound[mVideoIndex]);
        } else {
            mNextShowText = "End of schedule";
        }
        // videoPlayer.clip = videoFiles[videoIndex++];
        videoPlayer.enabled = true;
        videoPlayer.Play();
        //This has to be called after the call to viddyP.play
        state = eTVState.PLAYBACK;
        StartCoroutine(StartReminderTextCountdown());
        UpdateTimeOnServer();
    }
    
    //StopVideoPlayback
    //StopShowPlayback
    private void CompletedShowPlayback() {
        //Stop the subtitles
        //StopSubtitles();
        StartCoroutine(RESTApiTest.UpdateShowOnServer(mNextShowText));
        playbackLogo.gameObject.SetActive(false);

        //Stop the server time countdown and update server time so it's empty again
        if (mServerTimeEnumerator != null) {
            StopCoroutine(mServerTimeEnumerator);
            mServerTimeEnumerator = null;
        }

        if (mVideoIndex >= mVideoFilePathsFound.Count) {
            mVideoIndex = 0;
            ResetToEndOfStreamState();
        } else {
            PlayBumpAndLogo(OnLogoPostRollComplete);
        }
        UpdateTimeOnServer();
    }

    void UpdateTimeOnServer() {
        if (state == eTVState.PLAYBACK) {
            string formattedTime = "Current show " + mCurrentShowText;
            if (videoPlayer.length == 0) {
                formattedTime += " just started";
            } else {
                int remainingTime = (int)(videoPlayer.length - videoPlayer.time);
                int minutes = remainingTime / 60;
                formattedTime += " ends in " + minutes.ToString() + " minutes";
            }
            StartCoroutine(RESTApiTest.UpdateTimeOnServer(formattedTime));
        } else {
            //TODO this will wind up lying for a minute if it fires during a postroll bump
            int minuteWait = (mCountdownTimer / 60);
            StartCoroutine(RESTApiTest.UpdateTimeOnServer("Next show is " + mNextShowText + " in " + minuteWait.ToString() + " minutes"));
        }
    }

    private bool mUpdateServerTime = true;
    private IEnumerator StartServerTimeCountdown() {
        UpdateTimeOnServer();
        while (mUpdateServerTime) {
            yield return new WaitForSeconds(60.0f);
            UpdateTimeOnServer();
        }
    }

    //****************
    //END PLAYBACK
    //****************

    private void VideoPlayer_prepareCompleted(VideoPlayer source) {
        double videoLength = source.length;
        Debug.Log(videoPlayer.url + " is " + videoLength.ToString() + " seconds ");
    }

    private IEnumerator StartReminderTextCountdown() {
        yield return new WaitForSeconds(300);
        if(state == eTVState.PLAYBACK) {
            playbackLogo.gameObject.SetActive(false);
            reminderText.gameObject.SetActive(true);
            string currentShowText = mCurrentShowText;
            reminderText.text = "You're watching\n" + currentShowText;
            yield return new WaitForSeconds(15);
            int counter = 0;
            
            do {
                int remainingTime = (int)(videoPlayer.length - videoPlayer.time);
                string formattedTime = "";
                int minutes = remainingTime / 60;
                int seconds = remainingTime % 60;
                formattedTime = minutes.ToString() + ":" + seconds.ToString();
                reminderText.text = "Next up:\n" +mNextShowText + "\nin: " + formattedTime.ToString();
                yield return new WaitForSeconds(1);
                counter++;
            } while (counter < 15);
            reminderText.gameObject.SetActive(false);
            playbackLogo.gameObject.SetActive(state == eTVState.PLAYBACK);
            StartCoroutine(StartReminderTextCountdown());
        }
    }

    void OnLogoPostRollComplete() {
        mQueuedPostLogoAction = null;
        scheduleScript.AdvanceSchedule();
        ResetToCountdownState(mTimeBetweenShowWait);
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

    private const int mServerScheduleLength = 5;
    private void UpdateCurrentShowText() {
        if(mVideoIndex < 0) {
            Debug.LogError("Video index was still set to 0, did we load videos right?");
            return;
        }
        mCurrentShowText = GetEpisodeNameFromPath(mVideoFilePathsFound[mVideoIndex]);
        StartCoroutine(RESTApiTest.UpdateShowOnServer(mCurrentShowText));
        List<string> upcomingShows = new List<string>();
        for(int i=0; i < mServerScheduleLength; i++) {
            if(mVideoIndex + i > mVideoFilePathsFound.Count) {
                continue;
            }

            if(mVideoIndex + i < mVideoFilePathsFound.Count) {
                upcomingShows.Add(GetEpisodeNameFromPath(mVideoFilePathsFound[mVideoIndex + i]));
            }
        }
        StartCoroutine(RESTApiTest.UpdateScheduleOnServer(upcomingShows));
    }

    private void SkipToEndOfPlayback() {
        if (state== eTVState.PLAYBACK) {
            videoPlayer.time = videoPlayer.length - 10;
        }
    }

    private void VetoNextShow() {
        if(state != eTVState.COUNTDOWN) {
            return;
        }
        scheduleScript.AdvanceSchedule();
        mVideoIndex++;
        UpdateCurrentShowText();
    }

    //****************
    //API RESPONSES
    //****************
    public void OnSkipRequestedFromServer() {
        SkipToEndOfPlayback();
    }

    public void OnVetoRequestedFromServer() {
        VetoNextShow();
    }

    public void OnEmoteRequested() {
        if(uiParticleSystem != null && !uiParticleSystem.IsPlaying) {
            uiParticleSystem.Play();
        }
    }

    public void OnInitialServerConnection() {
        //Send a song, show, and schedule notification to the server so the API works right
        StartCoroutine(RESTApiTest.UpdateSongOnServer(mSongName));
        StartCoroutine(RESTApiTest.UpdateShowOnServer(mCurrentShowText));
    }

    //TODO find a spot to call this, onDisable, onDestroy doesn't work
    public void OnStreamShutdown() {
        StartCoroutine(RESTApiTest.UpdateSongOnServer("No Active Stream"));
        StartCoroutine(RESTApiTest.UpdateShowOnServer("No Active Stream"));
    }

    public void OnRequestRequestedFromServer(string seriesName) {
        if(state == eTVState.COUNTDOWN) {
            if (mSeriesDict.ContainsKey(seriesName)) {
                //Pull a random episode from the series
                var showFilePaths = mSeriesDict[seriesName];
                var randomEpisode = showFilePaths[UnityEngine.Random.Range(0, showFilePaths.Count)];
                mVideoFilePathsFound.Insert(mVideoIndex,randomEpisode);
                UpdateCurrentShowText();
                UpdateScheduleScript();
                //TODO actually remove any other instance of this episode
            } else {
                Debug.LogError("Tried to pull a series that doesn't exist");
            }     
            
        } else {
            Debug.Log("Tried to request a show outside of countdown state");
        }
    }
    //****************
    //END API RESPONSES
    //****************
}
