using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileUtils  {
    private static List<string> mApprovedVideoTypes = new List<string>() { ".mp4", ".avi" };

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
