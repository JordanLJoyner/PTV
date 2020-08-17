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
}

[Serializable]
public class ServerMessageBundle {
    public List<ServerMessage> values = new List<ServerMessage>();
}

public class RESTApiTest : MonoBehaviour {
    public CountdownScript countdownScript;

    // Start is called before the first frame update
    void Start() {
        var seriesData = FileUtils.LoadSeriesData();
        var seriesDict = new Dictionary<String, List<string>>();
        foreach (VideoSeries series in seriesData) {
            List<string> filePathsForSeries = new List<string>();
            FileUtils.FindAllFilesForPath(ref filePathsForSeries, series.FilePath);
            seriesDict.Add(series.Name, filePathsForSeries);
        }

        StartCoroutine(Upload());
        StartCoroutine(StartRequestingMessageQueue());
    }

    IEnumerator Upload() {
        WWWForm form = new WWWForm();
        var seriesData = FileUtils.LoadSeriesData();
        form.AddField("series_list", JsonHelper.ToJson<VideoSeries>(seriesData));

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/series/", form)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                Debug.Log("Form upload complete!");
            }
        }
    }

    private static IEnumerator UpdateOnServer(string endPoint, string fieldName, string value) {
        WWWForm form = new WWWForm();
        var seriesData = FileUtils.LoadSeriesData();
        form.AddField(fieldName, value);

        using (UnityWebRequest www = UnityWebRequest.Post(endPoint, form)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                //Debug.Log("Form upload complete!");
            }
        }
    }

    public static IEnumerator UpdateSongOnServer(string songName) {
        yield return UpdateOnServer("http://127.0.0.1:5000/PTV/song/","SongName",songName);
    }

    public static IEnumerator UpdateShowOnServer(string showName) {
        yield return UpdateOnServer("http://127.0.0.1:5000/PTV/show/", "ShowName", showName);
    }

    public static IEnumerator UpdateScheduleOnServer(List<string> schedule) {
        yield return UpdateOnServer("http://127.0.0.1:5000/PTV/schedule/", "Schedule", JsonHelper.ToJson<string>(schedule.ToArray()));
    }

    private bool mQueryMessageQueue = true;
    private bool mInitialConnection = true;
    IEnumerator GetMessageQueue(string uri) {
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
                string JSONToParse = "{\"values\":" + webRequest.downloadHandler.text + "}";
                var serverMessages = JsonUtility.FromJson<ServerMessageBundle>(JSONToParse);
                processServerMessages(serverMessages);
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
            yield return GetMessageQueue("http://127.0.0.1:5000/PTVMessageQueue/");
            yield return new WaitForSeconds(1.0f);
        }
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
                default:
                    break;
            }
        }
    }
}
