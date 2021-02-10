using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.IO;
using UnityEngine.Networking;

[Serializable]
public class ServerMessage {
    public string MessageType;
    public string Data;
}

[Serializable]
public class ServerMessageBundle {
    public List<ServerMessage> values = new List<ServerMessage>();
}

[Serializable]
public class ServerRoom {
    public string name;
    public string theater_name;
    public string url;
    public int id;
    public int viewers;
    public string current_show;
    public string series;
    public string status;

    public ServerRoom(string name, string url, int id, int viewers, string current_show, string series) {
        this.theater_name = name;
        this.url = url;
        this.id = id;
        this.viewers = viewers;
        this.current_show = current_show;
        this.series = series;
        this.status = RESTApiTest.STATUS_AVAILABLE;
    }
}

public class RESTApiTest : MonoBehaviour {
    [SerializeField] private TMPro.TextMeshProUGUI serverIdField;

    public CountdownScript countdownScript;
    private static string mBaseURL = "http://127.0.0.1";
    private static string mPortNumber = ":5000";
    private static string mServerPrefix = "/PTV";

    public static int mRoomId = -1;
    public static string STATUS_AVAILABLE = "available";
    public static string STATUS_BUSY = "available";
    public static string STATUS_PLAYING = "available";

    private string mSeriesString = "";
    private bool mQueryMessageQueue = true;
    private bool mInitialConnection = true;
    private bool mStartedCleanUp = false;
    private bool mCleanupComplete = false;

    // Start is called before the first frame update
    void Start() {
        var settings = FileUtils.LoadSettings();
        mPortNumber = ":"+settings.restServerPort.ToString();
        if (!settings.restServerUrl.Equals("")) {
            mBaseURL = settings.restServerUrl;
        }
        Application.wantsToQuit += OnQuitIncoming;
        SetSeriesData();

        StartCoroutine(StartRequestingMessageQueue());
        StartCoroutine(GetRoomId());
    }

