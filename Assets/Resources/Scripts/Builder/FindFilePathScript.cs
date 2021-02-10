using UnityEditor;
using UnityEngine;
using TMPro;
using SFB;

public class FindFilePathScript : MonoBehaviour {
    public delegate void OnFilePathFound(string chosenFilePath);
    public event OnFilePathFound OnFilePathFoundEvent;
    public TextMeshProUGUI FilePathText;
    private string lastKnownPath = "";

    
    public void SetDefaultPath(string defaultPath) {
        lastKnownPath = defaultPath;
    }

    public void _OnFindFilePathClicked() {
        WriteResult(StandaloneFileBrowser.OpenFolderPanel("Select Folder", lastKnownPath, true));
#if UNITY_EDITOR
        /*
        string path = EditorUtility.OpenFolderPanel("", lastKnownPath, "");
        if (path != "" && OnFilePathFoundEvent != null) {
            OnFilePathFoundEvent(path);
            lastKnownPath = path;
        }
        */
#endif
    }

    public void WriteResult(string[] paths) {
        string path = paths[0];// EditorUtility.OpenFolderPanel("", lastKnownPath, "");
        if (path != "" && OnFilePathFoundEvent != null) {
            OnFilePathFoundEvent(path);
            lastKnownPath = path;
        }
    }
}
