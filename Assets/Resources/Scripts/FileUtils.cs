using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileUtils {
    private static List<string> mApprovedVideoTypes = new List<string>() { ".mp4", ".avi", ".m4v" };

    private static string ReadFromFile(string path) {
        string str = "";
        if (!File.Exists(path)) {
            return "";
        }
        using (FileStream fs = new FileStream(path, FileMode.Open)) {
            using (StreamReader reader = new StreamReader(fs)) {
                str += reader.ReadToEnd();
            }
        }
        return str;
    }

    public static List<SaveDataItem> LoadSaveData() {
        string path = Application.streamingAssetsPath + "/Savedata.json";
        string json = ReadFromFile(path);
        return new List<SaveDataItem>(JsonHelper.FromJson<SaveDataItem>(json));
    }

    public static void WriteSaveData(List<SaveDataItem> saveData) {
        string path = null;
        path = Application.streamingAssetsPath + "/Savedata.json";
        using (FileStream fs = new FileStream(path, FileMode.Truncate)) {
            using (StreamWriter writer = new StreamWriter(fs)) {
                string temp = JsonHelper.ToJson<SaveDataItem>(saveData.ToArray(), true);
                writer.Write(temp);
            }
        }
        #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    public static Schedule LoadSchedule() {
        string path = Application.streamingAssetsPath + "/Schedule.json";
        string json = ReadFromFile(path);
        if(json == "") {
            return null;
        }
        return JsonUtility.FromJson<Schedule>(json);
    }

    private static string mSettingsFileName = "/Settings.json";
    private static string mSettingsLocalHostFileName = "/LocalhostServerSettings.json";
    private static string mSettingsPTVFileName = "/PTVServerSettings.json";
    public enum eSettingsType {
        DEFAULT,
        LOCAL_HOST,
        PTV
    }
    public static TheaterSettings LoadSettings() {
        return LoadSettings(eSettingsType.DEFAULT);
    }
    public static TheaterSettings LoadSettings(eSettingsType eSettings) {
        string settingsFile = mSettingsFileName;
        switch (eSettings) {
            case eSettingsType.LOCAL_HOST:
                settingsFile = mSettingsLocalHostFileName;
                break;
            case eSettingsType.PTV:
                settingsFile = mSettingsPTVFileName;
                break;
            default:
                settingsFile = mSettingsFileName;
                break;
        }
        string path = Application.streamingAssetsPath + settingsFile;
        string json = ReadFromFile(path);
        if (json == "") {
            return null;
        }
        return JsonUtility.FromJson<TheaterSettings>(json);
    }

    public static void SaveSettings(TheaterSettings settings) {
        string path = Application.streamingAssetsPath + mSettingsFileName;
        string settingsJson = JsonUtility.ToJson(settings);
        using (FileStream fs = new FileStream(path, FileMode.Truncate)) {
            using (StreamWriter writer = new StreamWriter(fs)) {
                writer.Write(settingsJson);
            }
        }
    }

    public static VideoSeries[] LoadSeriesData() {
        string path = Application.streamingAssetsPath + "/Series/SeriesInfo.json";
        string str = "";
        using (FileStream fs = new FileStream(path, FileMode.Open)) {
            using (StreamReader reader = new StreamReader(fs)) {
                str += reader.ReadToEnd();
            }
            VideoSeries[] vidArray = JsonHelper.FromJson<VideoSeries>(str);
            return vidArray;
        }
    }

    public static void FindAllFilesForPath(ref List<string> filePathsFound, string path) {
        var info = new DirectoryInfo(path);
        if (!info.Exists) {
            Debug.LogError("File path " + path + " not found");
            return;
        }
        var directories = info.GetDirectories();
        foreach (var directory in directories) {
            FindAllFilesForPath(ref filePathsFound, directory.FullName);
        }
        var files = info.GetFiles();
        foreach (FileInfo file in files) {
            foreach (string approvedVideoType in mApprovedVideoTypes) {
                if (file.Name.Contains(approvedVideoType)) {
                    filePathsFound.Add(file.FullName);
                    break;
                }
            }
        }
    }
}