    private void SetSeriesData() {
        var seriesData = FileUtils.LoadSeriesData();
        var seriesDict = new Dictionary<String, List<string>>();
        string seriesString = "";
        foreach (VideoSeries series in seriesData) {
            List<string> filePathsForSeries = new List<string>();
            FileUtils.FindAllFilesForPath(ref filePathsForSeries, series.FilePath);
            seriesDict.Add(series.Name, filePathsForSeries);
            seriesString += series.Name + '\n';
        }
        mSeriesString = seriesString;
        //string t = mBaseURL + mPortNumber + mServerPrefix + getRoomPrefix();
        //StartCoroutine(UpdateOnServer(mBaseURL + mPortNumber + mServerPrefix + getRoomPrefix() + "/series/", "series_list", seriesString));
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
           // StartCoroutine(CleanupRoom());
        }
    }
    /*
    IEnumerator Upload() {
        WWWForm form = new WWWForm();
        var seriesData = FileUtils.LoadSeriesData();
        form.AddField("series_list", JsonHelper.ToJson<VideoSeries>(seriesData));

        using (UnityWebRequest www = UnityWebRequest.Post(, form)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                Debug.Log("Form upload complete!");
            }
        }
    }*/

    private static IEnumerator UpdateOnServer(string endPoint, string fieldName, string value) {
        WWWForm form = new WWWForm();
        form.AddField(fieldName, value);

        using (UnityWebRequest www = UnityWebRequest.Post(endPoint, form)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error + " for " + endPoint);
            } else {
                //Debug.Log("Form upload complete!");
            }
        }
    }

    private static IEnumerator DeleteTheaterRoomOnServer(string endPoint) {
        Debug.Log("starting delete on server using endpoint: " + endPoint);
        mRoomId = -1;
        using (UnityWebRequest www = UnityWebRequest.Delete(endPoint)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error + " for " + endPoint);
            } else {
                Debug.Log("Called delete on server");
            }
        }
    }

    public static IEnumerator UpdateSongOnServer(string songName) {
        yield return UpdateOnServer(mBaseURL+ mPortNumber + mServerPrefix + "/song/","SongName",songName);
    }

    public static IEnumerator UpdateShowOnServer(string showName) {
        yield return UpdateOnServer(mBaseURL + mPortNumber + mServerPrefix + "/show/", "ShowName", showName);
    }

    public static IEnumerator UpdateScheduleOnServer(List<string> schedule) {
        yield return UpdateOnServer(mBaseURL + mPortNumber + mServerPrefix + getRoomPrefix() + "/schedule/", "Schedule", JsonHelper.ToJson<string>(schedule.ToArray()));
    }

    public static IEnumerator UpdateTimeOnServer(string timeRemaining) {
        yield return UpdateOnServer(mBaseURL + mPortNumber + mServerPrefix + "/time/", "TimeLeft", timeRemaining);
    }

    private IEnumerator GetRoomId() {
        yield return new WaitForSeconds(0.1f);
        yield return GetFromServer(mBaseURL + mPortNumber + mServerPrefix + "/rooms/newid", OnRoomIdReceived);
    }

    private void OnRoomIdReceived(string value) {
        mRoomId = int.Parse(value);
        Debug.Log("Room Id will be " + mRoomId.ToString());
        if(serverIdField != null) {
            serverIdField.text = "Server Id: " + value;
        }
        CreateAvailableRoomOnServer();
    }

    private void CreateAvailableRoomOnServer() {
        ServerRoom thisRoom = new ServerRoom("Jordan's Home PC", "https://content.jwplatform.com/manifests/Y5UQq0fG.m3u8", mRoomId,0,"Unclear", mSeriesString);
        StartCoroutine(UpdateOnServer(mBaseURL + mPortNumber + mServerPrefix + "/rooms/", "room",JsonUtility.ToJson(thisRoom)));
        
    }

    IEnumerator GetFromServer(string uri, Action<string> onServerResponse) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                mQueryMessageQueue = false;
                StartCoroutine(TryToReconnectToServer());
            } else {
                if (mInitialConnection) {
                    mInitialConnection = false;
                    countdownScript.OnInitialServerConnection();
                }
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                
                onServerResponse?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    IEnumerator TryToReconnectToServer() {
        yield return new WaitForSeconds(30.0f);
        mQueryMessageQueue = true;
        mInitialConnection = true;
        StartCoroutine(StartRequestingMessageQueue());
    }

    IEnumerator StartRequestingMessageQueue() {
        while (mQueryMessageQueue) {
            yield return GetFromServer(mBaseURL + mPortNumber + "/PTVMessageQueue/", OnMessageQueueReceived);
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnMessageQueueReceived(string serverJson) {
        string JSONToParse = "{\"values\":" + serverJson + "}";
        var serverMessages = JsonUtility.FromJson<ServerMessageBundle>(JSONToParse);
        processServerMessages(serverMessages);
    }

    void processServerMessages(ServerMessageBundle serverMessages) {
        if (countdownScript == null) {
            return;
        }
        for(int i=0; i < serverMessages.values.Count; i++) {
            string message = serverMessages.values[i].MessageType;
            switch(message){
                case "SKIP":
                    Debug.Log("Skip requested from server");
                    countdownScript.OnSkipRequestedFromServer();
                    break;
                case "VETO":
                   Debug.Log("Veto requested from server");
                   countdownScript.OnVetoRequestedFromServer(); 
                   break;
                case "EMOTE_WTF":
                    countdownScript.OnEmoteRequested();
                    break;
                case "REQUEST":
                    countdownScript.OnRequestRequestedFromServer(serverMessages.values[i].Data);
                    break;
                case "START":
                    countdownScript.OnStartRequestFromServer(serverMessages.values[i].Data);
                    break;
                case "PLAY":
                    countdownScript.OnPlayRequestFromServer();
                    break;
                case "PAUSE":
                    countdownScript.OnPauseRequestFromServer();
                    break;
                default:
                    break;
            }
        }
    }

    private bool OnQuitIncoming() {
        if (!mStartedCleanUp) {
            StartCoroutine(CleanupRoom());
        }
        return mCleanupComplete;
    }

    private static string getRoomPrefix() {
        return "/room/" + mRoomId.ToString();
    }

    //When we're shutting down clean up our id on the server
    private IEnumerator CleanupRoom() {
        mStartedCleanUp = true;
        if (mRoomId > -1) {
            Debug.Log("Destroy id: " + mRoomId.ToString() + " on the server");
            yield return DeleteTheaterRoomOnServer(mBaseURL + mPortNumber +  mServerPrefix + getRoomPrefix());
        }
        mCleanupComplete = true;
        Application.Quit();
    }
}
